using Asp.Versioning;
using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineVault.API.Controllers;

[Route("api/v{v:apiVersion}/[controller]/[action]")]
[ApiVersion("2")]
public class ActorsController : ControllerBase
{
    private readonly CineVaultDbContext dbContext;
    private readonly ILogger<ActorsController> logger;
    private readonly IMapper mapper;

    public ActorsController(CineVaultDbContext dbContext, ILogger<ActorsController> logger,
        IMapper mapper)
    {
        this.dbContext = dbContext;
        this.logger = logger;
        this.mapper = mapper;
    }

    // завдання 13.1
    [HttpOptions]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ICollection<ActorResponse>>>> GetActors(ApiRequest request)
    {
        this.logger.LogInformation("GetActors (v2) method called");

        var actors = await this.dbContext.Actors
            .AsNoTracking()
            .ToListAsync();

        this.logger.LogInformation("Fetched {ActorCount} actors from database", actors.Count);

        var actorsResponses = this.mapper.Map<ICollection<ActorResponse>>(actors);
        this.logger.LogInformation("Mapping of actors to ActorResponse completed successfully");

        return this.Ok(new ApiResponse<ICollection<ActorResponse>>
        {
            StatusCode = 200,
            Message = "Actors are received",
            Data = actorsResponses
        });
    }

    [HttpOptions("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ActorResponse>>> GetActorById(int id, ApiRequest request)
    {
        this.logger.LogInformation("GetActorById (v2) method called with id {ActorId}", id);

        var actor = await this.dbContext.Actors.FindAsync(id);
        if (actor is null)
        {
            this.logger.LogWarning("GetActorById (v2): Actor with id {ActorId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Actor is not found"
            });
        }

        var actorResponse = this.mapper.Map<ActorResponse>(actor);
        this.logger.LogInformation("GetActorById (v2) executed successfully for actor id {ActorId}", id);

        return this.Ok(new ApiResponse<ActorResponse>
        {
            StatusCode = 200,
            Message = "Actor is received",
            Data = actorResponse
        });
    }

    [HttpPost]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<int>>> CreateActor(ApiRequest<ActorRequest> request)
    {
        this.logger.LogInformation("CreateActor (v2) method called with FullName: {FullName}, BirthDate: {BirthDate}",
            request.Data.FullName, request.Data.BirthDate);

        var actor = this.mapper.Map<Actor>(request.Data);
        this.dbContext.Actors.Add(actor);
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("Actor created successfully (v2) with Id {ActorId}", actor.Id);
        return this.Ok(new ApiResponse<int>
        {
            StatusCode = 200,
            Message = "Actor is created",
            Data = actor.Id
        });
    }

    [HttpPut("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse<ActorResponse>>> UpdateActor(int id, ApiRequest<ActorRequest> request)
    {
        this.logger.LogInformation("UpdateActor (v2) method called for actor id {ActorId}", id);

        var actor = await this.dbContext.Actors.FindAsync(id);
        if (actor is null)
        {
            this.logger.LogWarning("UpdateActor (v2): Actor with id {ActorId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Actor is not found"
            });
        }

        this.mapper.Map(request.Data, actor);
        await this.dbContext.SaveChangesAsync();

        var actorResponse = this.mapper.Map<ActorResponse>(actor);
        this.logger.LogInformation("UpdateActor (v2) executed successfully for actor id {ActorId}", id);
        return this.Ok(new ApiResponse<ActorResponse>
        {
            StatusCode = 200,
            Message = "Actor is updated",
            Data = actorResponse
        });
    }

    [HttpDelete("{id}")]
    [MapToApiVersion("2")]
    public async Task<ActionResult<ApiResponse>> DeleteActor(int id, ApiRequest request)
    {
        this.logger.LogInformation("DeleteActor (v2) method called for actor id {ActorId}", id);

        var actor = await this.dbContext.Actors.FindAsync(id);
        if (actor is null)
        {
            this.logger.LogWarning("DeleteActor (v2): Actor with id {ActorId} not found", id);
            return this.NotFound(new ApiResponse
            {
                StatusCode = 404,
                Message = "Actor is not found"
            });
        }

        actor.IsDeleted = true;
        await this.dbContext.SaveChangesAsync();

        this.logger.LogInformation("DeleteActor (v2) executed successfully for actor id {ActorId}", id);
        return this.Ok(new ApiResponse
        {
            StatusCode = 200,
            Message = "Actor is deleted"
        });
    }
}