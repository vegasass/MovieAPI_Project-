using Microsoft.AspNetCore.Mvc;
using MovieAPI.Dto;
using MovieAPI.Services;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _service;

        public PlaylistController(IPlaylistService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(PlaylistInputDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (result == null)
                return BadRequest("Playlist already exists");
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PlaylistInputDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == null)
                return BadRequest();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool ok = await _service.DeleteAsync(id);
            return ok ? Ok() : BadRequest();
        }

        [HttpPost("{id}/addMovie")]
        public async Task<IActionResult> AddMovie(int id, int movieId)
        {
            bool ok = await _service.AddMovieAsync(id, movieId);
            return ok ? Ok() : BadRequest();
        }

        [HttpDelete("{id}/removeMovie")]
        public async Task<IActionResult> RemoveMovie(int id, int moveId)
        {
            bool ok = await _service.RemoveMovieAsync(id, moveId);
            return ok ? Ok() : BadRequest();
        }
    }

}
