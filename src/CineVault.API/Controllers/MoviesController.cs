using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/v{v:apiVersion}/[controller]/[action]")]
[ApiVersion("1")]
[ApiVersion("2")]
public sealed class MoviesController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<MoviesController> logger;
    private readonly IMapper mapper;

    public MoviesController(CineVaultDbContext dbContext, ILogger<MoviesController> logger,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1")]
    public async Task<ActionResult<List<MovieResponse>>> GetMovies()
    {
        this.logger.LogInformation("GetMovies method called");
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
        this.logger.LogInformation("GetMovies executed successfully with {MovieCount} movies",
            movies.Count);
        return this.Ok(movies);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult<MovieResponse>> GetMovieById(int id)
    {
        this.logger.LogInformation("GetMovieById method called with Id {MovieId}", id);
        var movie = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (movie is null)
        {
            this.logger.LogError("Movie with Id {MovieId} not found", id);
            return this.NotFound();
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
        this.logger.LogInformation(
            "GetMovieById executed successfully for MovieId {MovieId} with {ReviewCount} reviews",
            movie.Id, movie.Reviews.Count);
        return this.Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("1")]
    public async Task<ActionResult> CreateMovie(MovieRequest request)
    {
        this.logger.LogInformation("CreateMovie method called with Title {MovieTitle}",
            request.Title);
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
        this.logger.LogInformation(
            "Movie created successfully with Id {MovieId} and Title {MovieTitle}", movie.Id,
            movie.Title);
        return this.Created(string.Empty, null);
    }

    [HttpPut("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> UpdateMovie(int id, MovieRequest request)
    {
        this.logger.LogInformation("UpdateMovie method called with Id {MovieId}", id);
        var movie = await this.dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            this.logger.LogError("Movie with Id {MovieId} not found for update", id);
            return this.NotFound();
        }

        movie.Title = request.Title;
        movie.Description = request.Description;
        movie.ReleaseDate = request.ReleaseDate;
        movie.Genre = request.Genre;
        movie.Director = request.Director;
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation(
            "Movie updated successfully with Id {MovieId} and Title {MovieTitle}", movie.Id,
            movie.Title);
        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> DeleteMovie(int id)
    {
        this.logger.LogInformation("DeleteMovie method called with Id {MovieId}", id);
        var movie = await this.dbContext.Movies.FindAsync(id);
        if (movie is null)
        {
            this.logger.LogError("Movie with Id {MovieId} not found for deletion", id);
            return this.NotFound();
        }

        this.dbContext.Movies.Remove(movie);
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("Movie deleted successfully with Id {MovieId}", movie.Id);
        return this.Ok();
    }

    // завдання 13.2
    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ICollection<MovieResponse>>>> GetMovies(ApiRequest request)
    {
        this.logger.LogInformation("GetMovies (v2) method called");

        var movies = await this.dbContext.Movies
            .Select(m => new 
            {
                Movie = m,
                AverageRating = m.Reviews.Average(r => (double?)r.Rating) ?? 0,
                ReviewCount = m.Reviews.Count
            })
            .AsNoTracking()
            .ToListAsync();

        var movieResponses = movies.Select(x => new MovieResponse 
        {
            Id = x.Movie.Id,
            Title = x.Movie.Title,
            Description = x.Movie.Description,
            ReleaseDate = x.Movie.ReleaseDate,
            Genre = x.Movie.Genre,
            Director = x.Movie.Director,
            AverageRating = x.AverageRating,
            ReviewCount = x.ReviewCount
        }).ToList();

        return this.Ok(new ApiResponse<ICollection<MovieResponse>> 
            { StatusCode = 200, Message = "Movies are received", Data = movieResponses });
    }

    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ICollection<MovieResponse>>>> SearchMovies(
        ApiRequest<SearchMoviesRequest> request)
    {
        this.logger.LogInformation("SearchMovies (v2) method called with criteria: {@Criteria}",
            request.Data);

        var criteria = request.Data;
        var query = this.dbContext.Movies
            .Include(m => m.Reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.Title))
        {
            query = query.Where(m => m.Title.Contains(criteria.Title));
            this.logger.LogInformation("Filtering movies by Title containing: {Title}",
                criteria.Title);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Genre))
        {
            query = query.Where(m =>
                m.Genre != null &&
                m.Genre.Equals(criteria.Genre, StringComparison.OrdinalIgnoreCase));
            this.logger.LogInformation("Filtering movies by Genre: {Genre}", criteria.Genre);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Director))
        {
            query = query.Where(m => m.Director != null && m.Director.Contains(criteria.Director));
            this.logger.LogInformation("Filtering movies by Director containing: {Director}",
                criteria.Director);
        }

        if (criteria.ReleaseDate.HasValue)
        {
            query = query.Where(m =>
                m.ReleaseDate.HasValue && m.ReleaseDate.Value == criteria.ReleaseDate.Value);
            this.logger.LogInformation("Filtering movies by ReleaseDate: {ReleaseDate}",
                criteria.ReleaseDate.Value);
        }

        if (criteria.AvgRating.HasValue)
        {
            double minRating = criteria.AvgRating.Value;
            query = query.Where(m =>
                m.Reviews.Any() && m.Reviews.Average(r => r.Rating) >= minRating);
            this.logger.LogInformation("Filtering movies with average rating >= {MinRating}",
                minRating);
        }

        var movies = await query.ToListAsync();
        this.logger.LogInformation(
            "SearchMovies (v2) executed successfully. Found {MovieCount} movies", movies.Count);

        var movieResponses = this.mapper.Map<ICollection<MovieResponse>>(movies);
        return this.Ok(new ApiResponse<ICollection<MovieResponse>>
        {
            StatusCode = 200, Message = "Movies are received", Data = movieResponses
        });
    }

    // завдання 13.4
    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<List<MovieResponse>>>> SearchMoviesAlt(
        ApiRequest<SearchMoviesAltRequest> request)
    {
        this.logger.LogInformation("SearchMoviesAlt (v2) called with filters: {@Filters}", request.Data);

        var query = this.dbContext.Movies
            .Include(m => m.Reviews)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Data.Text))
        {
            string searchTerm = request.Data.Text.ToLower();
            query = query.Where(m =>
                m.Title.ToLower().Contains(searchTerm) ||
                (m.Description != null && m.Description.ToLower().Contains(searchTerm)) ||
                (m.Director != null && m.Director.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Data.Genre))
        {
            query = query.Where(m => m.Genre == request.Data.Genre);
        }

        if (request.Data.MinRating.HasValue)
        {
            query = query.Where(m =>
                m.Reviews.Any() &&
                m.Reviews.Average(r => r.Rating) >= request.Data.MinRating.Value);
        }

        if (request.Data.ReleaseDate.HasValue)
        {
            query = query.Where(m => m.ReleaseDate == request.Data.ReleaseDate);
        }

        var movies = await query.ToListAsync();
        var response = this.mapper.Map<List<MovieResponse>>(movies);

        this.logger.LogInformation("Found {Count} movies matching filters", response.Count);
        return this.Ok(new ApiResponse<List<MovieResponse>>
        {
            StatusCode = 200, Message = "Movies are received", Data = response
        });
    }

    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<MovieDetailsResponse>>> GetMovieDetails(int id, ApiRequest request)
    {
        this.logger.LogInformation("GetMovieDetails (v2) called for MovieId: {MovieId}", id);

        var result = await this.dbContext.Movies
            .Where(m => m.Id == id)
            .Select(m => new 
            {
                Movie = m,
                AverageRating = m.Reviews.Average(r => (double?)r.Rating) ?? 0,
                ReviewCount = m.Reviews.Count,
                LastReviews = m.Reviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new MovieDetailsResponse.ReviewUserResponse 
                    {
                        ReviewId = r.Id,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        User = new UserResponse 
                        { 
                            Id = r.User!.Id, 
                            Username = r.User.Username, 
                            Email = r.User.Email 
                        }
                    })
                    .ToList()
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (result == null) 
        {
            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "Movie is not found" });
        }

        var response = new MovieDetailsResponse
        {
            Id = result.Movie.Id,
            Title = result.Movie.Title,
            Description = result.Movie.Description,
            ReleaseDate = result.Movie.ReleaseDate,
            Genre = result.Movie.Genre,
            Director = result.Movie.Director,
            AverageRating = result.AverageRating,
            ReviewCount = result.ReviewCount,
            LastReviews = result.LastReviews
        };

        return this.Ok(new ApiResponse<MovieDetailsResponse> 
            { StatusCode = 200, Message = "Movie details are received", Data = response });
    }

    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> GetMovieById(int id,
        ApiRequest request)
    {
        this.logger.LogInformation("GetMovieById (v2) method called with Id {MovieId}", id);

        var movie = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
        {
            this.logger.LogWarning("GetMovieById (v2): Movie with Id {MovieId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404, Message = "Movie is not found"
            });
        }

        var movieResponse = this.mapper.Map<MovieResponse>(movie);
        this.logger.LogInformation("GetMovieById (v2) executed successfully for MovieId {MovieId}",
            id);
        return this.Ok(new ApiResponse<MovieResponse>
        {
            StatusCode = 200, Message = "OK", Data = movieResponse
        });
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<int>>> CreateMovie(ApiRequest<MovieRequest> request)
    {
        this.logger.LogInformation("CreateMovie (v2) method called with Title {MovieTitle}",
            request.Data.Title);

        var movie = this.mapper.Map<Movie>(request.Data);
        await this.dbContext.Movies.AddAsync(movie);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation(
            "Movie created successfully in CreateMovie (v2) with Id {MovieId}", movie.Id);
        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200, Message = "Movie is created", Data = movie.Id
        });
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ICollection<MovieResponse>>>> CreateMovies(
        ApiRequest<ICollection<MovieRequest>> request)
    {
        this.logger.LogInformation("CreateMovies (v2) method called with {MovieCount} movies",
            request.Data.Count);

        var movies = this.mapper.Map<ICollection<Movie>>(request.Data);
        await this.dbContext.Movies.AddRangeAsync(movies);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation(
            "CreateMovies (v2) executed successfully. Created movies with IDs: {MovieIds}",
            string.Join(", ", movies.Select(m => m.Id)));
        return this.Ok(new ApiResponse<ICollection<int>>
        {
            StatusCode = 200,
            Message = "Movies are created",
            Data = movies.Select(a => a.Id).ToList()
        });
    }

    [HttpPut("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<MovieResponse>>> UpdateMovie(int id,
        ApiRequest<MovieRequest> request)
    {
        this.logger.LogInformation("UpdateMovie (v2) method called for MovieId {MovieId}", id);

        var movie = await this.dbContext.Movies.FindAsync(id);
        if (movie == null)
        {
            this.logger.LogWarning("UpdateMovie (v2): Movie with Id {MovieId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404, Message = "Movie is not found"
            });
        }

        this.mapper.Map(request.Data, movie);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("UpdateMovie (v2) executed successfully for MovieId {MovieId}",
            id);
        var movieResponse = this.mapper.Map<MovieResponse>(movie);
        return this.Ok(new ApiResponse<MovieResponse>
        {
            StatusCode = 200, Message = "Movie is updated", Data = movieResponse
        });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse>> DeleteMovie(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteMovie (v2) method called with MovieId {MovieId}", id);

        var movie = await this.dbContext.Movies.FindAsync(id);
        if (movie == null)
        {
            this.logger.LogWarning("DeleteMovie (v2): Movie with Id {MovieId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404, Message = "Movie is not found"
            });
        }

        movie.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("DeleteMovie (v2) executed successfully for MovieId {MovieId}",
            id);
        return this.Ok(new ApiResponse { StatusCode = 200, Message = "Movie is deleted" });
    }

    [HttpDelete]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteMovies(
        ApiRequest<ICollection<int>> request)
    {
        var movieIdsToDelete = request.Data.Distinct().ToList();
        this.logger.LogInformation("DeleteMovies (v2) method called with MovieIds: {MovieIds}",
            string.Join(", ", movieIdsToDelete));

        var results = new List<string>();
        var moviesFromDb = await this.dbContext.Movies
            .Include(m => m.Reviews)
            .Where(m => movieIdsToDelete.Contains(m.Id))
            .ToListAsync();
        var movsToRemove = new List<Movie>();
        var foundIds = new HashSet<int>();

        foreach (var movie in moviesFromDb)
        {
            foundIds.Add(movie.Id);
            if (movie.Reviews.Count != 0)
            {
                string skipMessage =
                    $"Movie with ID {movie.Id} ('{movie.Title}') skipped: Has associated reviews";
                this.logger.LogWarning("DeleteMovies (v2): {SkipMessage}", skipMessage);
                results.Add(skipMessage);
            }
            else
            {
                movsToRemove.Add(movie);
            }
        }

        foreach (int id in movieIdsToDelete)
        {
            if (foundIds.Contains(id))
            {
                continue;
            }

            string notFoundMessage = $"Movie with ID {id} not found";
            this.logger.LogWarning("DeleteMovies (v2): {NotFoundMessage}", notFoundMessage);
            results.Add(notFoundMessage);
        }

        if (movsToRemove.Count != 0)
        {
            await this.dbContext.SaveChangesAsync();
            foreach (var movie in movsToRemove)
            {
                movie.IsDeleted = true;
                string deletedMessage = $"Movie with ID {movie.Id} deleted successfully.";
                this.logger.LogInformation("DeleteMovies (v2): {DeletedMessage}", deletedMessage);
                results.Add(deletedMessage);
            }
        }

        this.logger.LogInformation("DeleteMovies (v2) executed with {DeletedCount} movies deleted",
            movsToRemove.Count);
        return this.Ok(new ApiResponse<ICollection<string>>
        {
            StatusCode = 200,
            Message = $"Movies are deleted. Check data. Actually deleted: {movsToRemove.Count}.",
            Data = results
        });
    }
}