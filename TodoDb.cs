using Microsoft.EntityFrameworkCore;

// DbContext acts as a bridge between your application and a database
class TodoDb : DbContext
{
    // constructor - allows EF to set up the connection and behaviour
    public TodoDb (DbContextOptions<TodoDb> options) 
        : base (options) { }
    
    // Todos represents a table of To do items in the database which we can perform CRUD ops on
    public DbSet<Todo> Todos => Set<Todo>();
}
