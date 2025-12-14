using MovieAPI.Models;

namespace MovieAPI.Repositories
{
    public interface IPlaylistRepository
    {
        Task<Playlist?> GetByIdAsync(int id);
        Task<List<Playlist>> GetByUserAsync(string userName);
        Task<List<Playlist>> GetAllAsync();
        Task AddAsync(Playlist playlist);
        Task UpdateAsync(Playlist playlist);
        Task DeleteAsync(Playlist playlist);
    }

}
