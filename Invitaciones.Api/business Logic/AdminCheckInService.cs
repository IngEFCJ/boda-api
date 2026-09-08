using System.Security.Cryptography;
using System.Text;
using Invitaciones.Api.DTO;
using Invitaciones.Api.Interfaces;

namespace Invitaciones.Api.business_Logic;

public sealed class AdminCheckInService : IAdminCheckInService
{
    private readonly IAdminCheckInRepository _repo;
    private readonly string _qrSecret;

    public AdminCheckInService(IAdminCheckInRepository repo, IConfiguration configuration)
    {
        _repo = repo;
        _qrSecret = configuration["QrSecret"]
            ?? throw new InvalidOperationException("Missing configuration: QrSecret");
    }

    public async Task<CheckInResultDto> ProcessQrScanAsync(QrScanRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.QrPayload))
            return InvalidResult();

        // 1. Parse payload: ticketId|invitationId|expUnix|nonce|hmac
        var parts = req.QrPayload.Split('|');
        if (parts.Length != 5)
            return InvalidResult();

        if (!Guid.TryParse(parts[0], out var ticketId))
            return InvalidResult();

        if (!Guid.TryParse(parts[1], out _))
            return InvalidResult();

        if (!long.TryParse(parts[2], out var expUnix))
            return InvalidResult();

        var nonce = parts[3];
        var providedHmac = parts[4];

        // 2. Verify HMAC-SHA256
        var dataToSign = $"{parts[0]}|{parts[1]}|{parts[2]}|{nonce}";
        var computedHmac = ComputeHmacSha256Hex(_qrSecret, dataToSign);

        if (!string.Equals(computedHmac, providedHmac, StringComparison.OrdinalIgnoreCase))
            return InvalidResult();

        // 3. Check expiration
        var nowUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (expUnix <= nowUnix)
            return ExpiredResult();

        // 4. Check ticket exists
        var ticket = await _repo.GetTicketForCheckInAsync(ticketId, ct);
        if (ticket is null)
            return NotFoundResult();

        // 5. Check already used
        if (ticket.UsedAt is not null)
        {
            return new CheckInResultDto(
                Status: "already_checked_in",
                GuestName: ticket.Label,
                InvitationName: ticket.DisplayName,
                TableNumber: ticket.TableNumber,
                SeatNumber: ticket.SeatNumber,
                CheckedInAt: null,
                OriginalCheckInAt: ticket.UsedAt
            );
        }

        // 6. Mark as used
        var marked = await _repo.MarkUsedAsync(ticketId, ct);
        if (!marked)
        {
            // Race condition: another request marked it first
            var refreshed = await _repo.GetTicketForCheckInAsync(ticketId, ct);
            return new CheckInResultDto(
                Status: "already_checked_in",
                GuestName: refreshed?.Label,
                InvitationName: refreshed?.DisplayName,
                TableNumber: refreshed?.TableNumber,
                SeatNumber: refreshed?.SeatNumber,
                CheckedInAt: null,
                OriginalCheckInAt: refreshed?.UsedAt
            );
        }

        // 7. Return success with seat info
        return new CheckInResultDto(
            Status: "success",
            GuestName: ticket.Label,
            InvitationName: ticket.DisplayName,
            TableNumber: ticket.TableNumber,
            SeatNumber: ticket.SeatNumber,
            CheckedInAt: DateTime.UtcNow,
            OriginalCheckInAt: null
        );
    }

    public async Task<PagedResult<CheckInHistoryDto>> GetHistoryAsync(Guid eventId, int page, CancellationToken ct)
    {
        const int pageSize = 20;
        var result = await _repo.GetCheckInHistoryAsync(eventId, page, pageSize, ct);

        var items = result.Items.Select(r => new CheckInHistoryDto(
            GuestName: r.GuestName,
            InvitationName: r.InvitationName,
            TableNumber: r.TableNumber,
            SeatNumber: r.SeatNumber,
            CheckedInAt: r.CheckedInAt
        )).ToList();

        return new PagedResult<CheckInHistoryDto>(items, result.TotalCount, result.Page, result.PageSize);
    }

    private static CheckInResultDto InvalidResult() =>
        new("invalid", null, null, null, null, null, null);

    private static CheckInResultDto ExpiredResult() =>
        new("expired", null, null, null, null, null, null);

    private static CheckInResultDto NotFoundResult() =>
        new("not_found", null, null, null, null, null, null);

    private static string ComputeHmacSha256Hex(string secret, string data)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
