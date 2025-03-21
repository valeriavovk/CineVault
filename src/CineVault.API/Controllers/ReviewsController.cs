using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/[controller]/[action]")]
public sealed class ReviewsController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(CineVaultDbContext dbContext, ILogger<ReviewsController> logger)
    {
        this.dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        _logger.LogInformation("GetReviews method called");
        var reviews = await this.dbContext.Reviews
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
        return base.Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        _logger.LogInformation("GetReviewById method called with id {ReviewId}", id);
        var review = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            _logger.LogError("Review with id {ReviewId} not found", id);
            return base.NotFound();
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
        return base.Ok(response);
    }

    [HttpPost]
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
        this.dbContext.Reviews.Add(review);
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("Review created successfully with Id {ReviewId} for MovieId {MovieId} and UserId {UserId}", review.Id, review.MovieId, review.UserId);
        return base.Created();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        _logger.LogInformation("UpdateReview method called for review id {ReviewId}", id);
        var review = await this.dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.LogError("Review with id {ReviewId} not found for update", id);
            return base.NotFound();
        }
        review.MovieId = request.MovieId;
        review.UserId = request.UserId;
        review.Rating = request.Rating;
        review.Comment = request.Comment;
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("Review updated successfully with Id {ReviewId}", review.Id);
        return base.Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        _logger.LogInformation("DeleteReview method called for review id {ReviewId}", id);
        var review = await this.dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            _logger.LogError("Review with id {ReviewId} not found for deletion", id);
            return base.NotFound();
        }
        this.dbContext.Reviews.Remove(review);
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("Review deleted successfully with Id {ReviewId}", review.Id);
        return base.Ok();
    }
}
