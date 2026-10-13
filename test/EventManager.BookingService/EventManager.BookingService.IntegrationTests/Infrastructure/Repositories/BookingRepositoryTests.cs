using EventManager.BookingService.Domain.Entities;
using EventManager.BookingService.Infrastructure.Persistence;
using EventManager.BookingService.Infrastructure.Persistence.Repositories;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace EventManager.BookingService.IntegrationTests.Infrastructure.Repositories;

public class BookingRepositoryTests : IAsyncLifetime
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
    public async Task GetBookings_ShouldReturnBookings()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        const int testEventId = 1;
        const int testUserId = 1;

        context.Bookings.AddRange(
            Booking.CreateInstance(testEventId, testUserId),
            Booking.CreateInstance(testEventId, testUserId)
        );
        await context.SaveChangesAsync(ct);

        // Act
        await using var verifyContext = CreateContext();
        var repository = new BookingRepository(verifyContext);
        var bookings = await repository.GetBookings().ToListAsync(ct);

        // Assert
        bookings.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingExists_ShouldReturnBooking()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        const int testEventId = 1;
        const int testUserId = 1;

        var testBooking = Booking.CreateInstance(testEventId, testUserId);
        await context.Bookings.AddAsync(testBooking, ct);
        await context.SaveChangesAsync(ct);

        // Act
        await using var verifyContext = CreateContext();
        var repository = new BookingRepository(verifyContext);
        var booking = await repository.GetBookingByIdAsync(testBooking.Id, ct);

        // Assert
        booking.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBookingByIdAsync_WhenBookingDoNotExist_ShouldReturnNull()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Act
        await using var verifyContext = CreateContext();
        var repository = new BookingRepository(verifyContext);
        var booking = await repository.GetBookingByIdAsync(1, ct);

        // Assert
        booking.Should().BeNull();
    }

    [Fact]
    public async Task GetPendingBookingsAsync_ShouldReturnPendingBookings()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        const int testEventId = 1;
        const int testUserId = 1;

        var pendingBooking = Booking.CreateInstance(testEventId, testUserId);
        var confirmed = Booking.CreateInstance(testEventId, testUserId);
        confirmed.Confirm();
        context.Bookings.AddRange(pendingBooking, confirmed);
        await context.SaveChangesAsync(ct);

        // Act
        await using var verifyContext = CreateContext();
        var repository = new BookingRepository(verifyContext);
        var pendingBookings = await repository.GetPendingBookingsAsync(ct);

        // Assert
        pendingBookings.Should().HaveCount(1);
        pendingBookings.Should().AllSatisfy(b => b.Status.Should().Be(BookingStatus.Pending));
    }

    [Fact]
    public async Task AddBookingAsync_ShouldAddBookingToDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        const int testEventId = 1;
        const int testUserId = 1;

        // Act
        await using var actContext = CreateContext();
        var booking = Booking.CreateInstance(testEventId, testUserId);

        var repository = new BookingRepository(actContext);
        await repository.AddBookingAsync(booking, ct);
        await repository.SaveChangesAsync(ct);

        // Assert
        await using var verifyContext = CreateContext();
        var addedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.EventId == testEventId, ct);

        addedBooking.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveBooking_ShouldRemoveFromDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        const int testEventId = 1;
        const int testUserId = 1;

        await context.Bookings.AddAsync(Booking.CreateInstance(testEventId, testUserId), ct);
        await context.SaveChangesAsync(ct);

        // Act
        await using var actContext = CreateContext();
        var repository = new BookingRepository(actContext);
        var bookingToRemove = await repository.GetBookings().FirstAsync(b => b.EventId == testEventId, ct);
        repository.RemoveBooking(bookingToRemove);
        await repository.SaveChangesAsync(ct);

        // Assert
        await using var verifyContext = CreateContext();
        var removedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.EventId == testEventId, ct);

        removedBooking.Should().BeNull();
    }

    [Fact]
    public async Task AddBookingAsync_ShouldFillCreatedAt()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        const int testEventId = 1;
        const int testUserId = 1;

        // Act
        await using var actContext = CreateContext();
        var booking = Booking.CreateInstance(testEventId, testUserId);

        var repository = new BookingRepository(actContext);
        await repository.AddBookingAsync(booking, ct);
        await repository.SaveChangesAsync(ct);

        // Assert
        await using var verifyContext = CreateContext();
        var addedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.EventId == testEventId, ct);

        addedBooking.Should().NotBeNull();
        addedBooking.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task GetActiveBookingCountForUserAsync_ShouldReturnCountOfActiveBookings()
    {
        const int expectedCount = 2;
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        const int testEventId = 1;
        const int testUserId = 1;

        var confirmedBooking = Booking.CreateInstance(testEventId, testUserId);
        confirmedBooking.Confirm();
        var rejectedBooking = Booking.CreateInstance(testEventId, testUserId);
        rejectedBooking.Reject();
        var pendingBooking = Booking.CreateInstance(testEventId, testUserId);
        await context.Bookings.AddRangeAsync(confirmedBooking, rejectedBooking, pendingBooking);
        await context.SaveChangesAsync(ct);

        // Act
        await using var verifyContext = CreateContext();
        var bookingRepository = new BookingRepository(verifyContext);
        var actualCount = await bookingRepository.GetActiveBookingCountForUserAsync(testUserId, ct);

        // Assert
        actualCount.Should().Be(expectedCount);
    }
}