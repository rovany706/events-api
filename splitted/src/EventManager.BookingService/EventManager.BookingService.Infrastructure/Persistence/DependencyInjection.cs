using EventManager.BookingService.Application.Abstractions.Persistence.Repositories;
using EventManager.BookingService.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.BookingService.Infrastructure.Persistence;

internal static class DependencyInjection
{
    internal static IServiceCollection AddPersistence(this IServiceCollection services, string dbConnectionString)
    {
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddDbContext<BookingDbContext>(options => options.UseNpgsql(dbConnectionString));

        return services;
    }
}