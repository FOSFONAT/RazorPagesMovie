using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace RazorPagesMovie.Models
{
    public class Review
    {
        public int Id { get; set; }

        [StringLength(2000)]
        public string? AdminReply { get; set; }


        [StringLength(128)]
        public string? AdminReplyBy { get; set; }


        public DateTime? AdminRepliedAt { get; set; }


        [Required]
        public int MovieId { get; set; }


        // Identity користувача (з ApplicationDbContext)
        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;


        // Знімок відображуваного імені (щоб не тягнути UserManager для кожного)
        [StringLength(128)]
        public string? ReviewerName { get; set; }


        [Range(1, 5)]
        public int Rating { get; set; }


        [StringLength(2000)]
        public string? Comment { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        // Навігація
        public Movie? Movie { get; set; }
    }
}
