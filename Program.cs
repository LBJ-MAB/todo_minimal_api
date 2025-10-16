using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// dependency injection 
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});

var app = builder.Build();

// middleware
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TodoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

// implementing MapGroup for simplification
var todoItems = app.MapGroup("/todoitems");

// using methods rather than lambdas - methods can be unit tested
todoItems.MapGet("/", GetAllTodos);
todoItems.MapGet("/complete", GetCompleteTodos);
todoItems.MapGet("/{id}", GetTodo);
todoItems.MapPost("/", CreateTodo);
todoItems.MapPut("/{id}", UpdateTodo);
todoItems.MapDelete("/{id}", DeleteTodo);

// using TypedResults API
// get all todos from Todos table
static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToListAsync());
}

// get all todos that are complete
static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
}

// get a certain to do item by its id
static async Task<IResult> GetTodo(TodoDb db, int id)
{
    var todo = await db.Todos.FindAsync(id);
    if (todo == null)
    {
        return TypedResults.NotFound();
    }

    return TypedResults.Ok(new TodoItemDTO(todo));
}

// post request to add a new to-do
static async Task<IResult> CreateTodo(TodoDb db, TodoItemDTO todoItemDTO)
{
    var todoItem = new Todo
    {
        IsComplete = todoItemDTO.IsComplete,
        Name = todoItemDTO.Name
    };
    
    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    todoItemDTO = new TodoItemDTO(todoItem);

    return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
}

// put method to alter a to-do item
static async Task<IResult> UpdateTodo(TodoDb db, TodoItemDTO todoItemDTO, int id)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null)
    {
        return TypedResults.NotFound();
    }

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

// delete method to delete a to-do item
static async Task<IResult> DeleteTodo(TodoDb db, int id)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null)
    {
        return TypedResults.NotFound();
    }

    db.Todos.Remove(todo);

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

app.Run();