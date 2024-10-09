namespace KraftCinema.Models
{
    public class ActorDetailsVM
    {
        public Actor actor { get; set; }
        public List<Movie> movies { get; set; }

        public List<string> posts { get; set; }
        public string rating { get; set; }
    }
}
