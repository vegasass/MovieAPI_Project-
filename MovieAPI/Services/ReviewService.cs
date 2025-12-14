using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;

namespace MovieAPI.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly IMovieRepository _movieRepo;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            IReviewRepository reviewRepo,
            IMovieRepository movieRepo,
            ILogger<ReviewService> logger)
        {
            _reviewRepo = reviewRepo;
            _movieRepo = movieRepo;
            _logger = logger;
        }

        public async Task<ReviewOutputDto?> CreateAsync(ReviewInputDto dto)
        {
            Movie? movie = await _movieRepo.GetByIdAsync(dto.MovieId);
            if (movie == null)
                return null;

            if (dto.Score < 1 || dto.Score > 10)
                return null;

            Review? existing = await _reviewRepo.GetUserReviewAsync(dto.UserName, dto.MovieId);
            if (existing != null)
            {
            
                return null;
            }

            Review review = new Review
            {
                MovieId = dto.MovieId,
                UserName = dto.UserName,
                Score = dto.Score,
                Text = dto.Text
            };

            await _reviewRepo.AddAsync(review);

            await RecalculateMovieRating(dto.MovieId);

            return new ReviewOutputDto
            {
                Id = review.Id,
                MovieId = review.MovieId,
                UserName = review.UserName,
                Score = review.Score,
                Text = review.Text
            };
        }


        public async Task<ReviewOutputDto?> GetByIdAsync(int id)
        {
            Review? review = await _reviewRepo.GetByIdAsync(id);

            if (review == null)
                return null;

            return new ReviewOutputDto
            {
                Id = review.Id,
                MovieId = review.MovieId,
                UserName = review.UserName,
                Score = review.Score,
                Text = review.Text
            };
        }


        public async Task<List<ReviewOutputDto>> GetByMovieAsync(int movieId)
        {
            List<Review> list = await _reviewRepo.GetByMovieAsync(movieId);

            return list.Select(r => new ReviewOutputDto
            {
                Id = r.Id,
                MovieId = r.MovieId,
                UserName = r.UserName,
                Score = r.Score,
                Text = r.Text
            }).ToList();
        }

   
        public async Task<ReviewOutputDto?> UpdateAsync(int id, ReviewInputDto dto)
        {
            Review? review = await _reviewRepo.GetByIdAsync(id);
            if (review == null)
                return null;

            if (dto.Score < 1 || dto.Score > 10)
                return null;

            review.Score = dto.Score;
            review.Text = dto.Text;

            await _reviewRepo.UpdateAsync(review);

            await RecalculateMovieRating(review.MovieId);

            return new ReviewOutputDto
            {
                Id = review.Id,
                MovieId = review.MovieId,
                UserName = review.UserName,
                Score = review.Score,
                Text = review.Text
            };
        }


        public async Task<bool> DeleteAsync(int id)
        {
            Review? review = await _reviewRepo.GetByIdAsync(id);
            if (review == null)
                return false;

            int movieId = review.MovieId;

            await _reviewRepo.DeleteAsync(review);

            await RecalculateMovieRating(movieId);

            return true;
        }


        private async Task RecalculateMovieRating(int movieId)
        {
            Movie? movie = await _movieRepo.GetByIdAsync(movieId);
            if (movie == null)
                throw new ArgumentException("Couldn't find film");

            List<Review> reviews = await _reviewRepo.GetByMovieAsync(movieId);

            if (reviews.Count == 0)
            {
                movie.Rating = 0;
                movie.VotesCount = 0;
            }
            else
            {
                movie.VotesCount = reviews.Count;
                movie.Rating = reviews.Average(r => r.Score);
            }

            await _movieRepo.UpdateAsync(movie);
        }
    }

}
