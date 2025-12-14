using MovieAPI.Dto;

namespace MovieAPI.Services
{
    public interface IPlaylistService
    {
        Task<PlaylistOutputDto?> CreateAsync(PlaylistInputDto dto);
        Task<PlaylistOutputDto?> GetByIdAsync(int id);
        Task<PlaylistOutputDto?> UpdateAsync(int id, PlaylistInputDto dto);
        Task<bool> DeleteAsync(int id);

        Task<bool> AddMovieAsync(int id, int movieId);
        Task<bool> RemoveMovieAsync(int id, int movieId);
    }
}
