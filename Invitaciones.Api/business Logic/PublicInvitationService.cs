using Invitaciones.Api.Interfaces;
using System.Security.Cryptography;
using System.Text;
using static Invitaciones.Api.DTO.DTOs;

namespace Invitaciones.Api.business_Logic
{
    public sealed class PublicInvitationService : IPublicInvitationService
    {
        private readonly IInvitationRepository _repo;
        private const string QrSecret = "8F2C1A4B9E7D3F6A8C1B2D4E5F7A9C3D6E1F2A4B6C8D0E1F3A5B7C9D2E4F6A8";


        public PublicInvitationService(IInvitationRepository repo)
        {
            _repo = repo;
        }

        public async Task<InvitationResolveResult<InvitationByTokenResponse>> GetInvitationByTokenAsync(
            string token,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
                return InvitationResolveResult<InvitationByTokenResponse>.NotFound();

            token = token.Trim();
            // DB (token hash + SP). Aquí SOLO reglas.
            var db = await _repo.GetInvitationBundleByTokenAsync(token, ct);

            if (db.Status == InvitationBundleStatus.NotFound)
                return InvitationResolveResult<InvitationByTokenResponse>.NotFound();

            if (db.Status == InvitationBundleStatus.Expired)
                return InvitationResolveResult<InvitationByTokenResponse>.Expired();

            // Armar contadores / UI hints (reglas de negocio)
            var total = db.Tickets.Count;
            var confirmedCount = db.Tickets.Count(t => t.ConfirmedAt != null);
            var allConfirmed = total > 0 && confirmedCount == total;

            var primaryAction = allConfirmed ? "VIEW_TICKETS" : "CONFIRM";
            var canViewConfirmedQrs = confirmedCount > 0;

            var response = new InvitationByTokenResponse(
                Invitation: new InvitationDto(
                    db.Invitation.Id,
                    db.Invitation.DisplayName,
                    confirmedCount,
                    total,
                    allConfirmed
                ),
                Event: new EventDto(
                    db.Event.Id,
                    db.Event.Title,
                    db.Event.EventDate,
                    db.Event.LocationName,
                    db.Event.Address
                ),
                Tickets: db.Tickets.Select(t => new TicketDto(
                    t.Id,
                    t.Label,
                    Confirmed: t.ConfirmedAt != null,
                    Used: t.UsedAt != null
                )).ToList(),
                Ui: new UiHintsDto(primaryAction, canViewConfirmedQrs)
            );

            return InvitationResolveResult<InvitationByTokenResponse>.Ok(response);
        }

        public async Task<InvitationResolveResult<InvitationByTokenResponse>> ConfirmTicketsAsync(
                     ConfirmTicketsRequest req,
                     CancellationToken ct)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.Token))
                return InvitationResolveResult<InvitationByTokenResponse>.NotFound();

            var token = req.Token.Trim();

            var statusCode = await _repo.ConfirmTicketsByTokenAsync(token, req.TicketIds, ct);

            // Convertimos el int a enum
            var status = (InvitationBundleStatus)statusCode;

            if (status == InvitationBundleStatus.NotFound)
                return InvitationResolveResult<InvitationByTokenResponse>.NotFound();

            if (status == InvitationBundleStatus.Expired)
                return InvitationResolveResult<InvitationByTokenResponse>.Expired();

            // OK
            return await GetInvitationByTokenAsync(token, ct);
        }


        public async Task<InvitationResolveResult<TicketsWithQrResponse>> GetTicketsWithQrByTokenAsync(
            string token,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(token))
                return InvitationResolveResult<TicketsWithQrResponse>.NotFound();

            token = token.Trim();

            // Reutiliza tu SP bundle (mismo que GET invitación)
            var db = await _repo.GetInvitationBundleByTokenAsync(token, ct);

            if (db.Status == InvitationBundleStatus.NotFound)
                return InvitationResolveResult<TicketsWithQrResponse>.NotFound();

            if (db.Status == InvitationBundleStatus.Expired)
                return InvitationResolveResult<TicketsWithQrResponse>.Expired();

            var nowUtc = DateTime.UtcNow;

            // Regla: solo mostrar QR si ticket está confirmado (recomendado)
            var tickets = db.Tickets.Select(t =>
            {
                var confirmed = t.ConfirmedAt != null;
                var used = t.UsedAt != null;

                var qrPayload = confirmed
                    ? BuildSignedQrPayload(t.Id, db.Invitation.Id, nowUtc)
                    : ""; // o null, o no incluirlo

                return new TicketWithQrDto(
                    Id: t.Id,
                    Label: t.Label,
                    Confirmed: confirmed,
                    Used: used,
                    QrPayload: qrPayload
                );
            }).ToList();

            var total = tickets.Count;
            var confirmedCount = tickets.Count(x => x.Confirmed);
            var allConfirmed = total > 0 && confirmedCount == total;

            var response = new TicketsWithQrResponse(
                Invitation: new InvitationDto(
                    db.Invitation.Id,
                    db.Invitation.DisplayName,
                    confirmedCount,
                    total,
                    allConfirmed
                ),
                Event: new EventDto(
                    db.Event.Id,
                    db.Event.Title,
                    db.Event.EventDate,
                    db.Event.LocationName,
                    db.Event.Address
                ),
                Tickets: tickets
            );

            return InvitationResolveResult<TicketsWithQrResponse>.Ok(response);
        }

        private static string BuildSignedQrPayload(Guid ticketId, Guid invitationId, DateTime nowUtc)
        {
            // Exp: ejemplo: evento + 2 días sería mejor, pero aquí pongo 90 días.
            // Si quieres, lo calculamos con db.Event.EventDate.
            var exp = nowUtc.AddDays(90);
            var expUnix = new DateTimeOffset(exp).ToUnixTimeSeconds();

            // nonce corto para evitar reutilización simple (opcional)
            var nonce = Guid.NewGuid().ToString("N")[..10];

            var data = $"{ticketId:N}|{invitationId:N}|{expUnix}|{nonce}";
            var sig = HmacSha256Hex(QrSecret, data);

            // payload final
            return $"{data}|{sig}";
        }

        private static string HmacSha256Hex(string secret, string data)
        {
            var key = Encoding.UTF8.GetBytes(secret);
            using var h = new HMACSHA256(key);
            var hash = h.ComputeHash(Encoding.UTF8.GetBytes(data));

            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }


    }

}
