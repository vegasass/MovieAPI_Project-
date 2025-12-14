using MovieAPI.Data;
using MovieAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace MovieAPI.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApiContext _context;

        public ReviewRepository(ApiContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<Review>> GetByMovieAsync(int movieId)
        {
            return await _context.Reviews.Where(r => r.MovieId == movieId).ToListAsync();
        }

        public async Task<Review?> GetUserReviewAsync(string userName, int movieId)
        {
            return await _context.Reviews.FirstOrDefaultAsync(r => r.UserName == userName && r.MovieId == movieId);
        }

        public async Task AddAsync(Review review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Review review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Review review)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }

}
