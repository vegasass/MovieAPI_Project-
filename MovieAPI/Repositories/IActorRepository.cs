using MovieAPI.Models;

namespace MovieAPI.Repositories
{
    public interface IActorRepository
    {
        Task<List<Actor>> GetAllAsync();
        Task<Actor> GetByIdAsync(int id);
        Task AddAsync(Actor actor);
        Task UpdateAsync(Actor actor);
        Task DeleteAsync(Actor actor);
        Task DeleteAllAsync();
    }
}
