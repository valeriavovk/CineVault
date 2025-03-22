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
public class UsersController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<UsersController> _logger;

    public UsersController(CineVaultDbContext dbContext, ILogger<UsersController> logger)
    {
        this.dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    [MapToApiVersion("1")]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        _logger.LogInformation("GetUsers method called");
        var users = await dbContext.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();
        _logger.LogInformation("GetUsers executed successfully. Returned {UserCount} users", users.Count);
        return Ok(users);
    }

    [HttpGet("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        _logger.LogInformation("GetUserById method called with id {UserId}", id);
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found", id);
            return NotFound();
        }
        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
        _logger.LogInformation("GetUserById executed successfully for user id {UserId}", id);
        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("1")]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        _logger.LogInformation("CreateUser method called with Username {Username} and Email {Email}", request.Username, request.Email);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("User created successfully with Id {UserId}", user.Id);
        return Ok();
    }

    [HttpPut("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        _logger.LogInformation("UpdateUser method called for user id {UserId}", id);
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found for update", id);
            return NotFound();
        }
        user.Username = request.Username;
        user.Email = request.Email;
        user.Password = request.Password;
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("User updated successfully with Id {UserId}", user.Id);
        return Ok();
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("1")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        _logger.LogInformation("DeleteUser method called for user id {UserId}", id);
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found for deletion", id);
            return NotFound();
        }
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
        _logger.LogInformation("User deleted successfully with Id {UserId}", user.Id);
        return Ok();
    }

    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetUsers(ApiRequest request)
    {
        var users = await dbContext.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();
        var response = new ApiResponse<List<UserResponse>>
        {
            StatusCode = 200,
            Message = "OK",
            Data = users
        };
        return Ok(response);
    }

    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(int id, ApiRequest request)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new ApiResponse<UserResponse>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = default!
            });
        }
        var userResponse = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
        var response = new ApiResponse<UserResponse>
        {
            StatusCode = 200,
            Message = "OK",
            Data = userResponse
        };
        return Ok(response);
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser(ApiRequest<UserRequest> request)
    {
        var userRequest = request.Data;
        var user = new User
        {
            Username = userRequest.Username,
            Email = userRequest.Email,
            Password = userRequest.Password
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        var userResponse = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
        var response = new ApiResponse<UserResponse>
        {
            StatusCode = 201,
            Message = "Created",
            Data = userResponse
        };
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, response);
    }

    [HttpPut("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(int id, ApiRequest<UserRequest> request)
    {
        var userRequest = request.Data;
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new ApiResponse<UserResponse>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = default!
            });
        }
        user.Username = userRequest.Username;
        user.Email = userRequest.Email;
        user.Password = userRequest.Password;
        await dbContext.SaveChangesAsync();
        var userResponse = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
        var response = new ApiResponse<UserResponse>
        {
            StatusCode = 200,
            Message = "OK",
            Data = userResponse
        };
        return Ok(response);
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUser(int id, ApiRequest request)
    {
        var user = await dbContext.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = "Not Found",
                Data = "User not found"
            });
        }
        dbContext.Users.Remove(user);
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
