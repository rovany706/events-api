using EventManager.Domain.Entities;
using EventManager.Infrastructure.Persistence;
using EventManager.Infrastructure.Persistence.Repositories;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventManager.API.IntegrationTests.Domain.Repositories;

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

    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new AppDbContext(options);
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
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        context.Bookings.AddRange(
            Booking.CreateInstance(testEvent.Id),
            Booking.CreateInstance(testEvent.Id)
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
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        var testBooking = Booking.CreateInstance(testEvent.Id);
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
    public async Task GetBookingByIdAsync_WhenBookingExists_ShouldReturnBookingWithEvent()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        var testBooking = Booking.CreateInstance(testEvent.Id);
        await context.Bookings.AddAsync(testBooking, ct);
        await context.SaveChangesAsync(ct);
        
        // Act
        await using var verifyContext = CreateContext();
        var repository = new BookingRepository(verifyContext);
        var booking = await repository.GetBookingByIdAsync(testBooking.Id, ct);
        
        // Assert
        booking.Should().NotBeNull();
        booking.Event.Should().NotBeNull();
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
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        var pendingBooking = Booking.CreateInstance(testEvent.Id);
        var confirmed = Booking.CreateInstance(testEvent.Id);
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
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        // Act
        await using var actContext = CreateContext();
        var booking = Booking.CreateInstance(testEvent.Id);
        
        var repository = new BookingRepository(actContext);
        await repository.AddBookingAsync(booking, ct);
        await repository.SaveChangesAsync(ct);
        
        // Assert
        await using var verifyContext = CreateContext();
        var addedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.EventId == testEvent.Id, ct);
        
        addedBooking.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveBooking_ShouldRemoveFromDatabase()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        await context.Bookings.AddAsync(Booking.CreateInstance(testEvent.Id), ct);
        await context.SaveChangesAsync(ct);
        
        // Act
        await using var actContext = CreateContext();
        var repository = new BookingRepository(actContext);
        var bookingToRemove = await repository.GetBookings().FirstAsync(b => b.EventId == testEvent.Id, ct);
        repository.RemoveBooking(bookingToRemove);
        await repository.SaveChangesAsync(ct);
        
        // Assert
        await using var verifyContext = CreateContext();
        var removedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.EventId == testEvent.Id, ct);

        removedBooking.Should().BeNull();
    }

    [Fact]
    public async Task AddBookingAsync_ShouldFillCreatedAt()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        // Act
        await using var actContext = CreateContext();
        var booking = Booking.CreateInstance(testEvent.Id);
        
        var repository = new BookingRepository(actContext);
        await repository.AddBookingAsync(booking, ct);
        await repository.SaveChangesAsync(ct);
        
        // Assert
        await using var verifyContext = CreateContext();
        var addedBooking = await verifyContext.Bookings.FirstOrDefaultAsync(b => b.EventId == testEvent.Id, ct);

        addedBooking.Should().NotBeNull();
        addedBooking.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task LoadBookingsWithEvent_ShouldReturnBookingWithEvent()
    {
        var ct = TestContext.Current.CancellationToken;
        await ResetDatabaseAsync();

        // Arrange
        await using var context = CreateContext();
        var testEvent = Event.CreateInstance(
            "Event 1",
            "Description",
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 0, 1, 2), DateTimeKind.Utc),
            DateTime.SpecifyKind(new DateTime(2026, 10, 1, 1, 2, 3), DateTimeKind.Utc),
            10);
        await context.Events.AddAsync(testEvent, ct);
        await context.SaveChangesAsync(ct);

        var testBooking = Booking.CreateInstance(testEvent.Id);
        await context.Bookings.AddAsync(testBooking, ct);
        await context.SaveChangesAsync(ct);
        
        // Act
        await using var verifyContext = CreateContext();
        var booking = await verifyContext.Bookings.Include(b => b.Event).FirstAsync(b => b.EventId == testEvent.Id, ct);
        
        // Assert
        booking.Event.Should().NotBeNull();
    }
}