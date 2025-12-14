using Microsoft.EntityFrameworkCore;
using MovieAPI.Data;
using MovieAPI.Models;

namespace MovieAPI.Repositories
{
    public class ActorRepository : IActorRepository
    {
        private readonly ApiContext _context;

        public ActorRepository(ApiContext context, ILogger<ActorRepository> logger)
        {
            _context = context;
        }

        public async Task AddAsync(Actor actor)
        {

            _context.Actors.Add(actor);
            await _context.SaveChangesAsync();

        }

        public async Task DeleteAllAsync()
        {

            List<Actor> allActors = await GetAllAsync();
            _context.Actors.RemoveRange(allActors);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Actor actor)
        {

            _context.Actors.Remove(actor);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Actor>> GetAllAsync()
        {


            List<Actor> actors = await _context.Actors.ToListAsync();

            return actors;
        }

        public async Task<Actor> GetByIdAsync(int id)
        {

            Actor actor = await _context.Actors.FirstOrDefaultAsync(a => a.Id == id);

            return actor;
        }

        public async Task UpdateAsync(Actor actor)
        {

            _context.Actors.Update(actor);
            await _context.SaveChangesAsync();

        }
    }
}