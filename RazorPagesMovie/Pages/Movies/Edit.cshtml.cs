using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;
using RazorPagesMovie.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace RazorPagesMovie.Pages.Movies
{
    public class EditModel : PageModel
    {
        private readonly RazorPagesMovie.Data.RazorPagesMovieContext _context;
        private readonly IWebHostEnvironment _env;


        public EditModel(RazorPagesMovieContext context, IWebHostEnvironment env) // + env
        {
            _context = context;
            _env = env;
        }




        [BindProperty]
        public Movie Movie { get; set; } = default!;


        [BindProperty]
        public IFormFile? PosterFile { get; set; }


        private static readonly string[] _permitted = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long _maxSize = 2 * 1024 * 1024;


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }
            Movie = movie;
            return Page();
        }


        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            // _context.Attach(Movie).State = EntityState.Modified;


            var dbMovie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == Movie.Id);
            if (dbMovie == null) return NotFound();


            // Оновлюємо лише дозволені поля
            dbMovie.Title = Movie.Title;
            dbMovie.ReleaseDate = Movie.ReleaseDate;
            dbMovie.Genre = Movie.Genre;
            dbMovie.Price = Movie.Price;
            dbMovie.Rating = Movie.Rating;


            if (PosterFile is not null && PosterFile.Length > 0)
            {
                // завантажити новий
                var newPath = await SavePosterAsync(PosterFile, dbMovie.Id);


                // видалити старий (не обов'язково; але добре для чистоти)
                TryDeleteFile(dbMovie.PosterPath);


                dbMovie.PosterPath = newPath;
            }


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(Movie.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }


            return RedirectToPage("./Index");
        }


        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
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


            return $"/uploads/movies/{movieId}/{fileName}";
        }


        private void TryDeleteFile(string? webPath)
        {
            if (string.IsNullOrWhiteSpace(webPath)) return;
            var fullPath = Path.Combine(_env.WebRootPath, webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
            {
                try { System.IO.File.Delete(fullPath); } catch { /* ігноруємо */ }
            }
        }


    }
}
