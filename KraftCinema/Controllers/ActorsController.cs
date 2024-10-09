using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KraftCinema.Data;
using KraftCinema.Models;
using System.Text.Json;
using System.Web;
using System.Net;
using VaderSharp2;

namespace KraftCinema.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;

		public async Task<IActionResult> GetActorPhoto(int id)
		{
			var actor = await _context.Actor.FirstOrDefaultAsync(m => m.Id == id);

			if (actor == null)
			{
				return NotFound();
			}

			var imageData = actor.Photo;

			return File(imageData, "image/jpg");
		}

		public ActorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Actor.ToListAsync());
        }

        public static async Task<List<string>> SearchRedditAsync(string searchQuery)
        {
            var returnList = new List<string>();
            var json = "";
            using (HttpClient client = new HttpClient())
            {
                //fake like you are a "real" web browser
                client.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                json = await client.GetStringAsync("https://www.reddit.com/search.json?limit=100&q=" + HttpUtility.UrlEncode(searchQuery));
            }
            var textToExamine = new List<string>();
            JsonDocument doc = JsonDocument.Parse(json);
            // Navigate to the "data" object
            JsonElement dataElement = doc.RootElement.GetProperty("data");
            // Navigate to the "children" array
            JsonElement childrenElement = dataElement.GetProperty("children");
            foreach (JsonElement child in childrenElement.EnumerateArray())
            {
                if (child.TryGetProperty("data", out JsonElement data))
                {
                    if (data.TryGetProperty("selftext", out JsonElement selftext))
                    {
                        string selftextValue = selftext.GetString();
                        if (!string.IsNullOrEmpty(selftextValue)) { returnList.Add(selftextValue); }
                        else if (data.TryGetProperty("title", out JsonElement title)) //use title if text is empty
                        {
                            string titleValue = title.GetString();
                            if (!string.IsNullOrEmpty(titleValue)) { returnList.Add(titleValue); }
                        }
                    }
                }
            }
            return returnList;
        }
        public static string CategorizeSetiment(double sentiment)
        {
            if (sentiment >= -1 && sentiment < -0.6)
            {
                return "Extremely Negative";
            }
            else if (sentiment >= -0.6 && sentiment < -0.2)
            {
                return "Very Negative";
            }
            else if (sentiment >= -0.2 && sentiment < 0)
            {
                return "Slightly Negative";
            }
            else if (sentiment >= 0 && sentiment < 0.2)
            {
                return "Slightly Positive";
            }
            else if (sentiment >= 0.2 && sentiment < 0.6)
            {
                return "Very Positive";
            }
            else if (sentiment >= 0.6 && sentiment <= 1)
            {
                return "Extremely Positive";
            }
            else
            {
                return "Invalid sentiment Value";
            }
        }


        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            ActorDetailsVM aVM = new ActorDetailsVM();
            aVM.actor = actor;

            var movies = new List<Movie>();

            movies = await (from m in _context.Movie
                            join ma in _context.MovieActor on m.Id equals ma.MovieID
                            where ma.ActorID == id
                            select m).ToListAsync();
            aVM.movies = movies;

            var queryText = actor.Name;
            var json = "";

            using (WebClient wc = new WebClient())
            {
                //fake like you are a "real" web browser
                wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                json = wc.DownloadString("https://www.reddit.com/search.json?limit=100&q=" + HttpUtility.UrlEncode(queryText));
            }

            List<string> textToExamine = await SearchRedditAsync(actor.Name);

            var analyzer = new SentimentIntensityAnalyzer();
            int validResults = 0;
            double resultsTotal = 0;
            List<string> web = new List<string>();
            foreach (string textValue in textToExamine)
            {
                var results = analyzer.PolarityScores(textValue.Substring(0, textValue.Length < 2500 ? textValue.Length : 2500));
                if (results.Compound != 0)
                {
                    resultsTotal += results.Compound;
                    validResults++;
                }
                web.Add(textValue.Substring(0, textValue.Length < 100 ? textValue.Length : 100));
            }

            double avgResult = Math.Round(resultsTotal / validResults, 2);
            aVM.rating = actor.Name + " " + avgResult.ToString() + ": " + CategorizeSetiment(avgResult);

            aVM.posts = web;

            return View(aVM);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,ActorLink")] Actor actor, IFormFile ActorPhoto)
		{
			ModelState.Remove(nameof(ActorPhoto));

			if (ModelState.IsValid)
			{
				if (ActorPhoto != null && ActorPhoto.Length != 0)
				{
					var memoryStream = new MemoryStream();
					await ActorPhoto.CopyToAsync(memoryStream);
					actor.Photo = memoryStream.ToArray();
				}
				else
				{
					actor.Photo = new byte[0];
				}
				_context.Add(actor);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(actor);
		}

		// GET: Actors/Edit/5
		public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Gender,Age,ActorLink")] Actor actor, IFormFile ActorPhoto)
		{
			if (id != actor.Id)
			{
				return NotFound();
			}

			Actor existingActor = _context.Actor.AsNoTracking().FirstOrDefault(m => m.Id == id);

			if (ActorPhoto != null && ActorPhoto.Length != 0)
			{
				var memoryStream = new MemoryStream();
				await ActorPhoto.CopyToAsync(memoryStream);
				actor.Photo = memoryStream.ToArray();
			}

			else if (existingActor == null)
			{
				actor.Photo = existingActor.Photo;
			}
			else
			{
				actor.Photo = new byte[0];
			}
			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(actor);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ActorExists(actor.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(actor);
		}

		// GET: Actors/Delete/5
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int? id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
