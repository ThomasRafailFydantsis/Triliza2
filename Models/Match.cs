using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace triliza.Models
{
    public class Match
    {
        [Key]
        public int Id { get; set; }
        public virtual ApplicationUser User { get; set; }
        [Required]
        public string UserId { get; set; }
        [JsonIgnore]
        public virtual Session? Session { get; set; }
        public int SessionId { get; set; }
        public string? Shape { get; set; }
        public int Points { get; set; }
        public bool HasWon { get; set; } = false;

    }
}
