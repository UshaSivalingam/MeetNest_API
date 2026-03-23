using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace MeetNest.Infrastructure.Services;

public interface IEmailService
{
    // ── Admin emails ──────────────────────────────────────────────
    Task SendBookingSubmittedAsync(string employeeName, string roomName, string branchName, DateTime start, DateTime end);
    Task SendBookingCancelledToAdminAsync(string employeeName, string roomName, string branchName, DateTime start, DateTime end);
    Task SendMeetingEndReminderAsync(string roomName, string branchName, DateTime endTime);

    // ── Employee emails ───────────────────────────────────────────
    Task SendInstantBookingConfirmedAsync(string employeeEmail, string employeeName, string roomName, string branchName, DateTime start, DateTime end);
    Task SendBookingApprovedAsync(string employeeEmail, string employeeName, string roomName, string branchName, DateTime start, DateTime end);
    Task SendBookingRejectedAsync(string employeeEmail, string employeeName, string roomName, string? reason);
    Task SendBookingAutoRejectedAsync(string employeeEmail, string employeeName, string roomName, DateTime start, DateTime end);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    // ── IST timezone — works on both Windows and Linux ───────────
    private static readonly TimeZoneInfo Ist =
        TimeZoneInfo.GetSystemTimeZones()
            .FirstOrDefault(t => t.Id == "India Standard Time" || t.Id == "Asia/Kolkata")
        ?? TimeZoneInfo.Utc;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    // ── Convert UTC → IST before display ─────────────────────────
    private static DateTime ToIst(DateTime dt) =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(dt, DateTimeKind.Utc), Ist);

    // ── 1. Booking submitted → SuperAdmin ─────────────────────────
    public Task SendBookingSubmittedAsync(
        string employeeName, string roomName, string branchName,
        DateTime start, DateTime end) =>
        SendToAdminAsync(
            subject: $"📋 New Booking Request — {roomName}",
            body: HtmlTemplate("New Booking Request", "#2563EB", "📋", new[]
            {
                $"<b>{employeeName}</b> has submitted a booking request.",
                $"<b>Room:</b> {roomName}",
                $"<b>Branch:</b> {branchName}",
                $"<b>Date:</b> {ToIst(start):dddd, dd MMM yyyy}",
                $"<b>Time:</b> {ToIst(start):hh:mm tt} – {ToIst(end):hh:mm tt} IST",
                "Log in to MeetNest to approve or reject this request.",
            }));

    // ── 2. Booking cancelled → SuperAdmin ─────────────────────────
    public Task SendBookingCancelledToAdminAsync(
        string employeeName, string roomName, string branchName,
        DateTime start, DateTime end) =>
        SendToAdminAsync(
            subject: $"🚫 Booking Cancelled — {roomName}",
            body: HtmlTemplate("Booking Cancelled", "#FC6D84", "🚫", new[]
            {
                $"<b>{employeeName}</b> has cancelled their booking.",
                $"<b>Room:</b> {roomName}",
                $"<b>Branch:</b> {branchName}",
                $"<b>Date:</b> {ToIst(start):dddd, dd MMM yyyy}",
                $"<b>Time:</b> {ToIst(start):hh:mm tt} – {ToIst(end):hh:mm tt} IST",
                "The slot is now available for other bookings.",
            }));

    // ── 3. Meeting end reminder → SuperAdmin ──────────────────────
    public Task SendMeetingEndReminderAsync(
        string roomName, string branchName, DateTime endTime) =>
        SendToAdminAsync(
            subject: $"🔔 Meeting Ended — {roomName}",
            body: HtmlTemplate("Meeting Has Ended", "#2563EB", "🔔", new[]
            {
                $"The meeting in <b>{roomName}</b> has ended.",
                $"<b>Branch:</b> {branchName}",
                $"<b>Ended at:</b> {ToIst(endTime):hh:mm tt} IST",
                "You can now update the room status, edit, or delete it.",
            }));

    // ── 4. Instant booking confirmed → Employee ───────────────────
    public Task SendInstantBookingConfirmedAsync(
        string employeeEmail, string employeeName,
        string roomName, string branchName,
        DateTime start, DateTime end) =>
        SendToEmployeeAsync(employeeEmail,
            subject: $"⚡ Booking Confirmed — {roomName}",
            body: HtmlTemplate("Your Booking is Confirmed!", "#16A34A", "⚡", new[]
            {
                $"Hi <b>{employeeName}</b>, your room has been booked instantly.",
                $"<b>Room:</b> {roomName}",
                $"<b>Branch:</b> {branchName}",
                $"<b>Date:</b> {ToIst(start):dddd, dd MMM yyyy}",
                $"<b>Time:</b> {ToIst(start):hh:mm tt} – {ToIst(end):hh:mm tt} IST",
                "No approval needed — you're all set! 🎉",
            }));

    // ── 5. Booking approved → Employee ────────────────────────────
    public Task SendBookingApprovedAsync(
        string employeeEmail, string employeeName,
        string roomName, string branchName,
        DateTime start, DateTime end) =>
        SendToEmployeeAsync(employeeEmail,
            subject: $"✅ Booking Approved — {roomName}",
            body: HtmlTemplate("Your Booking is Approved!", "#16A34A", "✅", new[]
            {
                $"Hi <b>{employeeName}</b>, your booking request has been approved.",
                $"<b>Room:</b> {roomName}",
                $"<b>Branch:</b> {branchName}",
                $"<b>Date:</b> {ToIst(start):dddd, dd MMM yyyy}",
                $"<b>Time:</b> {ToIst(start):hh:mm tt} – {ToIst(end):hh:mm tt} IST",
                "See you there! 🎉",
            }));

    // ── 6. Booking rejected → Employee ────────────────────────────
    public Task SendBookingRejectedAsync(
        string employeeEmail, string employeeName,
        string roomName, string? reason) =>
        SendToEmployeeAsync(employeeEmail,
            subject: $"❌ Booking Rejected — {roomName}",
            body: HtmlTemplate("Booking Request Rejected", "#DC2626", "❌", new[]
            {
                $"Hi <b>{employeeName}</b>, your booking request was rejected.",
                $"<b>Room:</b> {roomName}",
                $"<b>Reason:</b> {(string.IsNullOrWhiteSpace(reason) ? "No reason provided." : reason)}",
                "You may submit a new request for a different time slot.",
            }));

    // ── 7. Auto-rejected due to conflict → Employee ───────────────
    public Task SendBookingAutoRejectedAsync(
        string employeeEmail, string employeeName,
        string roomName, DateTime start, DateTime end) =>
        SendToEmployeeAsync(employeeEmail,
            subject: $"❌ Booking No Longer Available — {roomName}",
            body: HtmlTemplate("Booking Request Closed", "#DC2626", "❌", new[]
            {
                $"Hi <b>{employeeName}</b>, unfortunately your booking request could not be fulfilled.",
                $"<b>Room:</b> {roomName}",
                $"<b>Date:</b> {ToIst(start):dddd, dd MMM yyyy}",
                $"<b>Time:</b> {ToIst(start):hh:mm tt} – {ToIst(end):hh:mm tt} IST",
                "Another booking was approved for this time slot.",
                "You may submit a new request for a different time slot.",
            }));

    // ── Send to SuperAdmin ────────────────────────────────────────
    private Task SendToAdminAsync(string subject, string body)
    {
        var adminEmail = _config["Email:AdminEmail"]!;
        return SendAsync(adminEmail, subject, body);
    }

    // ── Send to Employee (DevRedirectEmail overrides in demo mode) ─
    private Task SendToEmployeeAsync(string employeeEmail, string subject, string body)
    {
        var redirect = _config["Email:DevRedirectEmail"];
        var toEmail = string.IsNullOrWhiteSpace(redirect) ? employeeEmail : redirect;
        return SendAsync(toEmail, subject, body);
    }

    // ── Core SMTP send ────────────────────────────────────────────
    private async Task SendAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["Email:FromName"] ?? "MeetNest",
            _config["Email:Username"]!));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _config["Email:Host"]!,
            int.Parse(_config["Email:Port"]!),
            SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            _config["Email:Username"]!,
            _config["Email:Password"]!);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    // ── HTML email template ───────────────────────────────────────
    private static string HtmlTemplate(
        string title, string color, string icon, string[] lines)
    {
        var rows = string.Join("", lines.Select(l =>
            $"<tr><td style='padding:6px 0;color:#374151;font-size:15px;line-height:1.6'>{l}</td></tr>"));

        return $"""
        <!DOCTYPE html><html>
        <body style="margin:0;padding:0;background:#f1f5f9;font-family:Arial,sans-serif">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr><td align="center" style="padding:40px 20px">
              <table width="560" cellpadding="0" cellspacing="0"
                style="background:#fff;border-radius:16px;overflow:hidden;
                       box-shadow:0 4px 24px rgba(0,0,0,0.08)">
                <tr>
                  <td style="background:{color};padding:28px 36px">
                    <span style="color:#fff;font-size:22px;font-weight:bold">{title}</span>
                  </td>
                </tr>
                <tr>
                  <td style="padding:28px 36px">
                    <table width="100%" cellpadding="0" cellspacing="0">{rows}</table>
                  </td>
                </tr>
                <tr>
                  <td style="padding:16px 36px;background:#f8fafc;
                             border-top:1px solid #e2e8f0;
                             font-size:12px;color:#94a3b8">
                    Automated message from MeetNest — please do not reply.
                  </td>
                </tr>
              </table>
            </td></tr>
          </table>
        </body></html>
        """;
    }
}