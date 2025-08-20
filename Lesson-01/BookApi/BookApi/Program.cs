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

// Get /books => List
app.MapGet("/books", () =>
{
    return Results.Ok(books);
})
.WithName("Get Books")
.WithSummary("List all Books")
.WithDescription("Return every book currently stored in memory,");

// Get /books/{id} => single
app.MapGet("/books/{id:int}", (int id) =>
{
    var book = books.FirstOrDefault(b => b.Id == id);
    return book is not null ? Results.Ok(book) : Results.NotFound();
})
.WithName("GetBookById");

//Post /books => create (400 if Title empty)
app.MapPost("/books", (BookCreateDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
    {
        return Results.BadRequest(new { error = "Title can not be empty." });
    }

    var book = new Book(nextId++, dto.Title.Trim(), dto.Author, dto.Year);
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
        return Results.BadRequest(new { error = "Title can not be empty id provided." });

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