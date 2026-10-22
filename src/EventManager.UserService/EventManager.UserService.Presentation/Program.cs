using System.Text;

using EventManager.Presentation;
using EventManager.UserService.Application;
using EventManager.UserService.Application.Options;
using EventManager.UserService.Infrastructure;
using EventManager.UserService.Infrastructure.Persistence;
using EventManager.UserService.Presentation.Middlewares;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Serilog;
using Serilog.Formatting.Compact;

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

var dbConnectionString = builder.Configuration.GetConnectionString("UsersDb") ??
                         throw new InvalidOperationException("Connection string 'UsersDb' not found.");

var otlpEndpoint = builder.Configuration.GetSection("Otlp").GetValue<string>("Endpoint") ??
                   throw new InvalidOperationException("'Otlp/Endpoint' not found.");

builder.Services
    .AddApplicationServices()
    .AddInfrastructure(dbConnectionString)
    .AddPresentation()
    .AddObservability(otlpEndpoint);

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

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console(new CompactJsonFormatter());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Events API v1");
    });
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapPrometheusScrapingEndpoint();

app.Run();