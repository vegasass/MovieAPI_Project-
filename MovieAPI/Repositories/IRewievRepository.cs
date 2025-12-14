using MovieAPI.Models;

namespace MovieAPI.Repositories
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(int id);
        Task<List<Review>> GetByMovieAsync(int movieId);
        Task<Review?> GetUserReviewAsync(string userName, int movieId);

        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(Review review);
    }
}
