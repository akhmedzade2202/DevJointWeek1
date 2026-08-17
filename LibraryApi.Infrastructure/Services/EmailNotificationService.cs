using LibraryApi.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraryApi.Infrastructure.Services;

/// <summary>
/// Simulates async email sending without blocking the HTTP request pipeline.
/// In a real implementation this would call an SMTP server or mail provider SDK.
/// </summary>
public class EmailNotificationService : IEmailNotificationService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly int _simulatedDelayMs;

    public EmailNotificationService(ILogger<EmailNotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _simulatedDelayMs = configuration.GetValue<int>("Email:SimulatedDelayMs", 100);
    }

    public async Task SendLoanConfirmationAsync(
        string memberEmail,
        string memberName,
        string bookTitle,
        DateTime loanDate)
    {
        // Simulate I/O delay (e.g., calling an SMTP relay).
        await Task.Delay(_simulatedDelayMs);

        _logger.LogInformation(
            "[EMAIL] Loan confirmation sent → To: {Email} | Member: {Name} | Book: '{Title}' | Date: {Date:yyyy-MM-dd}",
            memberEmail, memberName, bookTitle, loanDate);
    }

    public async Task SendOverdueReminderAsync(
        string memberEmail,
        string memberName,
        string bookTitle,
        DateTime loanDate)
    {
        await Task.Delay(_simulatedDelayMs);

        _logger.LogInformation(
            "[EMAIL] Overdue reminder sent → To: {Email} | Member: {Name} | Book: '{Title}' | Loaned: {Date:yyyy-MM-dd}",
            memberEmail, memberName, bookTitle, loanDate);
    }
}
