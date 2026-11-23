using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using Microsoft.AspNetCore.Mvc; // для BindProperty
using Microsoft.AspNetCore.Mvc.Rendering; // для SelectList
namespace RazorPagesMovie.Pages.Catalog
{
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        private readonly RazorPagesMovieContext _context;
        public IndexModel(RazorPagesMovieContext context) => _context = context;
        public IList<Movie> Movies { get; private set; } = new List<Movie>();
        // Пошук за назвою (вже було)
        [BindProperty(SupportsGet = true)]
        public string? q { get; set; }
        // Нове: вибраний жанр
        [BindProperty(SupportsGet = true)]
        public string? genre { get; set; }
        // Нове: список жанрів для випадаючого списку

        public SelectList GenreOptions { get; private set; } = default!;
        public async Task OnGetAsync()
        {
            // 1) Підготувати список жанрів (без null/порожніх), унікальні відсортовані
            var genres = await _context.Movie.AsNoTracking()
            .Where(m => !string.IsNullOrEmpty(m.Genre))
            .Select(m => m.Genre!)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();
            GenreOptions = new SelectList(genres);
            // 2) Базовий запит фільмів
            var query = _context.Movie.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                // Пошук по назві (через LIKE для кращої сумісності)
                var pattern = $"%{q}%";
                query = query.Where(m => EF.Functions.Like(m.Title, pattern));
            }
            if (!string.IsNullOrWhiteSpace(genre))
            {
                // Точний збіг жанру
                query = query.Where(m => m.Genre == genre);
            }
            Movies = await query
            .OrderBy(m => m.Title)
            .ToListAsync();
        }
    }
}