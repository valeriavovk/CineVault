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
    private readonly ILogger<ReviewsController> logger;
    private readonly IMapper mapper;

    public ReviewsController(CineVaultDbContext dbContext, ILogger<ReviewsController> logger,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1")]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews()
    {
        this.logger.LogInformation("GetReviews method called");
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
        this.logger.LogInformation("GetReviews executed successfully. Returned {Count} reviews",
            reviews.Count);
        return this.Ok(reviews);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult<ReviewResponse>> GetReviewById(int id)
    {
        this.logger.LogInformation("GetReviewById method called with id {ReviewId}", id);
        var review = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            this.logger.LogError("Review with id {ReviewId} not found", id);
            return this.NotFound();
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
        this.logger.LogInformation("GetReviewById executed successfully for review id {ReviewId}",
            id);
        return this.Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("1")]
    public async Task<ActionResult> CreateReview(ReviewRequest request)
    {
        this.logger.LogInformation(
            "CreateReview method called for MovieId {MovieId} and UserId {UserId}", request.MovieId,
            request.UserId);
        var review = new Review
        {
            MovieId = request.MovieId,
            UserId = request.UserId,
            Rating = request.Rating,
            Comment = request.Comment
        };
        this.dbContext.Reviews.Add(review);
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation(
            "Review created successfully with Id {ReviewId} for MovieId {MovieId} and UserId {UserId}",
            review.Id, review.MovieId, review.UserId);
        return this.Created(string.Empty, null);
    }

    [HttpPut("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> UpdateReview(int id, ReviewRequest request)
    {
        this.logger.LogInformation("UpdateReview method called for review id {ReviewId}", id);
        var review = await this.dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            this.logger.LogError("Review with id {ReviewId} not found for update", id);
            return this.NotFound();
        }

        review.MovieId = request.MovieId;
        review.UserId = request.UserId;
        review.Rating = request.Rating;
        review.Comment = request.Comment;
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("Review updated successfully with Id {ReviewId}", review.Id);
        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> DeleteReview(int id)
    {
        this.logger.LogInformation("DeleteReview method called for review id {ReviewId}", id);
        var review = await this.dbContext.Reviews.FindAsync(id);
        if (review is null)
        {
            this.logger.LogError("Review with id {ReviewId} not found for deletion", id);
            return this.NotFound();
        }

        this.dbContext.Reviews.Remove(review);
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("Review deleted successfully with Id {ReviewId}", review.Id);
        return this.Ok();
    }

    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ICollection<ReviewResponse>>>> GetReviews(ApiRequest request)
    {
        this.logger.LogInformation("GetReviews (v2) method called");

        var reviews = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .ToListAsync();
        this.logger.LogInformation("Retrieved {ReviewCount} reviews from database", reviews.Count);

        var reviewResponses = this.mapper.Map<ICollection<ReviewResponse>>(reviews);

        this.logger.LogInformation("GetReviews (v2) executed successfully");
        return this.Ok(new ApiResponse<ICollection<ReviewResponse>>
        {
            StatusCode = 200,
            Message = "Reviews are received",
            Data = reviewResponses
        });
    }

    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> GetReviewById(int id, ApiRequest request)
    {
        this.logger.LogInformation("GetReviewById (v2) method called with id {ReviewId}", id);

        var review = await this.dbContext.Reviews
            .Include(r => r.Movie)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null)
        {
            this.logger.LogWarning("GetReviewById (v2): Review with id {ReviewId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Review is not found"
            });
        }

        var reviewResponse = this.mapper.Map<ReviewResponse>(review);
        this.logger.LogInformation("GetReviewById (v2) executed successfully for review id {ReviewId}", id);
        return this.Ok(new ApiResponse<ReviewResponse>
        {
            StatusCode = 200,
            Message = "Review is received",
            Data = reviewResponse
        });
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<int>>> CreateReview(ApiRequest<ReviewRequest> request)
    {
        this.logger.LogInformation("CreateReview (v2) method called for MovieId {MovieId} and UserId {UserId}",
            request.Data.MovieId, request.Data.UserId);

        if (request.Data.Rating > 10 || request.Data.Rating < 0)
        {
            this.logger.LogWarning("CreateReview (v2): Invalid rating {Rating} provided", request.Data.Rating);
            return this.BadRequest(new ApiResponse
            {
                StatusCode = 400,
                Message = "Rating is out of 1-10 span"
            });
        }

        var existingReview = await this.dbContext.Reviews
            .FirstOrDefaultAsync(r =>
                r.UserId == request.Data.UserId && r.MovieId == request.Data.MovieId);

        if (existingReview != null)
        {
            this.logger.LogInformation("Existing review found for MovieId {MovieId} and UserId {UserId}",
                request.Data.MovieId, request.Data.UserId);

            existingReview.Rating = request.Data.Rating;
            existingReview.Comment = request.Data.Comment;
            await this.dbContext.SaveChangesAsync();

            await this.dbContext.Entry(existingReview).Reference(r => r.User).LoadAsync();
            await this.dbContext.Entry(existingReview).Reference(r => r.Movie).LoadAsync();

            this.logger.LogInformation("Review updated successfully (v2) with Id {ReviewId}", existingReview.Id);
            return this.Ok(new ApiResponse<int>
            {
                StatusCode = 200,
                Message = "Review is updated",
                Data = existingReview.Id
            });
        }

        this.logger.LogInformation("No existing review found. Creating new review for MovieId {MovieId} and UserId {UserId}",
            request.Data.MovieId, request.Data.UserId);
        var newReview = this.mapper.Map<Review>(request.Data);
        this.dbContext.Reviews.Add(newReview);
        await this.dbContext.SaveChangesAsync();

        await this.dbContext.Entry(newReview).Reference(r => r.User).LoadAsync();
        await this.dbContext.Entry(newReview).Reference(r => r.Movie).LoadAsync();

        this.logger.LogInformation("New review created successfully (v2) with Id {ReviewId}", newReview.Id);
        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200,
            Message = "Review is created",
            Data = newReview.Id
        });
    }

    [HttpPut("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ReviewResponse>>> UpdateReview(int id, ApiRequest<ReviewRequest> request)
    {
        if (request.Data.Rating > 10 || request.Data.Rating < 0)
        {
            this.logger.LogWarning("UpdateReview (v2): Invalid rating {Rating} provided", request.Data.Rating);
            return this.BadRequest(new ApiResponse
            {
                StatusCode = 400,
                Message = "Rating is out of 1-10 span"
            });
        }

        this.logger.LogInformation("UpdateReview (v2) method called for review id {ReviewId}", id);
        var review = await this.dbContext.Reviews.FindAsync(id);

        if (review is null)
        {
            this.logger.LogWarning("UpdateReview (v2): Review with id {ReviewId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Review is not found"
            });
        }

        this.mapper.Map(request.Data, review);
        await this.dbContext.SaveChangesAsync();

        var reviewResponse = this.mapper.Map<ReviewResponse>(review);
        this.logger.LogInformation("UpdateReview (v2) executed successfully for review id {ReviewId}", id);
        return this.Ok(new ApiResponse<ReviewResponse>
        {
            StatusCode = 200,
            Message = "Review is updated",
            Data = reviewResponse
        });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse>> DeleteReview(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteReview (v2) method called for review id {ReviewId}", id);
        var review = await this.dbContext.Reviews.FindAsync(id);

        if (review is null)
        {
            this.logger.LogWarning("DeleteReview (v2): Review with id {ReviewId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Review is not found"
            });
        }

        this.dbContext.Reviews.Remove(review);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("DeleteReview (v2) executed successfully for review id {ReviewId}", id);
        return this.Ok(new ApiResponse
        {
            StatusCode = 200,
            Message = "Review is deleted"
        });
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<int>>> CreateLike(ApiRequest<LikeRequest> request)
    {
        this.logger.LogInformation("CreateLike (v2) method called for ReviewId {ReviewId} and UserId {UserId}",
            request.Data.ReviewId, request.Data.UserId);

        var existingLike = await this.dbContext.Likes.FirstOrDefaultAsync(r =>
            r.UserId == request.Data.UserId && r.ReviewId == request.Data.ReviewId);

        if (existingLike != null)
        {
            this.logger.LogWarning("CreateLike (v2): Like already exists for ReviewId {ReviewId} and UserId {UserId}",
                request.Data.ReviewId, request.Data.UserId);
            return this.BadRequest(new ApiResponse
            {
                StatusCode = 400,
                Message = "Like is existing"
            });
        }

        var like = this.mapper.Map<Like>(request.Data);
        await this.dbContext.Likes.AddAsync(like);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("Like created successfully (v2) with Id {LikeId}", like.Id);
        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200,
            Message = "Like is created",
            Data = like.Id
        });
    }

    [HttpDelete]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse>> DeleteLike(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteLike (v2) method called for LikeId {LikeId}", id);
        var like = await this.dbContext.Likes.FindAsync(id);

        if (like is null)
        {
            this.logger.LogWarning("DeleteLike (v2): Like with id {LikeId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Like is not found"
            });
        }

        this.dbContext.Likes.Remove(like);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("DeleteLike (v2) executed successfully for LikeId {LikeId}", id);
        return this.Ok(new ApiResponse
        {
            StatusCode = 200,
            Message = "Like is deleted"
        });
    }
}