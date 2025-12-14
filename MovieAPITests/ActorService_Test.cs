using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using MovieAPI;
using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;
using MovieAPI.Services;

namespace MovieAPITests
{
    public class ActorService_Test
    {
        private readonly Mock<IActorRepository> _actorRepositoryMock;
        private readonly Mock<IMovieRepository> _movieRepositoryMock;
        private readonly ActorService _actorService;
        private IOptions<ApiOptions> _options;
        private IMemoryCache _cache;

        public ActorService_Test()
        {
            _actorRepositoryMock = new Mock<IActorRepository>();
            _movieRepositoryMock = new Mock<IMovieRepository>();
            ILogger<ActorService> logger = NullLogger<ActorService>.Instance;

            _cache = new MemoryCache(new MemoryCacheOptions());

            _options = Options.Create(new ApiOptions
            {
                MaxActorsLimit = 3,
                AllowActorDelete = false
            });

            _actorService = new ActorService(
                _actorRepositoryMock.Object,
                _movieRepositoryMock.Object,
                logger,
                _cache,
                _options
            );
        }

        [Fact]
        public async Task CreateAsync_WhenLimitNotReached_Succeed()
        {
            _actorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Actor>());

            ActorInputDto dto = new ActorInputDto
            {
                Name = "Actor1",
                Gender = 0,
                Country = "USA"
            };

            ActorOutputDto result = await _actorService.CreateAsync(dto);

            Assert.NotNull(result);
            Assert.Equal("Actor1", result.Name);

            _actorRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Actor>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_CreateFiveActors_ThrowsInvalidOperationException()
        {
            List<Actor> existingActors = new List<Actor>
            {
                new Actor { Id = 1, Name = "A" },
                new Actor { Id = 2, Name = "B" },
                new Actor { Id = 3, Name = "C" }
            };

            _actorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(existingActors);

            ActorInputDto newActor = new ActorInputDto
            {
                Name = "D",
                Gender = 0,
                Country = "USA"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _actorService.CreateAsync(newActor)
            );

            _actorRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Actor>()), Times.Never);
        }

        [Fact]
        public async Task GetAllActors_ReturnsActors()
        {
            List<Actor> actors = new List<Actor>
            {
                new Actor { Id = 1, Name = "" },
                new Actor { Id = 2, Name = "Actor1" }
            };

            _actorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(actors);

            List<ActorOutputDto> result = await _actorService.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnEmptyList_WhenNoActorsExist()
        {
            _actorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Actor>());

            List<ActorOutputDto> result = await _actorService.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteActor_WhenNotAllowed_ReturnFalse()
        {

            Actor existingActor = new Actor { Id = 2, Name = "A" };

            List<Movie> existingMovies = new List<Movie>
            {
                new Movie { Id = 1, Title = "FilmA", ActorIds = new List<int> { 1 } }
            };

            _actorRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingActor);

            _movieRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(existingMovies);

            bool result = await _actorService.DeleteAsync(1);

            Assert.False(result);
            _actorRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Actor>()), Times.Never);
        }
        [Fact]
        public async Task DeleteActor_WhenAllowed_ReturnTrue()
        {
            _actorService.options = new ApiOptions
            {
                MaxActorsLimit = 3,
                AllowActorDelete = true
            };
            Actor existingActor = new Actor { Id = 1, Name = "A" };

            List<Movie> existingMovies = new List<Movie>
            {
                new Movie { Id = 1, Title = "FilmA", ActorIds = new List<int> { 1 } },
                new Movie { Id = 2, Title = "FilmB", ActorIds = new List<int> { 1} },
                new Movie { Id = 3, Title = "FilmC", ActorIds = new List<int> { 1 } }
            };

            _actorRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingActor);

            _movieRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(existingMovies);

            bool result = await _actorService.DeleteAsync(1);

            Assert.True(result);
            _movieRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Movie>()), Times.Exactly(3));
        }
        [Fact]
        public async Task DeleteAllAsync_ClearIndividualActorCache()
        {
            Actor actor = new Actor
            {
                Id = 1,
                Name = "Actor1"
            };

            _actorRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(actor);

            _actorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Actor> { actor });

            _movieRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Movie>());

            await _actorService.GetByIdAsync(1);
            await _actorService.DeleteAllAsync();

            ActorOutputDto? cached = _cache.Get<ActorOutputDto>("actor_1");

            Assert.Null(cached);
        }

        [Fact]
        public async Task DeleteAllAsync_RemoveActorsFromMovieActorIds()
        {
            List<Movie> movies = new List<Movie>
            {
                new Movie { Id = 10, Title = "Film1", ActorIds = new List<int> { 1, 2 } },
                new Movie { Id = 11, Title = "Film2", ActorIds = new List<int> { 3 } }
            };

            _movieRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(movies);

            _actorRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Actor>
                {
                    new Actor { Id = 1, Name = "A" },
                    new Actor { Id = 2, Name = "B" }
                });

            _actorService.options = new ApiOptions { AllowActorDelete = true };

            bool result = await _actorService.DeleteAllAsync();

            Assert.True(result);
            Assert.Empty(movies[0].ActorIds);
            Assert.Empty(movies[1].ActorIds);

            _movieRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Movie>(m => m.Id == 10)), Times.Once);
            _movieRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Movie>(m => m.Id == 11)), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnActor_WhenExists()
        {
            _actorRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Actor { Id = 1, Name = "Test" });

            ActorOutputDto? result = await _actorService.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnNull_WhenActorNotFound()
        {
            _actorRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Actor?)null);

            ActorOutputDto? result = await _actorService.GetByIdAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_UpdateActor_WhenExists()
        {
            Actor actor = new Actor { Id = 1, Name = "Old" };

            _actorRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(actor);

            ActorInputDto dto = new ActorInputDto
            {
                Name = "New",
                Gender = 1,
                Country = "USA"
            };

            ActorOutputDto? result = await _actorService.UpdateAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal("New", result.Name);

            _actorRepositoryMock.Verify(r => r.UpdateAsync(actor), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ReturnNull_WhenActorDoesNotExist()
        {
            _actorRepositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Actor?)null);

            ActorInputDto dto = new ActorInputDto();

            ActorOutputDto? result = await _actorService.UpdateAsync(1, dto);

            Assert.Null(result);
        }
    }
}
