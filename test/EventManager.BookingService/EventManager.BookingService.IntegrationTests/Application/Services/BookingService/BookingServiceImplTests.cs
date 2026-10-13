using EventManager.BookingService.Application.Common.Results;
using EventManager.BookingService.Application.Services.BookingService;
using EventManager.BookingService.Domain.Entities;
using EventManager.BookingService.Domain.Exceptions;
using EventManager.BookingService.Infrastructure.Persistence;
using EventManager.BookingService.Infrastructure.Persistence.Repositories;
using EventManager.Contracts.Jwt;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Testcontainers.PostgreSql;

namespace EventManager.BookingService.IntegrationTests.Application.Services.BookingService;

public class BookingServiceImplTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("test_db")
        .Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    private BookingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new BookingDbContext(options);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
    }

    [Fact]
    public async Task CreateBookingAsync_Always_ShouldReturnNewBookingId()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        const int testEventId = 1;
        const int testUserId = 1;
        const int expectedBookingId = 1;
        await using var context = CreateContext();

        var bookingRepository = new BookingRepository(context);
        var bookingService = new BookingServiceImpl(bookingRepository, NullLogger<BookingServiceImpl>.Instance);

        // Act
        var bookingResult = await bookingService.CreateBookingAsync(testEventId, testUserId, ct);

        // Assert
        bookingResult.IsSuccess.Should().BeTrue();
        var actualBookingId = bookingResult.Value!.Id;
        actualBookingId.Should().Be(expectedBookingId);
    }

    [Fact]
    public async Task CreateBookingAsync_Always_ShouldReturnUniqueBookingId()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        const int expectedBookingId1 = 1;
        const int expectedBookingId2 = 2;
        const int testEventId = 1;
        const int testUserId = 1;
        await using var context = CreateContext();

        var bookingRepository = new BookingRepository(context);
        var bookingService = new BookingServiceImpl(bookingRepository, NullLogger<BookingServiceImpl>.Instance);

        // Act
        var bookingResult1 = await bookingService.CreateBookingAsync(testEventId, testUserId, ct);
        var bookingResult2 = await bookingService.CreateBookingAsync(testEventId, testUserId, ct);

        // Assert
        bookingResult1.IsSuccess.Should().BeTrue();
        bookingResult2.IsSuccess.Should().BeTrue();
        var actualBookingId1 = bookingResult1.Value!.Id;
        var actualBookingId2 = bookingResult2.Value!.Id;
        actualBookingId1.Should().Be(expectedBookingId1);
        actualBookingId2.Should().Be(expectedBookingId2);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenConcurrent_ShouldReturnUniqueIds()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        const int totalSeats = 10;
        const int testEventId = 1;
        const int testUserId = 1;
        await using var context = CreateContext();

        // Act
        var tasks = new Task<Result<Booking?>>[totalSeats];

        for (var i = 0; i < totalSeats; i++)
        {
            tasks[i] = Task.Run(
                async () =>
                {
                    await using var taskContext = CreateContext();
                    var taskBookingRepository = new BookingRepository(taskContext);
                    var bookingService = new BookingServiceImpl(taskBookingRepository, NullLogger<BookingServiceImpl>.Instance);

                    return await bookingService.CreateBookingAsync(testEventId, testUserId, ct);
                },
                ct);
        }

        var bookingResults = await Task.WhenAll(tasks);

        // Assert
        bookingResults.All(x => x.IsSuccess).Should().BeTrue();
        bookingResults.Select(x => x.Value!.Id).Should().BeEquivalentTo(Enumerable.Range(1, totalSeats));
    }

    [Fact]
    public async Task CancelBookingAsync_WhenBookingExists_ShouldSetPendingCancellation()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        const int testEventId = 1;
        const int testUserId = 1;
        await using var context = CreateContext();

        var testBooking = Booking.CreateInstance(testEventId, testUserId);
        testBooking.Confirm();;
        context.Bookings.Add(testBooking);
        await context.SaveChangesAsync(ct);
        
        // Act
        await using var actContext = CreateContext();
        var bookingRepository = new BookingRepository(actContext);
        var bookingService = new BookingServiceImpl(bookingRepository, NullLogger<BookingServiceImpl>.Instance);

        var result =
            await bookingService.CancelBookingAsync(testBooking.Id, testUserId, JwtRoles.UserRole,
                ct);

        // Assert
        await using var verifyContext = CreateContext();
        var verifyBooking = verifyContext.Bookings.First(b => b.Id == testBooking.Id);
        result.IsSuccess.Should().BeTrue();
        verifyBooking.Status.Should().Be(BookingStatus.PendingCancellation);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenOtherUserHasMaxActiveBookingCount_ShouldReturnBooking()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        const int totalSeats = BookingConstants.MaxActiveBookingCountPerUser * 2;
        const int testEventId = 1;
        const int testUser1Id = 1;
        const int testUser2Id = 2;
        await using var context = CreateContext();

        var bookingRepository = new BookingRepository(context);
        var bookingService = new BookingServiceImpl(bookingRepository, NullLogger<BookingServiceImpl>.Instance);

        for (var i = 0; i < totalSeats / 2; i++)
        {
            await bookingService.CreateBookingAsync(testEventId, testUser1Id, ct);
        }

        Func<Task> act = () =>
            bookingService.CreateBookingAsync(testEventId, testUser2Id, TestContext.Current.CancellationToken);

        await act.Should().NotThrowAsync<TooManyActiveBookingsException>();
    }
}