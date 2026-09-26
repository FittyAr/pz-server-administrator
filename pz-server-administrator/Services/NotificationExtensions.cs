using System;
using System.Threading.Tasks;
using Microsoft.FluentUI.AspNetCore.Components;

namespace pz_server_administrator.Services;

/// <summary>
/// Extension methods for INotificationService to simplify toast notifications in FluentUI v5.
/// </summary>
public static class NotificationExtensions
{
    public static Task<ToastResult> ShowSuccessAsync(this INotificationService service, string message, string? title = null, TimeSpan? lifetime = null)
    {
        return service.ShowSuccessToastAsync(title ?? message, title != null ? message : null, (int?)lifetime?.TotalMilliseconds);
    }

    public static Task<ToastResult> ShowErrorAsync(this INotificationService service, string message, string? title = null, TimeSpan? lifetime = null)
    {
        return service.ShowErrorToastAsync(title ?? message, title != null ? message : null, (int?)lifetime?.TotalMilliseconds);
    }

    public static Task<ToastResult> ShowWarningAsync(this INotificationService service, string message, string? title = null, TimeSpan? lifetime = null)
    {
        return service.ShowWarningToastAsync(title ?? message, title != null ? message : null, (int?)lifetime?.TotalMilliseconds);
    }

    public static Task<ToastResult> ShowInfoAsync(this INotificationService service, string message, string? title = null, TimeSpan? lifetime = null)
    {
        return service.ShowInfoToastAsync(title ?? message, title != null ? message : null, (int?)lifetime?.TotalMilliseconds);
    }
}
