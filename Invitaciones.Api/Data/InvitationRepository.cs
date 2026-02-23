using Invitaciones.Api.Interfaces;
using System.Data;
using System.Text;
using static Invitaciones.Api.DTO.DTOs;
using System.Security.Cryptography;
using Dapper;


namespace Invitaciones.Api.Data
{
    public sealed class InvitationRepository : IInvitationRepository
    {
        private readonly IDbConnectionFactory _db;

        public InvitationRepository(IDbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<InvitationBundleResult> GetInvitationBundleByTokenAsync(string token, CancellationToken ct)
        {

            try
            {
                // No guardes token plano. Hashea (mínimo SHA-256) para comparar en DB.
                //var tokenHash = Sha256Hex(token);

                using var conn = _db.Create();

                // SP recomendado: dbo.Invitation_GetBundleByToken
                // Devuelve:
                //   ResultSet 1: Meta { StatusCode int } -> 0 OK, 1 NotFound, 2 Expired
                //   ResultSet 2: InvitationRow
                //   ResultSet 3: EventRow
                //   ResultSet 4: TicketRow*
                var p = new DynamicParameters();
                p.Add("@TokenHash", token, DbType.String, ParameterDirection.Input);

                using var multi = await conn.QueryMultipleAsync(
                    sql: "dbo.Invitation_GetBundleByToken",
                    param: p,
                    commandType: CommandType.StoredProcedure
                );

                var meta = await multi.ReadFirstOrDefaultAsync<MetaRow>();
                if (meta is null)
                    return InvitationBundleResult.NotFound();

                if (meta.StatusCode == 1)
                    return InvitationBundleResult.NotFound();

                if (meta.StatusCode == 2)
                    return InvitationBundleResult.Expired();

                var invitation = await multi.ReadFirstAsync<InvitationRow>();
                var ev = await multi.ReadFirstAsync<EventRow>();
                var tickets = (await multi.ReadAsync<TicketRow>()).ToList();

                return new InvitationBundleResult(InvitationBundleStatus.Ok, invitation, ev, tickets);

            }
            catch (Exception ex)
            {
                // Log ex (no exponer detalles a cliente)
                return InvitationBundleResult.NotFound();
            }
        }

        public async Task<int> ConfirmTicketsByTokenAsync(
        string tokenHash,
        IReadOnlyList<Guid>? ticketIds,
        CancellationToken ct)
        {

            try
            {
                using var conn = _db.Create(); // o como lo tengas

                var csv = (ticketIds == null || ticketIds.Count == 0)
                    ? null
                    : string.Join(",", ticketIds);

                var p = new DynamicParameters();
                p.Add("@TokenHash", tokenHash);
                p.Add("@TicketIdsCsv", csv);

                // SP devuelve 1 resultset con { StatusCode }
                var status = await conn.QueryFirstAsync<int>(
                    "dbo.Invitation_ConfirmTicketsByToken",
                    p,
                    commandType: CommandType.StoredProcedure
                );

                return status; // 0 ok, 1 notfound, 2 expired, 3 invalidtickets
            }
            catch (Exception ex)
            {
                // Log ex
                return 1; // NotFound por defecto en caso de error
            }
            }

        private sealed record MetaRow(int StatusCode);

        private static string Sha256Hex(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
