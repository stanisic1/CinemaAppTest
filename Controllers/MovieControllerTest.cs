using CinemaApp.Controllers;
using CinemaApp.Interfaces;
using CinemaApp.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaAppTest.Controllers
{
    public class MovieControllerTest
    {
        private readonly Mock<IMovieRepository> _mockRepo;
        private readonly MovieController _controller;

        public MovieControllerTest()
        {
            _mockRepo = new Mock<IMovieRepository>();
            _controller = new MovieController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetMovies_ReturnsOkResult_WithMovies()
        {
            // Arrange
            var mockMovies = new List<Movie>
        {
            new Movie { Title = "Movie1", Genre = "Action", Distributor = "Distributor1", CountryOrigin = "USA", Duration = 120, ReleaseYear = 2020 },
            new Movie { Title = "Movie2", Genre = "Drama", Distributor = "Distributor2", CountryOrigin = "UK", Duration = 100, ReleaseYear = 2019 }
        };

            _mockRepo.Setup(repo => repo.GetAllAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string>()
            )).ReturnsAsync(mockMovies);

            // Act
            var result = await _controller.GetMovies(null, null, null, null, null, null, null, null, null) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var returnMovies = Assert.IsType<List<Movie>>(result.Value);
            Assert.Equal(2, returnMovies.Count);
            Assert.Equal("Movie1", returnMovies[0].Title);
            Assert.Equal("Movie2", returnMovies[1].Title);
        }

        [Fact]
        public async Task GetMovies_ReturnsOkResult_WithEmptyList_WhenNoMoviesFound()
        {
            // Arrange
            var emptyMovieList = new List<Movie>();
            _mockRepo.Setup(repo => repo.GetAllAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string>()
            )).ReturnsAsync(emptyMovieList);

            // Act
            var result = await _controller.GetMovies(null, null, null, null, null, null, null, null, null) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var returnMovies = Assert.IsType<List<Movie>>(result.Value);
            Assert.Empty(returnMovies);
        }

    }
}
