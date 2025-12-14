using Microsoft.AspNetCore.Mvc;
using MovieAPI.Dto;
using MovieAPI.Services;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _service;

        public ReviewController(IReviewService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReviewInputDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (result == null)
                return BadRequest("Cannot create review");

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

        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetByMovie(int movieId)
        {
            return Ok(await _service.GetByMovieAsync(movieId));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ReviewInputDto dto)
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
            if (!ok)
                return BadRequest();

            return Ok();
        }
    }

}
