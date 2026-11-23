using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace RazorPagesMovie.Pages.Movies;

public class CreateModel : PageModel
{
    private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;
    private readonly IWebHostEnvironment _env;


    public CreateModel(RazorPagesMovieContext context, IWebHostEnvironment env) // <-- інжект env
    {
        _context = context;
        _env = env;
    }


    public IActionResult OnGet()
    {
        return Page();
    }


    [BindProperty]
    public Movie Movie { get; set; } = default!;


    [BindProperty]
    public IFormFile? PosterFile { get; set; }


    private static readonly string[] _permitted = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long _maxSize = 2 * 1024 * 1024;


    //  public void OnGet() { }


    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }


        _context.Movie.Add(Movie);
        await _context.SaveChangesAsync();
        // Якщо додано файл — зберегти й оновити PosterPath
        if (PosterFile is not null && PosterFile.Length > 0)
        {
            try
            {
                Movie.PosterPath = await SavePosterAsync(PosterFile, Movie.Id);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // за бажанням — видалити створений запис при фейлі завантаження
                ModelState.AddModelError(string.Empty, $"Помилка завантаження постера: {ex.Message}");
                return Page();
            }
        }
        return RedirectToPage("./Index");
    }


    private async Task<string> SavePosterAsync(IFormFile file, int movieId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_permitted.Contains(ext))
            throw new InvalidOperationException("Неприпустимий формат зображення.");


        if (file.Length <= 0 || file.Length > _maxSize)
            throw new InvalidOperationException("Файл порожній або перевищує 2 МБ.");


        var destDir = Path.Combine(_env.WebRootPath, "uploads", "movies", movieId.ToString());
        Directory.CreateDirectory(destDir);


        var fileName = $"poster-{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(destDir, fileName);


        using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream);


        // веб-відносний шлях
        return $"/uploads/movies/{movieId}/{fileName}";
    }
}
