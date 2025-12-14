using MovieAPI.Data;
using MovieAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace MovieAPI.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly ApiContext _context;

        public PlaylistRepository(ApiContext context)
        {
            _context = context;
        }

        public async Task<Playlist?> GetByIdAsync(int id)
        {
            return await _context.Playlists.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Playlist>> GetByUserAsync(string userName)
        {
            return await _context.Playlists
                .Where(p => p.UserName == userName)
                .ToListAsync();
        }

        public async Task<List<Playlist>> GetAllAsync()
        {
            return await _context.Playlists.ToListAsync();
        }

        public async Task AddAsync(Playlist playlist)
        {
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Playlist playlist)
        {
            _context.Playlists.Update(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Playlist playlist)
        {
            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
        }
    }

}
