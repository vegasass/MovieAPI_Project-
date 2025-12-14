using MovieAPI.Dto;

namespace MovieAPI.Services
{
    public interface IActorService
    {
        Task<ActorOutputDto> CreateAsync(ActorInputDto dto);
        Task<List<ActorOutputDto>> GetAllAsync();
        Task<ActorOutputDto?> GetByIdAsync(int id);
        Task<ActorOutputDto?> UpdateAsync(int id, ActorInputDto dto);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAllAsync();
    }
}
