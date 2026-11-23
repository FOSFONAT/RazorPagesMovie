using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using Microsoft.AspNetCore.Mvc;


namespace RazorPagesMovie.Pages.Admin.Reviews
{
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovieContext _context;
        public IndexModel(RazorPagesMovieContext context) => _context = context;


        public record RowVM(
            int ReviewId,
            string MovieTitle,
            string ReviewerName,
            int Rating,
            string? Comment,
            DateTime CreatedAt,
            string? AdminReply,
            DateTime? AdminRepliedAt
        );


        public IList<RowVM> Items { get; private set; } = new List<RowVM>();


        [BindProperty(SupportsGet = true)]
        public string? q { get; set; } // пошук за фільмом/автором/коментарем


        [BindProperty(SupportsGet = true)]
        public bool onlyUnreplied { get; set; } // лише без відповіді


        public async Task OnGetAsync()
        {
            var query = _context.Reviews
                .AsNoTracking()
                .Include(r => r.Movie)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(q))
            {
                var like = $"%{q}%";
                query = query.Where(r =>
                    EF.Functions.Like(r.Movie!.Title, like) ||
                    EF.Functions.Like(r.ReviewerName!, like) ||
                    EF.Functions.Like(r.Comment!, like));
            }


            if (onlyUnreplied)
                query = query.Where(r => r.AdminReply == null || r.AdminReply == "");


            Items = await query
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .Select(r => new RowVM(
                    r.Id,
                    r.Movie!.Title,
                    r.ReviewerName ?? "Користувач",
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    r.AdminReply,
                    r.AdminRepliedAt
                ))
                .ToListAsync();
        }
    }
}
