using Microsoft.EntityFrameworkCore;

// create web application
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// create http endpoint "/" that returns "Hello World!"
app.MapGet("/", () => "Hello World!");

app.Run();