using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/[controller]/[action]")]
public sealed class MoviesController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(CineVaultDbContext dbContext, ILogger<MoviesController> logger)
    {
        this.dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovieResponse>>> GetMovies()
    {
        _logger.LogInformation("GetMovies method called");
        var movies = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Select(m => new MovieResponse
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseDate = m.ReleaseDate,
                Genre = m.Genre,
                Director = m.Director,
                AverageRating = m.Reviews.Count != 0 ? m.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = m.Reviews.Count
            })
            .ToListAsync();
        _logger.LogInformation("GetMovies executed successfully with {MovieCount} movies", movies.Count);
        return base.Ok(movies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieResponse>> GetMovieById(int id)
    {
        _logger.LogInformation("GetMovieById method called with Id {MovieId}", id);
        var movie = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie is null)
        {
            _logger.LogError("Movie with Id {MovieId} not found", id);
            return base.NotFound();
        }
        var response = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = movie.Reviews.Count != 0 ? movie.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = movie.Reviews.Count
        };
        _logger.LogInformation("GetMovieById executed successfully for MovieId {MovieId} with {ReviewCount} reviews", movie.Id, movie.Reviews.Count);
        return base.Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateMovie(MovieRequest request)
    {
        _logger.LogInformation("CreateMovie method called with Title {MovieTitle}", request.Title);
        var movie = new Movie
        {
            Title = request.Title,
            Description = request.Description,
            ReleaseDate = request.ReleaseDate,
            Genre = request.Genre,
            Director = request.Director
        };
        await this.dbContext.Movies.AddAsync(movie);
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("Movie created successfully with Id {MovieId} and Title {MovieTitle}", movie.Id, movie.Title);
        return base.Created();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateMovie(int id, MovieRequest request)
    {
        _logger.LogInformation("UpdateMovie method called with Id {MovieId}", id);
        var movie = await this.dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            _logger.LogError("Movie with Id {MovieId} not found for update", id);
            return base.NotFound();
        }
        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Genre = request.Genre;
        movie.Director = request.Director;
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("Movie updated successfully with Id {MovieId} and Title {MovieTitle}", movie.Id, movie.Title);
        return base.Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        _logger.LogInformation("DeleteMovie method called with Id {MovieId}", id);
        var movie = await this.dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            _logger.LogError("Movie with Id {MovieId} not found for deletion", id);
            return base.NotFound();
        }
        this.dbContext.Movies.Remove(movie);
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("Movie deleted successfully with Id {MovieId}", movie.Id);
        return base.Ok();
    }
}
