using LibraryApi.Application.Interfaces.Services;
using LibraryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LibraryApi.Infrastructure.BackgroundJobs;

/// <summary>
/// Hosted background service that runs once per day.
/// It scans for overdue loans and sends email reminders asynchronously.
/// In a real scenario it could also archive or flag long-overdue records.
/// </summary>
public class LoanCleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LoanCleanupJob> _logger;
    private readonly int _overdueDaysThreshold;

    // Run every 24 hours.
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public LoanCleanupJob(
        IServiceProvider serviceProvider,
        ILogger<LoanCleanupJob> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _overdueDaysThreshold = configuration.GetValue<int>("LoanCleanup:OverdueDaysThreshold", 30);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LoanCleanupJob started. Will run every {Hours} hours.", Interval.TotalHours);

        // Wait 30 seconds after startup before the first run so the app is fully initialized.
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCleanupAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task RunCleanupAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("LoanCleanupJob — starting overdue loan scan at {Time:HH:mm:ss}", DateTime.UtcNow);

        try
        {
            // Use a new DI scope because DbContext is scoped.
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();

            var cutoffDate = DateTime.UtcNow.AddDays(-_overdueDaysThreshold);

            // Find all loans that are overdue (not returned, and past the threshold).
            var overdueLoans = await dbContext.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.ReturnDate == null && l.LoanDate <= cutoffDate)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("LoanCleanupJob — found {Count} overdue loan(s) (>{Days} days).",
                overdueLoans.Count, _overdueDaysThreshold);

            foreach (var loan in overdueLoans)
            {
                // Fire-and-forget email — we don't await so we don't block the loop.
                _ = emailService.SendOverdueReminderAsync(
                    loan.Member.Email,
                    $"{loan.Member.FirstName} {loan.Member.LastName}",
                    loan.Book.Title,
                    loan.LoanDate);
            }

            _logger.LogInformation("LoanCleanupJob — completed at {Time:HH:mm:ss}", DateTime.UtcNow);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "LoanCleanupJob — unhandled error during loan scan.");
        }
    }
}
