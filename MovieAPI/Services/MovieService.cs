using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace MovieAPI.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _movieRepo;
        private readonly IActorRepository _actorRepo;
        private readonly ILogger<MovieService> _logger;
        private readonly IMemoryCache _cache;

        public MovieService(
            IMovieRepository movieRepo,
            IActorRepository actorRepo,
            ILogger<MovieService> logger,
            IMemoryCache cache)
        {
            _movieRepo = movieRepo;
            _actorRepo = actorRepo;
            _logger = logger;
            _cache = cache;
        }

        public async Task<MovieOutputDto> CreateAsync(MovieInputDto dto)
        {
            _logger.LogInformation("Створення фільму: {@Dto}", dto);

            foreach (int id in dto.ActorIds)
            {
                Actor? actor = await _actorRepo.GetByIdAsync(id);

                if (actor == null)
                {
                    _logger.LogError($"Неможливо створити фільм — актор ID={id} не існує");
                    throw new Exception($"Актор {id} не існує");
                }
            }
                
            Movie movie = new Movie
            {
                Title = dto.Title,
                Year = dto.Year,
                Genre = dto.Genre,
                ActorIds = dto.ActorIds.ToList()
            };

            await _movieRepo.AddAsync(movie);

            _logger.LogInformation("Фільм створений успішно. ID={Id}", movie.Id);

            _cache.Remove("movies_all");

            MovieOutputDto result = new MovieOutputDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Genre = movie.Genre,
                ActorIds = movie.ActorIds.ToList(),
                Rating = movie.Rating,
                VotesCount = movie.VotesCount
            };

            return result;
        }

        public async Task<List<MovieOutputDto>> GetAllAsync()
        {
            _logger.LogInformation("Отримання списку всіх фільмів...");

            List<MovieOutputDto>? cached;
            if (_cache.TryGetValue("movies_all", out cached))
            {
                _logger.LogInformation("Фільми отриманні");

                return cached;
            }

            List<Movie> list = await _movieRepo.GetAllAsync();

            _logger.LogInformation("Фільми отриманні");

            List<MovieOutputDto> result = list
                .Select(m => new MovieOutputDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Year = m.Year,
                    Genre = m.Genre,
                    ActorIds = m.ActorIds.ToList(),
                    Rating = m.Rating,
                    VotesCount = m.VotesCount
                })
                .ToList();

            _cache.Set("movies_all", result, TimeSpan.FromDays(1));

            return result;
        }

        public async Task<MovieOutputDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Пошук фільму ID={Id}", id);

            string key = "movie_" + id.ToString();

            MovieOutputDto? cached;
            if (_cache.TryGetValue(key, out cached))
            {
                _logger.LogInformation("Отримання" +
                    " фільму ID={Id}", id);
                return cached;
            }

            Movie? movie = await _movieRepo.GetByIdAsync(id);

            if (movie == null)
            {
                _logger.LogWarning("Фільм ID={Id} не знайдений", id);
                return null;
            }

            MovieOutputDto result = new MovieOutputDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Genre = movie.Genre,
                ActorIds = movie.ActorIds.ToList(),
                Rating = movie.Rating,
                VotesCount = movie.VotesCount
            };

            _cache.Set(key, result, TimeSpan.FromDays(1));

            return result;
        }

        public async Task<MovieOutputDto?> UpdateAsync(int id, MovieInputDto dto)
        {
            _logger.LogInformation("Оновлення фільму ID={Id} новими даними {@Dto}", id, dto);

            Movie? movie = await _movieRepo.GetByIdAsync(id);

            if (movie == null)
            {
                _logger.LogWarning("Спроба оновлення. Фільм ID={Id} не знайдений", id);
                return null;
            }

            foreach (int actorId in dto.ActorIds)
            {
                Actor actor = await _actorRepo.GetByIdAsync(actorId);

                if (actor == null)
                {
                    throw new Exception($"Актор {actorId} не існує");
                }
            }

            movie.Title = dto.Title;
            movie.Year = dto.Year;
            movie.Genre = dto.Genre;
            movie.ActorIds = dto.ActorIds.ToList();

            await _movieRepo.UpdateAsync(movie);

            _logger.LogInformation("Фільм ID={Id} оновлений успішно", id);

            _cache.Remove("movies_all");
            _cache.Remove("movie_" + id.ToString());

            MovieOutputDto updated = new MovieOutputDto
            {
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Genre = movie.Genre,
                ActorIds = movie.ActorIds.ToList(),
                Rating = movie.Rating,
                VotesCount = movie.VotesCount
            };

            return updated;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Спроба видалення фільму ID={Id}", id);

            Movie? movie = await _movieRepo.GetByIdAsync(id);

            if (movie == null)
            {
                _logger.LogWarning("Фільм ID={Id} не знайдений — видалення неможливе", id);
                return false;
            }

            await _movieRepo.DeleteAsync(movie);

            _logger.LogInformation("Фільм ID={Id} успішно видалений", id);

            _cache.Remove("movies_all");
            _cache.Remove("movie_" + id.ToString());

            return true;
        }

        public async Task<bool> DeleteAllAsync()
        {
            _logger.LogInformation("Видалення всіх фільмів...");

            await _movieRepo.DeleteAllAsync();

            _logger.LogWarning("Усі фільми були видалені");

            _cache.Remove("movies_all");

            return true;
        }

    }
}
