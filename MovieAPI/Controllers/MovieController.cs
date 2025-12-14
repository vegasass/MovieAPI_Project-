using Microsoft.AspNetCore.Mvc;
using MovieAPI.Dto;
using MovieAPI.Services;
using System.Threading.Tasks;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public MoviesController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        [HttpPost]
        public async Task<IActionResult> AddMovie(MovieInputDto dto)
        {
            try
            {
                var result = await _movieService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetMovieById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            return Ok(await _movieService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(int id)
        {
            var movie = await _movieService.GetByIdAsync(id);

            if (movie == null)
                return NotFound($"Фільм з ID {id} не знайдений");

            return Ok(movie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovie(int id, MovieInputDto dto)
        {
            try
            {
                var updated = await _movieService.UpdateAsync(id, dto);

                if (updated == null)
                    return NotFound($"Фільм з ID {id} не знайдений");

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovieById(int id)
        {
            var success = await _movieService.DeleteAsync(id);

            if (!success)
                return NotFound($"Фільм з ID {id} не знайдений");

            return Ok($"Фільм {id} видалений");
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllMovies()
        {
            await _movieService.DeleteAllAsync();
            return Ok("Усі фільми видалені");
        }
    }
}
