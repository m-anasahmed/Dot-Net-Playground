using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Open Api // Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// In-Memory Store ( For demo only)
var books = new List<Book>();
var nextId = 1;

// GET /books ? list (optionally filter by author and/or year via query string)
app.MapGet("/books", (string? title, string? author, int? year) =>
{
    IEnumerable<Book> query = books;

    // If 'Title' was provided (?Title=...), filter case-insensitively and ignore leading/trailing spaces
    if (!string.IsNullOrWhiteSpace(title))
    {
        var titleFilter = title.Trim();
        query = query.Where(b => b.Title.Contains(titleFilter, StringComparison.OrdinalIgnoreCase));
    }

    // If 'author' was provided (?author=...), filter case-insensitively and ignore leading/trailing spaces
    if (!string.IsNullOrWhiteSpace(author))
    {
        var authorFilter = author.Trim();
        query = query.Where(b => b.Author.Contains(authorFilter, StringComparison.OrdinalIgnoreCase));
    }

    // If 'year' was provided (?year=...), filter exact year
    if (year is not null)
    {
        query = query.Where(b => b.Year == year.Value);
    }

    return Results.Ok(query);
})
.WithName("GetBooks")
.WithSummary("List books (with optional filters)")
.WithDescription("Use query params ?author=... and/or ?year=... to filter. Author match is case-insensitive.");


// Get /books/{id} => single
app.MapGet("/books/{id:int}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    return book is not null ? Results.Ok(book) : Results.NotFound();
})
.WithName("GetBookById");

//Post /books => create (400 if Title empty, 400 if Year < 0, 409 if duplicate title)
app.MapPost("/books", (BookCreateDto dto) =>
{
    // 01 Basic Validation.
    if(string.IsNullOrWhiteSpace(dto.Title)){
        return Results.BadRequest(new { error = "Title can not be empty." });
    }
    // 02 Non-Negative Validation.
    if(dto.Year < 0){
        return Results.BadRequest(new {error = "Year must be non-negative."});
    }

    // Normalize inout title once.
    var normalizedTitle = dto.Title.Trim();

    // 3) Duplicate check (case-insensitive)
    var isDuplicate = books.Any(b => 
        string.Equals(b.Title, normalizedTitle, StringComparison.OrdinalIgnoreCase));

    // Validation Condition
    if (isDuplicate){
        return Results.Conflict(new { error = "A book with this title already exist." });
    }

    // 4) Create
    var book = new Book(nextId++, normalizedTitle, dto.Author, dto.Year);
    books.Add(book);

    return Results.Created($"/books/{book.Id}", book);
})
.WithName("CreateBook")
.WithSummary("Create a new book")
.WithDescription("Add a book. Return 400 when Title is empty or Whitespace.");

// PUT /books/{id} => Update
app.MapPut("/books/{id:int}", (int id, BookUpdateDto dto) =>
{
    var index = books.FindIndex(b => b.Id == id);
    if (index == -1) return Results.NotFound();

    // If Title is provided, it cannot be empty
    if (dto.Title is not null && string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { error = "Title can not be empty if provided." });

    var current = books[index];
    var updated = current with
    {
        Title = dto.Title ?? current.Title,
        Author = dto.Author ?? current.Author,
        Year = dto.Year ?? current.Year
    };

    books[index] = updated;
    return Results.Ok(updated);
})
.WithName("UpdateBook");

// Delete /books/{id} => delete
app.MapDelete("/books/{id:int}", (int id) =>
{
    var removed = books.RemoveAll(b => b.Id == id) > 0;
    return removed ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteBook");

app.Run();

// *************** DTO ***************
// Record (DTOs + Model)
record Book(int Id, string Title, string Author, int Year);
record BookCreateDto(string Title, string Author, int Year);
record BookUpdateDto(string? Title, string? Author, int? Year);