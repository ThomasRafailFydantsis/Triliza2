using System.ComponentModel.DataAnnotations;

namespace triliza.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }
        public string? Score { get; set; }
        public ICollection<Match>? MatchIds { get; set; }
        public DateTime DateOfGame { get; set; }
        public bool IsFull { get; set; }
        public bool IsOver { get; set; }
    }
}
