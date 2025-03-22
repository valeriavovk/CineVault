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
public sealed class ReviewsController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<ReviewsController> _logger;
    private readonly IMapper _mapper;

    public ReviewsController(CineVaultDbContext dbContext, ILogger<ReviewsController> logger, IMapper mapper)
    {
        this.dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1")]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        _logger.LogInformation("GetReviews method called");
        var reviews = await dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .Select(r => new ReviewResponse
            {
                Id = r.Id,
                MovieId = r.MovieId,
                MovieTitle = r.Movie!.Title,
                UserId = r.UserId,
                Username = r.User!.Username,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
        _logger.LogInformation("GetReviews executed successfully. Returned {Count} reviews", reviews.Count);
        return Ok(reviews);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        _logger.LogInformation("GetReviewById method called with id {ReviewId}", id);
        var review = await dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            _logger.LogError("Review with id {ReviewId} not found", id);
            return NotFound();
        }
        var response = new ReviewResponse
        {
            Id = review.Id,
            MovieId = review.MovieId,
            MovieTitle = review.Movie!.Title,
            UserId = review.UserId,
            Username = review.User!.Username,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
        _logger.LogInformation("GetReviewById executed successfully for review id {ReviewId}", id);
        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("1")]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        _logger.LogInformation("CreateReview method called for MovieId {MovieId} and UserId {UserId}", request.MovieId, request.UserId);
        var review = new Review
        {
            MovieId = request.MovieId,
            UserId = request.UserId,
            Rating = request.Rating,
            Comment = request.Comment
        };
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Review created successfully with Id {ReviewId} for MovieId {MovieId} and UserId {UserId}", review.Id, review.MovieId, review.UserId);
        return Created(string.Empty, null);
    }

    [HttpPut("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        _logger.LogInformation("UpdateReview method called for review id {ReviewId}", id);
        var review = await dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.LogError("Review with id {ReviewId} not found for update", id);
            return NotFound();
        }
        review.MovieId = request.MovieId;
        review.UserId = request.UserId;
        review.Rating = request.Rating;
        review.Comment = request.Comment;
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Review updated successfully with Id {ReviewId}", review.Id);
        return Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        _logger.LogInformation("DeleteReview method called for review id {ReviewId}", id);
        var review = await dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.LogError("Review with id {ReviewId} not found for deletion", id);
            return NotFound();
        }
        dbContext.Reviews.Remove(review);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("Review deleted successfully with Id {ReviewId}", review.Id);
        return Ok();
    }

    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<List<ReviewResponse>>>> GetReviews(ApiRequest request)
    {
        var reviews = await dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .ToListAsync();
        var reviewResponses = _mapper.Map<List<ReviewResponse>>(reviews);
        var response = new ApiResponse<List<ReviewResponse>>
        {
            StatusCode = 200,
            Message = "OK",
            Data = reviewResponses
        };
        return Ok(response);
    }

    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> GetReviewById(int id, ApiRequest request)
    {
        var review = await dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            return NotFound(new ApiResponse<ReviewResponse>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = default!
            });
        }
        var reviewResponse = _mapper.Map<ReviewResponse>(review);
        var response = new ApiResponse<ReviewResponse>
        {
            StatusCode = 200,
            Message = "OK",
            Data = reviewResponse
        };
        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> CreateReview(ApiRequest<ReviewRequest> request)
    {
        var reviewRequest = request.Data;
        var review = _mapper.Map<Review>(reviewRequest);
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync();
        var reviewResponse = _mapper.Map<ReviewResponse>(review);
        var response = new ApiResponse<ReviewResponse>
        {
            StatusCode = 201,
            Message = "Created",
            Data = reviewResponse
        };
        return CreatedAtAction(nameof(GetReviewById), new { id = review.Id }, response);
    }

    [HttpPut("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReview(int id, ApiRequest<ReviewRequest> request)
    {
        var reviewRequest = request.Data;
        var review = await dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            return NotFound(new ApiResponse<ReviewResponse>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = default!
            });
        }
        _mapper.Map(reviewRequest, review);
        await dbContext.SaveChangesAsync();
        var reviewResponse = _mapper.Map<ReviewResponse>(review);
        var response = new ApiResponse<ReviewResponse>
        {
            StatusCode = 200,
            Message = "OK",
            Data = reviewResponse
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteReview(int id, ApiRequest request)
    {
        var review = await dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = "Review not found"
            });
        }
        dbContext.Reviews.Remove(review);
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
