namespace LibraryApi.Application.Interfaces.Services;

/// <summary>
/// Async email notification service (fire-and-forget simulation).
/// </summary>
public interface IEmailNotificationService
{
    /// <summary>Sends a loan confirmation email without blocking the calling thread.</summary>
    Task SendLoanConfirmationAsync(string memberEmail, string memberName, string bookTitle, DateTime loanDate);

    /// <summary>Sends an overdue reminder email without blocking the calling thread.</summary>
    Task SendOverdueReminderAsync(string memberEmail, string memberName, string bookTitle, DateTime loanDate);
}
