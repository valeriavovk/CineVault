using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/v{v:apiVersion}/[controller]/[action]")]
[ApiVersion(1)]
[ApiVersion(2)]
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
    [MapToApiVersion("1")]
    public async Task<ActionResult<List<MovieResponse>>> GetMovies()
    {
        _logger.LogInformation("GetMovies method called");
        var movies = await dbContext.Movies
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
        return Ok(movies);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult<MovieResponse>> GetMovieById(int id)
    {
        _logger.LogInformation("GetMovieById method called with Id {MovieId}", id);
        var movie = await dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie is null)
        {
            _logger.LogError("Movie with Id {MovieId} not found", id);
            return NotFound();
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
        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("1")]
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
        await dbContext.Movies.AddAsync(movie);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Movie created successfully with Id {MovieId} and Title {MovieTitle}", movie.Id, movie.Title);
        return Created(string.Empty, null);
    }

    [HttpPut("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> UpdateMovie(int id, MovieRequest request)
    {
        _logger.LogInformation("UpdateMovie method called with Id {MovieId}", id);
        var movie = await dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            _logger.LogError("Movie with Id {MovieId} not found for update", id);
            return NotFound();
        }
        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Genre = request.Genre;
        movie.Director = request.Director;
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Movie updated successfully with Id {MovieId} and Title {MovieTitle}", movie.Id, movie.Title);
        return Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        _logger.LogInformation("DeleteMovie method called with Id {MovieId}", id);
        var movie = await dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            _logger.LogError("Movie with Id {MovieId} not found for deletion", id);
            return NotFound();
        }
        dbContext.Movies.Remove(movie);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Movie deleted successfully with Id {MovieId}", movie.Id);
        return Ok();
    }

    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> GetMovies(ApiRequest request)
    {
        var movies = await dbContext.Movies
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
        var response = new ApiResponse<List<MovieResponse>>
        {
            StatusCode = 200,
            Message = "OK",
            Data = movies
        };
        return Ok(response);
    }

    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieById(int id, ApiRequest request)
    {
        var movie = await dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie == null)
        {
            return NotFound(new ApiResponse<MovieResponse>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = default!
            });
        }
        var movieResponse = new MovieResponse
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
        var response = new ApiResponse<MovieResponse>
        {
            StatusCode = 200,
            Message = "OK",
            Data = movieResponse
        };
        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> CreateMovie(ApiRequest<MovieRequest> request)
    {
        var movieRequest = request.Data;
        var movie = new Movie
        {
            Title = movieRequest.Title,
            Description = movieRequest.Description,
            ReleaseDate = movieRequest.ReleaseDate,
            Genre = movieRequest.Genre,
            Director = movieRequest.Director
        };
        await dbContext.Movies.AddAsync(movie);
        await dbContext.SaveChangesAsync();
        var movieResponse = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            ReleaseDate = movie.ReleaseDate,
            Genre = movie.Genre,
            Director = movie.Director,
            AverageRating = 0,
            ReviewCount = 0
        };
        var response = new ApiResponse<MovieResponse>
        {
            StatusCode = 201,
            Message = "Created",
            Data = movieResponse
        };
        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, response);
    }

    [HttpPut("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> UpdateMovie(int id, ApiRequest<MovieRequest> request)
    {
        var movieRequest = request.Data;
        var movie = await dbContext.Movies.FindAsync(id);
        if (movie == null)
        {
            return NotFound(new ApiResponse<MovieResponse>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = default!
            });
        }
        movie.Title = movieRequest.Title;
        movie.Description = movieRequest.Description;
        movie.ReleaseDate = movieRequest.ReleaseDate;
        movie.Genre = movieRequest.Genre;
        movie.Director = movieRequest.Director;
        await dbContext.SaveChangesAsync();
        var movieResponse = new MovieResponse
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
        var response = new ApiResponse<MovieResponse>
        {
            StatusCode = 200,
            Message = "OK",
            Data = movieResponse
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteMovie(int id, ApiRequest request)
    {
        var movie = await dbContext.Movies.FindAsync(id);
        if (movie == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = "Movie not found"
            });
        }
        dbContext.Movies.Remove(movie);
        await dbContext.SaveChangesAsync();
        var response = new ApiResponse<string>
        {
            StatusCode = 200,
            Message = "OK",
            Data = "Deleted"
        };
        return Ok(response);
    }
}
