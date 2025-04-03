using CineVault.API.Controllers.Requests;
using CineVault.API.Controllers.Responses;
using CineVault.API.Entities;
using Mapster;

namespace CineVault.API.Extensions;

public class EntitiesProfiles : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MovieRequest, Movie>();

        config.NewConfig<ReviewRequest, Review>();

        config.NewConfig<UserRequest, User>();

        config.NewConfig<LikeRequest, Like>();

        config.NewConfig<ActorRequest, Actor>();

        config.NewConfig<Movie, MovieResponse>()
            .Map(m => m.AverageRating,
                m => m.Reviews.Count != 0 ? m.Reviews.Average(r => r.Rating) : 0)
            .Map(m => m.ReviewCount,
                m => m.Reviews.Count);

        config.NewConfig<Review, ReviewResponse>()
            .Map(r => r.MovieTitle,
                r => r.Movie!.Title)
            .Map(r => r.Username,
                r => r.User!.Username);

        config.NewConfig<User, UserResponse>();

        config.NewConfig<Actor, ActorResponse>();
    }
}
