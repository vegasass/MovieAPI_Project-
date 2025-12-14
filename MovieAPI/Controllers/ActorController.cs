using Microsoft.AspNetCore.Mvc;
using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;
using MovieAPI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActorsController : ControllerBase
    {
        private readonly IActorService _actorService;

        public ActorsController(IActorService actorService)
        {
            _actorService = actorService;
        }

        [HttpPost]
        public async Task<IActionResult> AddActor(ActorInputDto dto)
        {
            try
            {
                ActorOutputDto actor = await _actorService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetActorById), new { id = actor.Id }, actor);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllActors()
        {
            return Ok(await _actorService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetActorById(int id)
        {
            ActorOutputDto actor = await _actorService.GetByIdAsync(id);
            if (actor == null)
                return NotFound($"Актор з ID {id} не знайдений");

            return Ok(actor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateActor(int id, ActorInputDto dto)
        {
            ActorOutputDto updated = await _actorService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound($"Актор з ID {id} не знайдений");

            return Ok(updated);
        }

   
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActorById(int id)
        {
            bool success = await _actorService.DeleteAsync(id);

            if (!success)
                return BadRequest($"Неможливо видалити актора {id}, бо він бере участь у фільмах або не існує");

            return Ok($"Актор {id} видалений");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllActors()
        {
            bool success = await _actorService.DeleteAllAsync();

            if (!success)
                return BadRequest("Неможливо видалити всіх акторів — існують фільми з акторами");

            return Ok("Усі актори видалені");
        }
    }
}