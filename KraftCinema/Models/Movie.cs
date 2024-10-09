using System.ComponentModel.DataAnnotations;

namespace KraftCinema.Models
{
    public class Movie
    {
        public int? Id { get; set; }

        [Required]
        public string? Title { get; set; }
        [Display(Name ="Link")]
        public string? MovieLink { get; set; }

        public string? Genre { get; set; }

        [Display(Name = "Year Released")]
        public string? YearOfRelease { get; set; }

        [DataType(DataType.Upload)]
        public byte[]? Poster { get; set; }
    }
}
