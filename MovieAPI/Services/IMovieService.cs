using MovieAPI.Dto;

namespace MovieAPI.Services
{
    public interface IMovieService
    {
        Task<MovieOutputDto> CreateAsync(MovieInputDto dto);
        Task<List<MovieOutputDto>> GetAllAsync();
        Task<MovieOutputDto?> GetByIdAsync(int id);
        Task<MovieOutputDto?> UpdateAsync(int id, MovieInputDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAllAsync();
    }
}
