using MovieAPI.Dto;

namespace MovieAPI.Services
{
    public interface IReviewService
    {
        Task<ReviewOutputDto?> CreateAsync(ReviewInputDto dto);
        Task<ReviewOutputDto?> GetByIdAsync(int id);
        Task<List<ReviewOutputDto>> GetByMovieAsync(int movieId);
        Task<ReviewOutputDto?> UpdateAsync(int id, ReviewInputDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
