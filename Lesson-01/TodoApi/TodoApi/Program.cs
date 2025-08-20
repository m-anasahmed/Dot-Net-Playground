using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ----- In-memory "database" -----
var todos = new List<Todo>();
var nextId = 1;

// GET /todos
app.MapGet("/todos", () => Results.Ok(todos))
   .WithName("GetTodos")
   .WithSummary("List todos")
   .WithDescription("Returns all todos in memory.");

// GET /todos/{id}
app.MapGet("/todos/{id:int}", (int id) =>
{
    var t = todos.FirstOrDefault(x => x.Id == id);
    return t is not null ? Results.Ok(t) : Results.NotFound();
})
.WithName("GetTodoById");

// POST /todos
app.MapPost("/todos", (CreateTodoDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { error = "Title is required." });

    var todo = new Todo(nextId++, dto.Title, false);
    todos.Add(todo);
    return Results.Created($"/todos/{todo.Id}", todo);
})
.WithName("CreateTodo");

// PUT /todos/{id}
app.MapPut("/todos/{id:int}", (int id, UpdateTodoDto dto) =>
{
    var t = todos.FirstOrDefault(x => x.Id == id);
    if (t is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { error = "Title is required." });

    t.Title = dto.Title;
    t.IsDone = dto.IsDone;
    return Results.Ok(t);
})
.WithName("UpdateTodo");

// DELETE /todos/{id}
app.MapDelete("/todos/{id:int}", (int id) =>
{
    var t = todos.FirstOrDefault(x => x.Id == id);
    if (t is null) return Results.NotFound();
    todos.Remove(t);
    return Results.NoContent();
})
.WithName("DeleteTodo");

app.Run();

// --------- Models / DTOs ---------
public record CreateTodoDto(string Title);
public record UpdateTodoDto(string Title, bool IsDone);

public class Todo
{
    public Todo(int id, string title, bool isDone)
    {
        Id = id; Title = title; IsDone = isDone;
    }
    public int Id { get; init; }
    public string Title { get; set; }
    public bool IsDone { get; set; }
}
