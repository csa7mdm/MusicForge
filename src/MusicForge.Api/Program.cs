using MusicForge.Api;
using MusicForge.Api.Endpoints;
using MusicForge.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Clean Architecture Layers
builder.Services.AddMusicForgeServices(builder.Configuration);
builder.Services.AddSignalR();

// OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "MusicForge API", Version = "v1" });
});

// CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MusicForge API v1"));
}

app.UseCors();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MusicForge.Infrastructure.Persistence.MusicForgeDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Map endpoints
app.MapHealthEndpoints();
app.MapProjectEndpoints();

// SignalR hub
app.MapHub<GenerationHub>("/hubs/generation");

// Root redirect to Swagger
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
