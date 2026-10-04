using EventManager.Application;
using EventManager.Application.Options;
using EventManager.Infrastructure;
using EventManager.Infrastructure.Persistence;
using EventManager.Presentation;
using EventManager.Presentation.Middlewares;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
}

builder.Services.AddOptions<UserJwtTokenSettings>()
    .Bind(builder.Configuration.GetSection("UserJwtToken"))
    .ValidateDataAnnotations()
    .Validate(s => s.Lifetime > TimeSpan.Zero, "UserJwtToken:Lifetime must be greater than zero seconds.")
    .ValidateOnStart();

var dbConnectionString = builder.Configuration.GetConnectionString("EventsDb") ??
                         throw new InvalidOperationException("Connection string 'EventsDb' not found.");

builder.Services
    .AddApplicationServices()
    .AddInfrastructure(dbConnectionString)
    .AddPresentation();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Events API v1");
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();