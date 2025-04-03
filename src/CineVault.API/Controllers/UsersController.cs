using System.Linq.Expressions;
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
public class UsersController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<UsersController> logger;
    private readonly IMapper mapper;

    public UsersController(CineVaultDbContext dbContext, ILogger<UsersController> logger,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1")]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        this.logger.LogInformation("GetUsers method called");
        var users = await this.dbContext.Users
            .Select(u => new UserResponse { Id = u.Id, Username = u.Username, Email = u.Email })
            .ToListAsync();
        this.logger.LogInformation("GetUsers executed successfully. Returned {UserCount} users",
            users.Count);
        return this.Ok(users);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        this.logger.LogInformation("GetUserById method called with id {UserId}", id);
        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this.logger.LogError("User with id {UserId} not found", id);
            return this.NotFound();
        }

        var response = new UserResponse
        {
            Id = user.Id, Username = user.Username, Email = user.Email
        };
        this.logger.LogInformation("GetUserById executed successfully for user id {UserId}", id);
        return this.Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("1")]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        this.logger.LogInformation(
            "CreateUser method called with Username {Username} and Email {Email}", request.Username,
            request.Email);
        var user = new User
        {
            Username = request.Username, Email = request.Email, Password = request.Password
        };
        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("User created successfully with Id {UserId}", user.Id);
        return this.Ok();
    }

    [HttpPut("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        this.logger.LogInformation("UpdateUser method called for user id {UserId}", id);
        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this.logger.LogError("User with id {UserId} not found for update", id);
            return this.NotFound();
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.Password = request.Password;
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("User updated successfully with Id {UserId}", user.Id);
        return this.Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        this.logger.LogInformation("DeleteUser method called for user id {UserId}", id);
        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this.logger.LogError("User with id {UserId} not found for deletion", id);
            return this.NotFound();
        }

        this.dbContext.Users.Remove(user);
        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation("User deleted successfully with Id {UserId}", user.Id);
        return this.Ok();
    }

    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ICollection<UserResponse>>>> GetUsers(
        ApiRequest request)
    {
        this.logger.LogInformation("GetUsers (v2) method called");

        var users = await this.dbContext.Users.ToListAsync();
        this.logger.LogInformation("Fetched {UserCount} users from database", users.Count);

        var usersResponses = this.mapper.Map<ICollection<UserResponse>>(users);
        this.logger.LogInformation("Mapping of users to UserResponse completed successfully");

        return this.Ok(new ApiResponse<ICollection<UserResponse>>
        {
            StatusCode = 200, Message = "Users are received", Data = usersResponses
        });
    }

    // 12 завдання
    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<UserStatsResponse>>> GetUserStats(int id,
        ApiRequest request)
    {
        this.logger.LogInformation("GetUserStats (v2) called for UserId: {UserId}", id);

        var statsQuery = await this.dbContext.Reviews
            .Where(r => r.UserId == id)
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                TotalReviews = g.Count(),
                AverageRating = g.Average(r => (double?)r.Rating) ?? 0,
                GenreStats = g.GroupBy(r => r.Movie!.Genre ?? "Unknown")
                    .Select(gr => new { Genre = gr.Key, Count = gr.Count() }),
                LastActivity = g.Max(r => (DateTime?)r.CreatedAt)
            })
            .FirstOrDefaultAsync();

        if (statsQuery == null)
        {
            this.logger.LogWarning("User with Id {UserId} not found or has no reviews", id);
            return this.NotFound(
                new ApiResponse { StatusCode = 404, Message = "User is not found" });
        }

        var stats = new UserStatsResponse
        {
            TotalReviews = statsQuery.TotalReviews,
            AverageRating = statsQuery.AverageRating,
            GenreStats = statsQuery.GenreStats.ToDictionary(g => g.Genre, g => g.Count),
            LastActivity = statsQuery.LastActivity
        };

        this.logger.LogInformation("Successfully retrieved stats for UserId: {UserId}", id);
        return this.Ok(new ApiResponse<UserStatsResponse>
        {
            StatusCode = 200, Message = "User stats are received", Data = stats
        });
    }

    // завдання 13.5
    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ICollection<UserResponse>>>> SearchUsers(
        ApiRequest<SearchUsersRequest> request)
    {
        this.logger.LogInformation("SearchUsers (v2) method called with criteria: {@Criteria}", request.Data);

        var criteria = request.Data;
        var query = this.dbContext.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            string searchTermLower = criteria.SearchTerm.ToLower();
            query = query.Where(u => u.Username.ToLower().Contains(searchTermLower) || u.Email.ToLower().Contains(searchTermLower));
            this.logger.LogInformation("Filtering users by SearchTerm: {SearchTerm}", criteria.SearchTerm);
        }

        if (criteria.CreatedAfter.HasValue)
        {
            query = query.Where(u => u.CreatedAt >= criteria.CreatedAfter.Value);
            this.logger.LogInformation("Filtering users with CreatedAt >= {CreatedAfter}", criteria.CreatedAfter.Value);
        }

        if (criteria.CreatedBefore.HasValue)
        {
            query = query.Where(u => u.CreatedAt <= criteria.CreatedBefore.Value);
            this.logger.LogInformation("Filtering users with CreatedAt <= {CreatedBefore}", criteria.CreatedBefore.Value);
        }

        Expression<Func<User, object>> keySelector = criteria.SortBy?.ToLowerInvariant() switch
        {
            "username" => u => u.Username,
            "email" => u => u.Email,
            _ => u => u.Id
        };

        bool descending = criteria.SortOrder?.ToLowerInvariant() == "desc";
        query = descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        this.logger.LogInformation("Applying sorting: SortBy = {SortBy}, Order = {SortOrder}", criteria.SortBy, descending ? "desc" : "asc");

        int pageNumber = criteria.PageNumber ?? 1;
        int pageSize = criteria.PageSize ?? 10;
        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        this.logger.LogInformation("Applying pagination: PageNumber = {PageNumber}, PageSize = {PageSize}", pageNumber, pageSize);

        var users = await query.ToListAsync();
        this.logger.LogInformation("SearchUsers (v2) retrieved {UserCount} users after filtering", users.Count);

        var userResponses = this.mapper.Map<List<UserResponse>>(users);
        this.logger.LogInformation("Mapping of filtered users to UserResponse completed successfully");

        return this.Ok(new ApiResponse<ICollection<UserResponse>>
        {
            StatusCode = 200, Message = "Users are received", Data = userResponses
        });
    }

    private static readonly Func<CineVaultDbContext, int, Task<User?>> GetUserByIdCompiledQuery = 
        EF.CompileAsyncQuery((CineVaultDbContext context, int id) => 
            context.Users.AsNoTracking().FirstOrDefault(u => u.Id == id));

    // завдання 13.3
    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int id, ApiRequest request)
    {
        this.logger.LogInformation("GetUserById (v2) method called with id {UserId}", id);

        var user = await GetUserByIdCompiledQuery(this.dbContext, id);
    
        if (user is null) 
        {
            return this.NotFound(new ApiResponse { StatusCode = 404, Message = "User is not found" });
        }

        return this.Ok(new ApiResponse<UserResponse> 
            { StatusCode = 200, Message = "User is received", Data = this.mapper.Map<UserResponse>(user) });
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<int>>> CreateUser(ApiRequest<UserRequest> request)
    {
        this.logger.LogInformation(
            "CreateUser (v2) method called with Username: {Username}, Email: {Email}",
            request.Data.Username, request.Data.Email);

        var user = this.mapper.Map<User>(request.Data);
        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("User created successfully (v2) with Id {UserId}", user.Id);
        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200, Message = "User is created", Data = user.Id
        });
    }

    [HttpPut("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(int id,
        ApiRequest<UserRequest> request)
    {
        this.logger.LogInformation("UpdateUser (v2) method called for user id {UserId}", id);

        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this.logger.LogWarning("UpdateUser (v2): User with id {UserId} not found", id);
            return this.NotFound(
                new ApiResponse { StatusCode = 404, Message = "User is not found" });
        }

        this.mapper.Map(request.Data, user);
        await this.dbContext.SaveChangesAsync();

        var userResponse = this.mapper.Map<UserResponse>(user);
        this.logger.LogInformation("UpdateUser (v2) executed successfully for user id {UserId}",
            id);
        return this.Ok(new ApiResponse<UserResponse>
        {
            StatusCode = 200, Message = "User is updated", Data = userResponse
        });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse>> DeleteUser(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteUser (v2) method called for user id {UserId}", id);

        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            this.logger.LogWarning("DeleteUser (v2): User with id {UserId} not found", id);
            return this.NotFound(
                new ApiResponse { StatusCode = 404, Message = "User is not found" });
        }

        user.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("DeleteUser (v2) executed successfully for user id {UserId}",
            id);
        return this.Ok(new ApiResponse { StatusCode = 200, Message = "User is deleted" });
    }

    [HttpPost("{id}/restore")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse>> RestoreUser(int id, ApiRequest request)
    {
        this.logger.LogInformation("RestoreUser (v2) method called for user id {UserId}", id);

        var user = await this.dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            this.logger.LogWarning("RestoreUser (v2): User with id {UserId} not found", id);
            return this.NotFound(
                new ApiResponse { StatusCode = 404, Message = "User is not found" });
        }

        user.IsDeleted = false;
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation(
            "User (v2) restored successfully. UserId: {UserId}, Username: {Username}",
            user.Id, user.Username);
        return this.Ok(new ApiResponse { StatusCode = 200, Message = "User is restored" });
    }
}