using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;
using MovieAPI.Services;

namespace MovieAPITests
{
    public class MovieService_Test
    {
        private readonly Mock<IMovieRepository> _movieRepoMock;
        private readonly Mock<IActorRepository> _actorRepoMock;
        private readonly MovieService _movieService;
        private readonly IMemoryCache _cache;

        public MovieService_Test()
        {
            _movieRepoMock = new Mock<IMovieRepository>();
            _actorRepoMock = new Mock<IActorRepository>();

            ILogger<MovieService> logger = NullLogger<MovieService>.Instance;

            _cache = new MemoryCache(new MemoryCacheOptions());

            _movieService = new MovieService(
                _movieRepoMock.Object,
                _actorRepoMock.Object,
                logger,
                _cache
            );
        }
        [Fact]
        public async Task CreateAsync_Throw_WhenActorDoesNotExist()
        {
            
            MovieInputDto dto = new()
            {
                Title = "Film",
                Year = 2020,
                Genre = "Drama",
                ActorIds = new List<int> { 99 }
            };

            _actorRepoMock.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((Actor?)null);

     
            await Assert.ThrowsAsync<Exception>(async () =>
                await _movieService.CreateAsync(dto)
            );

            _movieRepoMock.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Never);
        }
        [Fact]
        public async Task CreateAsync_CreateMovie_WhenActorsExist()
        {
            
            _actorRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Actor { Id = 1 });

            MovieInputDto dto = new MovieInputDto
            {
                Title = "TestMovie",
                Year = 2020,
                Genre = "Drama",
                ActorIds = new List<int> { 1 }
            };

            Movie movieCaptured = null!;

            _movieRepoMock.Setup(r => r.AddAsync(It.IsAny<Movie>()))
                .Callback<Movie>(m => movieCaptured = m)
                .Returns(Task.CompletedTask);

            
            MovieOutputDto result = await _movieService.CreateAsync(dto);

            
            Assert.NotNull(result);
            Assert.Equal("TestMovie", result.Title);
            Assert.Single(result.ActorIds);

            _movieRepoMock.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UpdateMovie_WhenValid()
        {
            
            Movie movie = new Movie
            {
                Id = 1,
                Title = "Old",
                ActorIds = new List<int>()
            };

            _movieRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(movie);

            _actorRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Actor());

            MovieInputDto dto = new()
            {
                Title = "NewTitle",
                Year = 2022,
                Genre = "New",
                ActorIds = new List<int> { 1 }
            };

            
            var result = await _movieService.UpdateAsync(1, dto);

            
            Assert.NotNull(result);
            Assert.Equal("NewTitle", result.Title);
            Assert.Contains(1, result.ActorIds);

            _movieRepoMock.Verify(r => r.UpdateAsync(movie), Times.Once);
        }
        [Fact]
        public async Task UpdateAsync_Throw_WhenInvalidActorId()
        {
            _movieRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Movie { Id = 1 });

            _actorRepoMock.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Actor?)null);

            MovieInputDto dto = new()
            {
                Title = "Film",
                Year = 2022,
                Genre = "Action",
                ActorIds = new List<int> { 999 }
            };

            await Assert.ThrowsAsync<Exception>(() =>
                _movieService.UpdateAsync(1, dto)
            );

            _movieRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Never);
        }

        [Theory]
        [InlineData(1, "FilmA")]
        [InlineData(2, "FilmB")]
        [InlineData(99, null)]   
        public async Task GetByIdAsync_ReturnCorrectMovieOrNull(int id, string? expectedTitle)
        {
            _cache.Remove("movie_"+id);
            
            var movies = new List<Movie>
            {
                 new Movie { Id = 1, Title = "FilmA", ActorIds = new List<int>() },
                 new Movie { Id = 2, Title = "FilmB", ActorIds = new List<int>() }
            };

            _movieRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int inputId) =>
                {
                    return movies.FirstOrDefault(m => m.Id == inputId);
                });

            
            var result = await _movieService.GetByIdAsync(id);

            
            if (expectedTitle == null)
            {
                Assert.Null(result);          
            }
            else
            {
                Assert.NotNull(result);        
                Assert.Equal(expectedTitle, result.Title);
     
            }
        }

        [Fact]
        public async Task DeleteAsync_DeleteMovie_WhenExists()
        {
            
            Movie movie = new Movie { Id = 1 };

            _movieRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(movie);

            
            bool result = await _movieService.DeleteAsync(1);

            
            Assert.True(result);
            _movieRepoMock.Verify(r => r.DeleteAsync(movie), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnFalse_WhenMovieNotFound()
        {
            
            _movieRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Movie?)null);

            
            bool result = await _movieService.DeleteAsync(1);

            
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnMovies()
        {
            
            List<Movie> movies = new List<Movie>
            {
                new Movie { Id = 1, Title = "A", ActorIds = new List<int>() },
                new Movie { Id = 2, Title = "B", ActorIds = new List<int>() }
            };

            _movieRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(movies);

            
            List<MovieOutputDto> result = await _movieService.GetAllAsync();

            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnEmptyList_WhenNoMoviesExist()
        {
            
            _movieRepoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Movie>());

            
            List<MovieOutputDto> result = await _movieService.GetAllAsync();

            
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteAllAsync_ShouldDeleteAllMovies()
        {
            _movieRepoMock.Setup(r => r.DeleteAllAsync())
                .Returns(Task.CompletedTask);

            
            bool result = await _movieService.DeleteAllAsync();

            
            Assert.True(result);
            _movieRepoMock.Verify(r => r.DeleteAllAsync(), Times.Once);
        }
    }
}
