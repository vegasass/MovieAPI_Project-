using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;

namespace MovieAPI.Services
{
    public class ActorService : IActorService
    {
        private readonly IActorRepository _actorRepo;
        private readonly IMovieRepository _movieRepo;
        private readonly ILogger<ActorService> _logger;
        private readonly IMemoryCache _cache;
        public ApiOptions options;

        public ActorService(
            IActorRepository actorRepo,
            IMovieRepository movieRepo,
            ILogger<ActorService> logger,
            IMemoryCache cache,
            IOptions<ApiOptions> options)
        {
            _actorRepo = actorRepo;
            _movieRepo = movieRepo;
            _logger = logger;
            _cache = cache;
            this.options = options.Value;  
        }

        public async Task<ActorOutputDto> CreateAsync(ActorInputDto dto)
        {
            _logger.LogInformation($"Створення актора: {dto.Name}");

            List<Actor> currentActors = await _actorRepo.GetAllAsync();

            if (currentActors.Count >= options.MaxActorsLimit)
            {
                _logger.LogError(
                    "Досягнуто максимальну кількість акторів ({Limit}). Додавання заборонено.",
                    options.MaxActorsLimit
                );

                throw new InvalidOperationException("Досягнуто максимальну кількість акторів. Додавання неможливе.");
            }

            Actor actor = new Actor
            {
                Name = dto.Name,
                Gender = dto.Gender,
                Country = dto.Country
            };

            await _actorRepo.AddAsync(actor);

            _logger.LogInformation("Актор створений успішно. ID = {Id}", actor.Id);

            _cache.Remove("actors_all");

            ActorOutputDto output = new ActorOutputDto
            {
                Id = actor.Id,
                Name = actor.Name,
                Gender = actor.Gender,
                Country = actor.Country
            };

            return output;
        }

        public async Task<List<ActorOutputDto>> GetAllAsync()
        {
            _logger.LogInformation("Отримання списку всіх акторів");

            List<ActorOutputDto>? cached;
            if (_cache.TryGetValue("actors_all", out cached))
            {
                _logger.LogInformation("Отримання списку акторів");
                return cached;
            }

            List<Actor> list = await _actorRepo.GetAllAsync();

            _logger.LogInformation("Отримано всіх акторів");

            List<ActorOutputDto> result = list
                .Select(a => new ActorOutputDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    Gender = a.Gender,
                    Country = a.Country
                })
                .ToList();

            _cache.Set("actors_all", result, TimeSpan.FromDays(1));

            return result;
        }

        public async Task<ActorOutputDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Пошук актора з ID={Id}", id);

            string cacheKey = "actor_" + id.ToString();

            ActorOutputDto? cached;
            if (_cache.TryGetValue(cacheKey, out cached))
            {
                _logger.LogInformation("Отримання даних актора ID={Id}", id);
                return cached;
            }

            Actor? actor = await _actorRepo.GetByIdAsync(id);

            if (actor == null)
            {
                _logger.LogWarning("Актор ID={Id} не знайдений", id);
                return null;
            }

            ActorOutputDto result = new ActorOutputDto
            {
                Id = actor.Id,
                Name = actor.Name,
                Gender = actor.Gender,
                Country = actor.Country
            };

            _cache.Set(cacheKey, result, TimeSpan.FromDays(1));

            return result;
        }

        public async Task<ActorOutputDto?> UpdateAsync(int id, ActorInputDto dto)
        {
            _logger.LogInformation("Оновлення актора ID={Id} новими даними: {@Dto}", id, dto);

            Actor? actor = await _actorRepo.GetByIdAsync(id);

            if (actor == null)
            {
                _logger.LogWarning("Спроба оновлення. Актор ID={Id} не знайдений", id);
                return null;
            }

            actor.Name = dto.Name;
            actor.Gender = dto.Gender;
            actor.Country = dto.Country;

            await _actorRepo.UpdateAsync(actor);

            _logger.LogInformation("Актор ID={Id} оновлений успішно", id);

            _cache.Remove("actors_all");
            _cache.Remove("actor_" + id.ToString());

            ActorOutputDto result = new ActorOutputDto
            {
                Id = actor.Id,
                Name = actor.Name,
                Gender = actor.Gender,
                Country = actor.Country
            };

            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Спроба видалення актора ID={Id}", id);

            Actor? actor = await _actorRepo.GetByIdAsync(id);

            if (actor == null)
            {
                _logger.LogWarning("Актор ID={Id} не знайдений — видалення неможливе", id);
                return false;
            }

            List<Movie> movies = await _movieRepo.GetAllAsync();

            bool isUsed = movies.Any(m => m.ActorIds.Contains(id));


            if (!options.AllowActorDelete && isUsed)
            {
                _logger.LogWarning("Актор ID={Id} використовується у фільмах — видалення заборонено конфігурацією", id);
                return false;
            }


            if (options.AllowActorDelete && isUsed)
            {
                _logger.LogInformation("Каскадне видалення: очищаємо актора з фільмів");

                foreach (Movie movie in movies)
                {
                    if (movie.ActorIds.Contains(id))
                    {
                        movie.ActorIds.Remove(id);
                        await _movieRepo.UpdateAsync(movie);
                      

                        _logger.LogInformation("Актор ID={Id} видалений з фільму '{Title}'", id, movie.Title);
                    }
                }
            }

  
            await _actorRepo.DeleteAsync(actor);

            _logger.LogInformation("Актор ID={Id} успішно видалений", id);

            _cache.Remove("actors_all");
            _cache.Remove("actor_" + id);

            return true;
        }

        public async Task<bool> DeleteAllAsync()
        {
            _logger.LogInformation("Спроба видалення всіх акторів...");

            List<Movie> movies = await _movieRepo.GetAllAsync();
            List<Actor> allActors = await _actorRepo.GetAllAsync();


            bool anyUsed = movies.Any(m => m.ActorIds.Any());


            if (!options.AllowActorDelete && anyUsed)
            {
                _logger.LogWarning("Видалення всіх акторів заборонено — знайдено фільми з акторами");
                return false;
            }

            if (options.AllowActorDelete && anyUsed)
            {
                _logger.LogInformation("Каскадне видалення всіх акторів: очищаємо всі фільми");

                foreach (Movie movie in movies)
                {
                    if (movie.ActorIds.Count > 0)
                    {
                        movie.ActorIds.Clear();
                        await _movieRepo.UpdateAsync(movie);

                        _logger.LogInformation( "Видалено всіх акторів з фільму '{Title}'", movie.Title);
                    }
                }
            }

            await _actorRepo.DeleteAllAsync();

            _logger.LogWarning("Усі актори були видалені успішно");

            _cache.Remove("actors_all");


            foreach (Actor actor in allActors)
            {
                _cache.Remove("actor_" + actor.Id.ToString());
            }

            return true;
        }

    }
}
