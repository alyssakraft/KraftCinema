using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KraftCinema.Data;
using KraftCinema.Models;

using System.Numerics;
using System.Runtime.Intrinsics.X86;
using System.Net;
using System.Text.Json;
using System.Web;
using VaderSharp2;
using System.Drawing;

namespace KraftCinema.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

		public async Task<IActionResult> GetMoviePhoto(int id)
		{
			var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

			if (movie == null)
			{
				return NotFound();
			}

			var imageData = movie.Poster;

			return File(imageData, "image/jpg");
		}

		public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie.ToListAsync());
        }

        public static readonly HttpClient client = new HttpClient();
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

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            MovieDetailsVM mVM = new MovieDetailsVM();
            mVM.movie = movie;

            var actors = new List<Actor>();

            actors = await (from a in _context.Actor
                            join ma in _context.MovieActor on a.Id equals ma.ActorID
                            where ma.MovieID == id
                            select a).ToListAsync();
            mVM.actors = actors;

            List<string> textToExamine = await SearchRedditAsync(movie.Title);

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
            mVM.rating = movie.Title + " " + avgResult.ToString() + ": " + CategorizeSetiment(avgResult);

            mVM.posts = web;

            return View(mVM);
        }


        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Title,MovieLink,Genre,YearOfRelease")] Movie movie, IFormFile MoviePoster)
		{
			ModelState.Remove(nameof(MoviePoster));

			if (ModelState.IsValid)
			{
				if (MoviePoster != null && MoviePoster.Length != 0)
				{
					var memoryStream = new MemoryStream();
					await MoviePoster.CopyToAsync(memoryStream);
					movie.Poster = memoryStream.ToArray();
				}
				else
				{
					movie.Poster = new byte[0];
				}
				_context.Add(movie);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(movie);
		}

		// GET: Movies/Edit/5
		public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,MovieLink,Genre,YearOfRelease")] Movie movie, IFormFile MoviePoster)
		{
			if (id != movie.Id)
			{
				return NotFound();
			}
			Movie existingMovie = _context.Movie.AsNoTracking().FirstOrDefault(m => m.Id == id);

			if (MoviePoster != null && MoviePoster.Length != 0)
			{
				var memoryStream = new MemoryStream();
				await MoviePoster.CopyToAsync(memoryStream);
				movie.Poster = memoryStream.ToArray();
			}

			else if (existingMovie == null)
			{
				movie.Poster = existingMovie.Poster;
			}
			else
			{
				movie.Poster = new byte[0];
			}
			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(movie);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!MovieExists(movie.Id))
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
			return View(movie);
		}

		// GET: Movies/Delete/5
		public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int? id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
