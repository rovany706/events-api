using EventManager.Presentation.Application;
using EventManager.Presentation.Domain;
using EventManager.Presentation.Domain.DataAccess;
using EventManager.Presentation.Middlewares;
using EventManager.Presentation.Presentation;
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

var dbConnectionString = builder.Configuration.GetConnectionString("EventsDb") ??
                         throw new InvalidOperationException("Connection string 'EventsDb' not found.");

builder.Services
    .AddApplication()
    .AddDomain(dbConnectionString)
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
