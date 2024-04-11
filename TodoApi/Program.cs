using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Version = "v1" });
});

var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.36-mysql")), ServiceLifetime.Singleton);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");

app.MapGet("/tasks", async (ToDoDbContext dbContext, HttpContext context) =>
{
    var tasks = await dbContext.Items.ToListAsync();
    await context.Response.WriteAsJsonAsync(tasks);
});

app.MapPost("/tasks", async(ToDoDbContext context, Item item)=>{
    item.IsComplete=false;
    context.Add(item);
    await context.SaveChangesAsync();
    return item;
});
// app.MapPost("/tasks", async (ToDoDbContext dbContext, HttpRequest request) =>
// {
//     var taskData = await request.ReadFromJsonAsync<Item>(); // לקרוא את נתוני המשימה החדשה מהבקשה
//     var newTask = new Item { Name = taskData?.Name }; // ליצור רשומת משימה חדשה
//     dbContext.Items.Add(newTask); // להוסיף את המשימה החדשה למסד הנתונים
//     await dbContext.SaveChangesAsync(); // לשמור את השינויים במסד הנתונים
//     return Results.Created($"/tasks/{newTask.Id}", newTask); // להחזיר תשובת הצלחה עם מידע על המשימה החדשה
// });

// app.MapPut("/tasks/{id}", async(ToDoDbContext context, [FromBody]Item item, int id)=>{
//     var existItem = await context.Items.FindAsync(id);
//     if(existItem is null) return Results.NotFound();

//     existItem.Name = item.Name;
//     existItem.IsComplete = item.IsComplete;

//     await context.SaveChangesAsync();
//     return Results.Ok();
// });
// app.MapPut("/tasks/{id}", async(ToDoDbContext dbContext, HttpContext context, int id, Item updatedItem)=>
// {
//     if (updatedItem == null)
//     {
//         context.Response.StatusCode = StatusCodes.Status400BadRequest;
//         await context.Response.WriteAsync("Invalid task data");
//         return;
//     }

//     var existingItem = await dbContext.Items.FindAsync(id);
//     if (existingItem == null)
//     {
//         context.Response.StatusCode = StatusCodes.Status404NotFound;
//         await context.Response.WriteAsync($"Task with ID {id} not found");
//         return;
//     }

//     if (updatedItem.Name != null)
//     {
//         existingItem.Name = updatedItem.Name;
//     }

//     existingItem.IsComplete = updatedItem.IsComplete;

//     await dbContext.SaveChangesAsync();
//     context.Response.StatusCode = StatusCodes.Status200OK;
//     await context.Response.WriteAsJsonAsync(existingItem);
// });
app.MapPut("/items/{id}", async(ToDoDbContext context, [FromBody]Item item, int id)=>{
    var existItem = await context.Items.FindAsync(id);
    if(existItem is null) return Results.NotFound();

    existItem.Name = item.Name;
    existItem.IsComplete = item.IsComplete;

    await context.SaveChangesAsync();
    return Results.Ok();
});
app.MapDelete("/tasks/{id}", async (ToDoDbContext dbContext, HttpContext context, int id) =>
{
    var existingItem = await dbContext.Items.FindAsync(id);
    if (existingItem == null)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    dbContext.Items.Remove(existingItem);
    await dbContext.SaveChangesAsync();
    context.Response.StatusCode = StatusCodes.Status200OK;
});
app.Run();


