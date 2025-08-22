using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// In-Movies data stores.
var movies = new List<Movie>();
var nextId = 1;


// ---------- Seed few for testing... later

// Endpoints.

// Create (Post / Movies)
app.MapPost("/movies", (MovieCreateDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
    {
        return Results.BadRequest(new { error = "Title is required." });
    }

    if (dto.Year < 1888) // First record are films are late 1880s
    {
        return Results.BadRequest(new { error = "Year Seems invalid for a movie." });
    }

    var duplicate = movies.Any(m =>
    m.Title.Equals(dto.Title.Trim(), StringComparison.OrdinalIgnoreCase) &&
    m.Director.Equals(dto.Director.Trim(), StringComparison.OrdinalIgnoreCase));

    if (duplicate)
    {
        return Results.Conflict(new { error = "This movie by the same director already exists." });
    }

    var movie = new Movie(
        nextId++,
        dto.Title.Trim(),
        dto.Director.Trim(),
        dto.Year,
        dto.Genre.Trim()
    );

    movies.Add(movie);
    return Results.Created($"/movies/{movie.Id}", movie);
})
.WithSummary("Create a Movie")
.WithDescription("Validates input and prevents exact duplicate Title+Director.");

// Read all (GET /movies) with filters, sort, paging
// /movies?director=Nolan&genre=sci&year=2010&sort=title|year|director&skip=0&take=10
app.MapGet("/movies", (string? director, string? genre, int? year, string? sort, int skip = 0, int take = 10) =>
{
    IEnumerable<Movie> q = movies;

    if(!string.IsNullOrWhiteSpace(director))
    {
        q = q.Where(m => m.Director.Contains(director, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(genre))
    {
        q = q.Where(m => m.Genre.Contains(genre, StringComparison.OrdinalIgnoreCase));
    }

    if(year is not null)
    {
        q = q.Where(m => m.Year == year);
    }

    q = (sort?.ToLowerInvariant()) switch
    {
        "year" => q.OrderBy(m => m.Year).ThenBy(m => m.Title),
        "director" => q.OrderBy(m => m.Director).ThenBy(m => m.Title),
        _ => q.OrderBy(m => m.Title) // Default.
    };
 
    if(skip < 0) { skip = 0; }  
    if(take <= 0 || take > 100) { take = 10; }

    var total = q.Count();
    var items = q.Skip(skip).Take(take).ToList();

    return Results.Ok(new {total, skip, take, items});

})
.WithSummary("Query Movies")
.WithDescription("Optional filter (director, genre, year), sorting, and paging.");

app.MapGet("/movies/{id:int}", (int id) =>
{
    var movie = movies.FirstOrDefault(m => m.Id == id);
    return movie is not null ? Results.Ok(movie) : Results.NotFound();
})
.WithSummary("Get by Id.");

// Update (PUT /movies/{id})
app.MapPut("/movies/{id:int}", (int id, MovieUpdateDto dto) =>
{
    var m = movies.FirstOrDefault(x => x.Id == id);
    if(m is null)
    {
        return Results.NotFound();
    }

    if(string.IsNullOrWhiteSpace(dto.Title))
    {
        return Results.BadRequest(new {error = "Title is required."});
    }

    m.Title = dto.Title.Trim();
    m.Director = dto.Director.Trim();
    m.Year = dto.Year;
    m.Genre = dto.Genre.Trim();

    return Results.Ok(m);
})
.WithSummary("Update a movie.");

// Delete (DELETE /movies/{id})
app.MapDelete("/movies/{id:int}", (int id) =>
{
    var m = movies.FirstOrDefault(x => x.Id == id);
    if(m is null)
    {
        return Results.NotFound();
    }

    movies.Remove(m);
    return Results.NoContent();

})
.WithSummary("Delete a movie.");

// Switch expression route: status by era
// GET /movies/era/{kind} where kind = "classic" (<=1995) or "modern" (>1995)
app.MapGet("/movies/era/{kind}", (string kind) =>
{
    IEnumerable<Movie> result = kind.ToLowerInvariant() switch
    {
        "classic" => movies.Where(m => m.Year <= 1995),
        "modern" => movies.Where(m => m.Year > 1995),
        _ => Enumerable.Empty<Movie>()
    };

})
.WithSummary("Filter by era")
.WithDescription("Use 'Classic' or 'Modern'. ");

app.Run();


// --------- Entity (class) 
public class Movie
{
    public Movie(int id, string title, string director, int year, string genre)
    {
        Id = id;
        Title = title;
        Director = director;
        Year = year;
        Genre = genre;
    }
    public int Id { get; init; }
    public string Title { get; set; } = default!;
    public string Director { get; set; } = default!;
    public int Year { get; set; }
    public string Genre { get; set; } = default!;
}

// -------(DTO) Records----------
public record MovieCreateDto(string Title, string Director, int Year, string Genre);
public record MovieUpdateDto(string Title, string Director, int Year, string Genre);

