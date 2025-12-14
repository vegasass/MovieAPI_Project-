using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;
using Microsoft.Extensions.Logging;

namespace MovieAPI.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepo;
        private readonly IMovieRepository _movieRepo;

        public PlaylistService(
            IPlaylistRepository playlistRepo,
            IMovieRepository movieRepo)
        {
            _playlistRepo = playlistRepo;
            _movieRepo = movieRepo;
        }

        private PlaylistOutputDto Convert(Playlist playlist)
        {
            return new PlaylistOutputDto
            {
                Id = playlist.Id,
                UserName = playlist.UserName,
                Name = playlist.Name,
                MovieIds = playlist.MovieIds
            };
        }

        public async Task<PlaylistOutputDto?> CreateAsync(PlaylistInputDto dto)
        {
            var existing = await _playlistRepo.GetByUserAsync(dto.UserName);
            if (existing.Any(p => p.Name == dto.Name))
                return null;

            var playlist = new Playlist
            {
                UserName = dto.UserName,
                Name = dto.Name
            };


            if (dto.MovieIds != null)
            {
                foreach (var movieId in dto.MovieIds)
                {
                    var movie = await _movieRepo.GetByIdAsync(movieId);
                    if (movie != null)
                        playlist.MovieIds.Add(movieId);
                }
            }

            await _playlistRepo.AddAsync(playlist);
            return Convert(playlist);
        }

        public async Task<PlaylistOutputDto?> GetByIdAsync(int id)
        {
            var playlist = await _playlistRepo.GetByIdAsync(id);
            return playlist == null ? null : Convert(playlist);
        }
        public async Task<PlaylistOutputDto?> UpdateAsync(int id, PlaylistInputDto dto)
        {
            var playlist = await _playlistRepo.GetByIdAsync(id);
            if (playlist == null)
                return null;

            playlist.UserName = dto.UserName;
            playlist.Name = dto.Name;

            if (dto.MovieIds != null)
            {
                playlist.MovieIds.Clear();
                foreach (var movieId in dto.MovieIds)
                {
                    var movie = await _movieRepo.GetByIdAsync(movieId);
                    if (movie != null)
                        playlist.MovieIds.Add(movieId);
                }
            }

            await _playlistRepo.UpdateAsync(playlist);
            return Convert(playlist);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var playlist = await _playlistRepo.GetByIdAsync(id);
            if (playlist == null)
                return false;

            await _playlistRepo.DeleteAsync(playlist);
            return true;
        }

        public async Task<bool> AddMovieAsync(int playlistId, int movieId)
        {
            var playlist = await _playlistRepo.GetByIdAsync(playlistId);

            if (playlist == null)
                return false;

            var movie = await _movieRepo.GetByIdAsync(movieId);

            if (movie == null)
                return false;

            if (playlist.MovieIds.Contains(movieId))
                return false;

            playlist.MovieIds.Add(movieId);
            await _playlistRepo.UpdateAsync(playlist);

            return true;
        }

        public async Task<bool> RemoveMovieAsync(int playlistId, int movieId)
        {
            Playlist playlist = await _playlistRepo.GetByIdAsync(playlistId)
                ;
            if (playlist == null)
                return false;

            if (!playlist.MovieIds.Contains(movieId))
                return false;

            playlist.MovieIds.Remove(movieId);

            await _playlistRepo.UpdateAsync(playlist);

            return true;
        }
    }
}
