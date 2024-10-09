namespace KraftCinema.Models
{
    public class MovieDetailsVM
    {
        public Movie movie { get; set; }
        public List<Actor> actors { get; set; }
        public List<string> posts { get; set; }
        public string rating { get; set; }
    }
}
