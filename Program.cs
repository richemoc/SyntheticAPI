using Microsoft.EntityFrameworkCore;
using SyntheticApi.Data;
using SyntheticApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("SyntheticDb"));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "SyntheticApi",
        Version = "v1",
        Description = "A synthetic .NET 8 Web API with Products and Orders"
    });
    c.EnableAnnotations();
});

var app = builder.Build();

// Seed in-memory database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SyntheticApi v1"));

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
