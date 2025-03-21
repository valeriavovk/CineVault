using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/[controller]/[action]")]
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
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        _logger.LogInformation("GetUsers method called");
        var users = await this.dbContext.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();
        _logger.LogInformation("GetUsers executed successfully. Returned {UserCount} users", users.Count);
        return base.Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUserById(int id)
    {
        _logger.LogInformation("GetUserById method called with id {UserId}", id);
        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found", id);
            return base.NotFound();
        }
        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
        _logger.LogInformation("GetUserById executed successfully for user id {UserId}", id);
        return base.Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateUser(UserRequest request)
    {
        _logger.LogInformation("CreateUser method called with Username {Username} and Email {Email}", request.Username, request.Email);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password
        };
        this.dbContext.Users.Add(user);
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("User created successfully with Id {UserId}", user.Id);
        return base.Ok();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, UserRequest request)
    {
        _logger.LogInformation("UpdateUser method called for user id {UserId}", id);
        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found for update", id);
            return base.NotFound();
        }
        user.Username = request.Username;
        user.Email = request.Email;
        user.Password = request.Password;
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("User updated successfully with Id {UserId}", user.Id);
        return base.Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        _logger.LogInformation("DeleteUser method called for user id {UserId}", id);
        var user = await this.dbContext.Users.FindAsync(id);
        if (user is null)
        {
            _logger.LogError("User with id {UserId} not found for deletion", id);
            return base.NotFound();
        }
        this.dbContext.Users.Remove(user);
        await this.dbContext.SaveChangesAsync();
        _logger.LogInformation("User deleted successfully with Id {UserId}", user.Id);
        return base.Ok();
    }
}
