﻿using CinemaApp.Controllers;
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

        [Fact]
        public async Task GetMovie_ReturnsOkResult_WithMovie_WhenMovieExists()
        {
            // Arrange
            var mockMovie = new Movie
            {
                Title = "Movie1",
                Genre = "Action",
                Distributor = "Distributor1",
                CountryOrigin = "USA",
                Duration = 120,
                ReleaseYear = 2020
            };

            _mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(mockMovie);

            // Act
            var result = await _controller.GetMovie(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);
            var returnMovie = Assert.IsType<Movie>(result.Value);
            Assert.Equal("Movie1", returnMovie.Title);
        }

        [Fact]
        public async Task GetMovie_ReturnsNotFoundResult_WhenMovieDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Movie?)null);

            // Act
            var result = await _controller.GetMovie(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PostMovie_ReturnsCreatedAtAction_WhenModelIsValid()
        {
            // Arrange
            var newMovie = new Movie
            {
                Title = "New Movie",
                Genre = "Action",
                Distributor = "Distributor1",
                CountryOrigin = "USA",
                Duration = 120,
                ReleaseYear = 2022
            };

            _mockRepo.Setup(repo => repo.AddAsync(It.IsAny<Movie>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PostMovie(newMovie) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetMovie", result.ActionName);
            Assert.Equal(newMovie.Id, result.RouteValues["id"]);
            Assert.Equal(newMovie, result.Value);
        }

        [Fact]
        public async Task PostMovie_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "The Title field is required.");

            var invalidMovie = new Movie
            {
                // Missing Title to simulate invalid state
                Genre = "Action",
                Distributor = "Distributor1",
                CountryOrigin = "USA",
                Duration = 120,
                ReleaseYear = 2022
            };

            // Act
            var result = await _controller.PostMovie(invalidMovie) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(result.Value);

            // Use reflection to get the anonymous type properties
            var valueType = result.Value.GetType();

            // Get the 'Message' property
            var messageProperty = valueType.GetProperty("Message");
            Assert.NotNull(messageProperty);
            var messageValue = messageProperty.GetValue(result.Value) as string;
            Assert.Equal("Invalid model state.", messageValue);

            // Get the 'Errors' property
            var errorsProperty = valueType.GetProperty("Errors");
            Assert.NotNull(errorsProperty);
            var errorsValue = errorsProperty.GetValue(result.Value) as List<string>;
            Assert.NotNull(errorsValue);
            Assert.Contains("The Title field is required.", errorsValue);
        }

        [Fact]
        public async Task PutMovie_ReturnsOk_WhenMovieIsUpdatedSuccessfully()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Sample Movie" };
            _mockRepo.Setup(repo => repo.UpdateAsync(movie)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PutMovie(movie.Id, movie);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMovie = Assert.IsType<Movie>(okResult.Value);
            Assert.Equal(movie, returnedMovie);
        }

        [Fact]
        public async Task PutMovie_ReturnsBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Sample Movie" };
            _mockRepo.Setup(repo => repo.UpdateAsync(movie)).ThrowsAsync(new Exception("Update failed"));

            // Act
            var result = await _controller.PutMovie(movie.Id, movie);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to update telephone. Error: Update failed", badRequestResult.Value);
        }

        [Fact]
        public async Task PutMovie_ReturnsBadRequest_WhenIdDoesNotMatchMovieId()
        {
            // Arrange
            var movie = new Movie { Id = 1, Title = "Sample Movie" };

            // Act
            var result = await _controller.PutMovie(2, movie);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteMovie_PerformsLogicalDelete_WhenMovieHasProjections()
        {
            // Arrange
            int movieId = 1;
            var movie = new Movie { Id = movieId, Title = "Sample Movie" };
            _mockRepo.Setup(repo => repo.GetByIdAsync(movieId)).ReturnsAsync(movie);
            _mockRepo.Setup(repo => repo.HasProjectionsAsync(movieId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteMovie(movieId);

            // Assert
            _mockRepo.Verify(repo => repo.LogicalDeleteAsync(movie), Times.Once);
            _mockRepo.Verify(repo => repo.DeleteAsync(movie), Times.Never);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteMovie_PerformsPhysicalDelete_WhenMovieHasNoProjections()
        {
            // Arrange
            int movieId = 1;
            var movie = new Movie { Id = movieId, Title = "Sample Movie" };
            _mockRepo.Setup(repo => repo.GetByIdAsync(movieId)).ReturnsAsync(movie);
            _mockRepo.Setup(repo => repo.HasProjectionsAsync(movieId)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteMovie(movieId);

            // Assert
            _mockRepo.Verify(repo => repo.DeleteAsync(movie), Times.Once);
            _mockRepo.Verify(repo => repo.LogicalDeleteAsync(movie), Times.Never);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteMovie_ReturnsNotFound_WhenMovieDoesNotExist()
        {
            // Arrange
            int movieId = 1;
            _mockRepo.Setup(repo => repo.GetByIdAsync(movieId)).ReturnsAsync((Movie)null);

            // Act
            var result = await _controller.DeleteMovie(movieId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

    }
}
