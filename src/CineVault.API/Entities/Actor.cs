namespace CineVault.API.Entities;

public sealed class Actor
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Biography { get; set; }
    public ICollection<Movie> Movies { get; set; } = [];
    public bool IsDeleted { get; set; }
}