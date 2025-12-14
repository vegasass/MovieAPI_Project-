using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;
using MovieAPI.Services;

namespace MovieAPITests
{
    public class PlaylistService_Test
    {
        private readonly Mock<IPlaylistRepository> _playlistRepoMock;
        private readonly Mock<IMovieRepository> _movieRepoMock;
        private readonly PlaylistService _service;

        public PlaylistService_Test()
        {
            _playlistRepoMock = new Mock<IPlaylistRepository>();
            _movieRepoMock = new Mock<IMovieRepository>();

            _service = new PlaylistService(
                _playlistRepoMock.Object,
                _movieRepoMock.Object
            );
        }

        [Fact]
        public async Task CreateAsync_CreatePlaylist_WhenValid()
        {
            
            List<Playlist> existing = new List<Playlist>();

            _playlistRepoMock.Setup(r => r.GetByUserAsync("user"))
                .ReturnsAsync(existing);

            _movieRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(new Movie { Id = 10 });

            PlaylistInputDto dto = new PlaylistInputDto
            {
                UserName = "user",
                Name = "MyList",
                MovieIds = new List<int> { 10 }
            };

            
            PlaylistOutputDto? result = await _service.CreateAsync(dto);

            
            Assert.NotNull(result);
            Assert.Equal("MyList", result.Name);
            Assert.Single(result.MovieIds);

            _playlistRepoMock.Verify(r => r.AddAsync(It.IsAny<Playlist>()), Times.Once);
        }


        [Fact]
        public async Task CreateAsync_ReturnNull_WhenPlaylistNameExists()
        {
            
            List<Playlist> existing = new List<Playlist>
            {
                new Playlist { Id = 1, UserName = "user", Name = "MyList" }
            };

            _playlistRepoMock.Setup(r => r.GetByUserAsync("user"))
                .ReturnsAsync(existing);

            PlaylistInputDto dto = new PlaylistInputDto
            {
                UserName = "user",
                Name = "MyList"
            };

            
            PlaylistOutputDto? result = await _service.CreateAsync(dto);

            
            Assert.Null(result);
            _playlistRepoMock.Verify(r => r.AddAsync(It.IsAny<Playlist>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnPlaylist_WhenExists()
        {
            
            Playlist playlist = new Playlist
            {
                Id = 1,
                UserName = "u",
                Name = "p"
            };

            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(playlist);

            
            PlaylistOutputDto? result = await _service.GetByIdAsync(1);

            
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnNull_WhenNotFound()
        {
            
            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Playlist?)null);

            
            PlaylistOutputDto? result = await _service.GetByIdAsync(1);

            
            Assert.Null(result);
        }


        [Fact]
        public async Task UpdateAsync_UpdatePlaylist_WhenExists()
        {
            
            Playlist playlist = new Playlist
            {
                Id = 1,
                UserName = "u",
                Name = "old",
                MovieIds = new List<int>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(playlist);

            _movieRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(new Movie { Id = 10 });

            PlaylistInputDto dto = new PlaylistInputDto
            {
                UserName = "user2",
                Name = "new",
                MovieIds = new List<int> { 10 }
            };

            
            PlaylistOutputDto? result = await _service.UpdateAsync(1, dto);

            
            Assert.NotNull(result);
            Assert.Equal("new", result.Name);
            Assert.Single(result.MovieIds);

            _playlistRepoMock.Verify(r => r.UpdateAsync(playlist), Times.Once);
        }


        [Fact]
        public async Task UpdateAsync_ReturnNull_WhenPlaylistNotFound()
        {
            
            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Playlist?)null);

            PlaylistInputDto dto = new PlaylistInputDto
            {
                UserName = "x",
                Name = "y"
            };

            
            PlaylistOutputDto? result = await _service.UpdateAsync(1, dto);

            
            Assert.Null(result);
        }


        [Fact]
        public async Task DeleteAsync_Delete_WhenExists()
        {
            
            Playlist playlist = new Playlist { Id = 1 };

            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(playlist);

            
            bool result = await _service.DeleteAsync(1);

            
            Assert.True(result);
            _playlistRepoMock.Verify(r => r.DeleteAsync(playlist), Times.Once);
        }


        [Fact]
        public async Task DeleteAsync_ReturnFalse_WhenNotFound()
        {
            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Playlist?)null);

            bool result = await _service.DeleteAsync(1);

            Assert.False(result);
        }

        [Fact]
        public async Task AddMovieAsync_AddMovie_WhenValid()
        {
            
            Playlist playlist = new Playlist
            {
                Id = 1,
                MovieIds = new List<int>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(playlist);

            _movieRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(new Movie { Id = 10 });

            
            bool result = await _service.AddMovieAsync(1, 10);

            
            Assert.True(result);
            Assert.Single(playlist.MovieIds);

            _playlistRepoMock.Verify(r => r.UpdateAsync(playlist), Times.Once);
        }

        [Fact]
        public async Task AddMovieAsync_ReturnFalse_WhenMovieNotFound()
        {
            Playlist playlist = new Playlist
            {
                Id = 1,
                MovieIds = new List<int>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(playlist);

            _movieRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync((Movie?)null);

            bool result = await _service.AddMovieAsync(1, 10);

            Assert.False(result);
        }


        [Fact]
        public async Task RemoveMovieAsync_RemoveMovie_WhenExists()
        {
            Playlist playlist = new Playlist
            {
                Id = 1,
                MovieIds = new List<int> { 5 }
            };

            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(playlist);

            bool result = await _service.RemoveMovieAsync(1, 5);

            Assert.True(result);
            Assert.Empty(playlist.MovieIds);

            _playlistRepoMock.Verify(r => r.UpdateAsync(playlist), Times.Once);
        }

        [Fact]
        public async Task RemoveMovieAsync_ReturnFalse_WhenMovieNotInPlaylist()
        {
            Playlist playlist = new Playlist
            {
                Id = 1,
                MovieIds = new List<int>()
            };

            _playlistRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(playlist);

            bool result = await _service.RemoveMovieAsync(1, 7);

            Assert.False(result);
        }
    }
}
