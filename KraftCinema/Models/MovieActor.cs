using System.ComponentModel.DataAnnotations.Schema;

namespace KraftCinema.Models
{
    public class MovieActor
    {
        public int Id { get; set; }

        [ForeignKey("Movie")]
        public int? MovieID { get; set; }
        public Movie? Movie { get; set; }

        [ForeignKey("Actor")]
        public int? ActorID { get; set; }
        public Actor? Actor { get; set; }
    }
}
