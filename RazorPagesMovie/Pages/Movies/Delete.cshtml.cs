using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;

namespace RazorPagesMovie.Pages.MoviePages;

public class DeleteModel : PageModel
{
    private readonly RazorPagesMovieContext _context;
    private readonly IWebHostEnvironment _env;


    public DeleteModel(RazorPagesMovie.Data.RazorPagesMovieContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public DeleteModel(RazorPagesMovieContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Movie Movie { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
        if (movie is null)
        {
            return NotFound();
        }
        else
        {
            Movie = movie;
        }

        return Page();
    }
    private void TryDeletePosterFolder(int movieId)
    {
        var dir = Path.Combine(_env.WebRootPath, "uploads", "movies", movieId.ToString());
        if (Directory.Exists(dir))
        {
            try { Directory.Delete(dir, recursive: true); } catch { }
        }
    }


    

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var movie = await _context.Movie.FindAsync(id);
        if (movie != null)
        {
            Movie = movie;
            _context.Movie.Remove(Movie);
            await _context.SaveChangesAsync();
        }
        // в методі OnPostAsync після успішного SaveChanges:
        TryDeletePosterFolder(Movie.Id);
        return RedirectToPage("./Index");
    }
}
