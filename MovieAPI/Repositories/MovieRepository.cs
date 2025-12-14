using Microsoft.EntityFrameworkCore;
using MovieAPI.Data;
using MovieAPI.Models;

namespace MovieAPI.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly ApiContext _context;

        public MovieRepository(ApiContext context)
        {
            _context = context;
        }

        public async Task<List<Movie>> GetAllAsync()
        {
            List<Movie> movies = await _context.Movies.ToListAsync();
            return movies;
        }

        public async Task<Movie> GetByIdAsync(int id)
        {
            Movie movie = await _context.Movies.FirstOrDefaultAsync(m => m.Id == id);
            return movie;
        }

        public async Task AddAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Movie movie)
        {
            _context.Movies.Update(movie);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Movie movie)
        {
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllAsync()
        {
            List<Movie> all = await _context.Movies.ToListAsync();
            _context.Movies.RemoveRange(all);
            await _context.SaveChangesAsync();
        }
    }
}
