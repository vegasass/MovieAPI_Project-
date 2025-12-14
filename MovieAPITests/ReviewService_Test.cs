using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MovieAPI.Dto;
using MovieAPI.Models;
using MovieAPI.Repositories;
using MovieAPI.Services;

namespace MovieAPITests
{
    public class ReviewService_Test
    {
        private readonly Mock<IReviewRepository> _reviewRepoMock;
        private readonly Mock<IMovieRepository> _movieRepoMock;
        private readonly ReviewService _service;

        public ReviewService_Test()
        {
            _reviewRepoMock = new Mock<IReviewRepository>();
            _movieRepoMock = new Mock<IMovieRepository>();

            ILogger<ReviewService> logger = NullLogger<ReviewService>.Instance;

            _service = new ReviewService(
                _reviewRepoMock.Object,
                _movieRepoMock.Object,
                logger
            );
        }

  
        [Fact]
        public async Task CreateAsync_CreateReview_WhenValid()
        {
            Movie movie = new Movie { Id = 10 };
            _movieRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(movie);

            _reviewRepoMock.Setup(r => r.GetUserReviewAsync("u", 10))
                .ReturnsAsync((Review?)null);

            List<Review> reviewsFirstAdd = new List<Review>
            {
                new Review { MovieId = 10, Score = 7 }
            };
            List<Review> reviewsSecondAdd = new List<Review>
            {
                new Review { MovieId = 10, Score = 7 },
                new Review { MovieId = 10, Score = 2 }
            };
            _reviewRepoMock.SetupSequence(r => r.GetByMovieAsync(10))
                .ReturnsAsync(reviewsFirstAdd)
                .ReturnsAsync(reviewsSecondAdd);
                
            ReviewInputDto dto = new ReviewInputDto
            {
                MovieId = 10,
                UserName = "u",
                Score = 7,
                Text = "nice"
            };
            ReviewInputDto secondDto = new ReviewInputDto
            {
                MovieId = 10,
                UserName = "y",
                Score = 3,
                Text = "bad"
            };

            ReviewOutputDto? firstResult = await _service.CreateAsync(dto);

            Assert.NotNull(firstResult);
            Assert.Equal(7, movie.Rating);
            Assert.Equal(1, movie.VotesCount);

            ReviewOutputDto? secondResult = await _service.CreateAsync(dto);

            Assert.NotNull(secondResult);
            Assert.Equal(4.5, movie.Rating);
            Assert.Equal(2, movie.VotesCount);

            _reviewRepoMock.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.AtLeastOnce);
            _movieRepoMock.Verify(r => r.UpdateAsync(movie), Times.AtLeastOnce);
            _reviewRepoMock.Verify(r => r.GetByMovieAsync(10), Times.AtLeastOnce);

        }


        [Fact]
        public async Task CreateAsync_ReturnNull_WhenUserAlreadyReviewed()
        {
            _movieRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(new Movie { Id = 10 });

            _reviewRepoMock.Setup(r => r.GetUserReviewAsync("u", 10))
                .ReturnsAsync(new Review());

            ReviewInputDto dto = new ReviewInputDto
            {
                MovieId = 10,
                UserName = "u",
                Score = 5
            };

            ReviewOutputDto? result = await _service.CreateAsync(dto);

            Assert.Null(result);
        }


        [Fact]
        public async Task GetByIdAsync_ReturnReview_WhenExists()
        {
            Review review = new Review
            {
                Id = 1,
                MovieId = 10,
                Score = 8
            };

            _reviewRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(review);

            ReviewOutputDto? result = await _service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(8, result.Score);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnNull_WhenNotFound()
        {
            _reviewRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Review?)null);

            ReviewOutputDto? result = await _service.GetByIdAsync(1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByMovieAsync_ReturnReviewList()
        {
            List<Review> reviews = new List<Review>
            {
                new Review { Id = 1, MovieId = 10, Score = 7 },
                new Review { Id = 2, MovieId = 10, Score = 9 }
            };

            _reviewRepoMock.Setup(r => r.GetByMovieAsync(10))
                .ReturnsAsync(reviews);

            List<ReviewOutputDto> result = await _service.GetByMovieAsync(10);

            Assert.Equal(2, result.Count);
        }


        [Fact]
        public async Task GetByMovieAsync_ReturnEmptyList_WhenNoReviews()
        {
            _reviewRepoMock.Setup(r => r.GetByMovieAsync(10))
                .ReturnsAsync(new List<Review>());

            List<ReviewOutputDto> result = await _service.GetByMovieAsync(10);

            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateAsync_UpdateReview_AndRecalculateRating()
        {
            Movie movie = new Movie { Id = 10 };

            Review review = new Review
            {
                Id = 1,
                MovieId = 10,
                Score = 5
            };

            _reviewRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(review);

            _movieRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(movie);

            _reviewRepoMock.Setup(r => r.GetByMovieAsync(10))
                .ReturnsAsync(new List<Review>
                {
                    new Review { Score = 9 }
                });

            ReviewInputDto dto = new ReviewInputDto
            {
                Score = 9,
                Text = "Updated"
            };

            ReviewOutputDto? result = await _service.UpdateAsync(1, dto);

            Assert.NotNull(result);
            Assert.Equal(9, movie.Rating);
            Assert.Equal(1, movie.VotesCount);
        }

        [Fact]
        public async Task UpdateAsync_ReturnNull_WhenScoreInvalid()
        {
            Review review = new Review { Id = 1, MovieId = 10 };

            _reviewRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(review);

            ReviewInputDto dto = new ReviewInputDto
            {
                Score = 0
            };

            ReviewOutputDto? result = await _service.UpdateAsync(1, dto);

            Assert.Null(result);
        }


        [Fact]
        public async Task UpdateAsync_ReturnNull_WhenReviewNotFound()
        {
            _reviewRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Review?)null);

            ReviewInputDto dto = new ReviewInputDto();

            ReviewOutputDto? result = await _service.UpdateAsync(1, dto);

            Assert.Null(result);
        }
        [Fact]
        public async Task DeleteAsync_DeleteReview_AndRecalculateRating()
        {
            Movie movie = new Movie { Id = 10 };

            Review review = new Review
            {
                Id = 1,
                MovieId = 10
            };

            _reviewRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(review);

            _movieRepoMock.Setup(r => r.GetByIdAsync(10))
                .ReturnsAsync(movie);

            _reviewRepoMock.Setup(r => r.GetByMovieAsync(10))
                .ReturnsAsync(new List<Review>());

            bool result = await _service.DeleteAsync(1);

            Assert.True(result);
            Assert.Equal(0, movie.Rating);
            Assert.Equal(0, movie.VotesCount);
        }


        [Fact]
        public async Task DeleteAsync_ReturnFalse_WhenReviewNotFound()
        {
            _reviewRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Review?)null);

            bool result = await _service.DeleteAsync(1);

            Assert.False(result);
        }
    }
}
