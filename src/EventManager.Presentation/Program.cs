using System.Text;

using EventManager.Application;
using EventManager.Application.Options;
using EventManager.Infrastructure;
using EventManager.Infrastructure.Persistence;
using EventManager.Presentation;
using EventManager.Presentation.Middlewares;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });
}

var userJwtTokenSection = builder.Configuration.GetSection("UserJwtToken") ??
                          throw new InvalidOperationException("UserJwtToken section not found");

builder.Services.AddOptions<UserJwtTokenSettings>()
    .Bind(userJwtTokenSection)
    .ValidateDataAnnotations()
    .Validate(s => s.Lifetime > TimeSpan.Zero, "UserJwtToken:Lifetime must be greater than zero seconds.")
    .ValidateOnStart();

var dbConnectionString = builder.Configuration.GetConnectionString("EventsDb") ??
                         throw new InvalidOperationException("Connection string 'EventsDb' not found.");

builder.Services
    .AddApplicationServices()
    .AddInfrastructure(dbConnectionString)
    .AddPresentation();

var userJwtTokenSettings = userJwtTokenSection.Get<UserJwtTokenSettings>()!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RoleClaimType = "role",
        ValidateIssuer = true,
        ValidIssuer = userJwtTokenSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = userJwtTokenSettings.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(userJwtTokenSettings.Secret))
    };
});

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();