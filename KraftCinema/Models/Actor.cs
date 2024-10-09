using System.ComponentModel.DataAnnotations;

namespace KraftCinema.Models
{
    public class Actor
    {
        public int? Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Gender { get; set; }

        public int? Age { get; set; }

        [Display(Name = "Profile")]
        public string? ActorLink { get; set; }

        [DataType(DataType.Upload)]
        public byte[]? Photo { get; set; }
    }
}
