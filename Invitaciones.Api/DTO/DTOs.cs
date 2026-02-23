namespace Invitaciones.Api.DTO
{
    public class DTOs
    {
        public sealed record InvitationByTokenResponse(
            InvitationDto Invitation,
            EventDto Event,
            IReadOnlyList<TicketDto> Tickets,
            UiHintsDto Ui
        );

        public sealed record InvitationDto(
            Guid Id,
            string DisplayName,
            int ConfirmedCount,
            int TotalTickets,
            bool AllConfirmed
        );

        public sealed record EventDto(
            Guid Id,
            string Title,
            DateTime EventDate,
            string LocationName,
            string Address
        );

        public sealed record TicketDto(
            Guid Id,
            string Label,
            bool Confirmed,
            bool Used
        );

        public sealed record UiHintsDto(
            string PrimaryAction,          // "CONFIRM" | "VIEW_TICKETS"
            bool CanViewConfirmedQrs
        );

        public enum InvitationBundleStatus { Ok, NotFound, Expired }

        public sealed record InvitationBundleResult(
            InvitationBundleStatus Status,
            InvitationRow Invitation,
            EventRow Event,
            List<TicketRow> Tickets
        )
        {
            public static InvitationBundleResult NotFound() =>
                new(InvitationBundleStatus.NotFound, new InvitationRow(), new EventRow(), new());

            public static InvitationBundleResult Expired() =>
                new(InvitationBundleStatus.Expired, new InvitationRow(), new EventRow(), new());
        }

        public sealed record InvitationRow
        {
            public Guid Id { get; init; }
            public string DisplayName { get; init; } = "";
        }

        public sealed record EventRow
        {
            public Guid Id { get; init; }
            public string Title { get; init; } = "";
            public DateTime EventDate { get; init; }
            public string LocationName { get; init; } = "";
            public string Address { get; init; } = "";
        }

        public sealed record TicketRow
        {
            public Guid Id { get; init; }
            public string Label { get; init; } = "";
            public DateTime? ConfirmedAt { get; init; }
            public DateTime? UsedAt { get; init; }
        }

        public sealed record ConfirmTicketsRequest(
            string Token,
            IReadOnlyList<Guid>? TicketIds = null
        );

        public sealed record TicketsWithQrResponse(
            InvitationDto Invitation,
            EventDto Event,
            IReadOnlyList<TicketWithQrDto> Tickets
        );

        public sealed record TicketWithQrDto(
            Guid Id,
            string Label,
            bool Confirmed,
            bool Used,
            string QrPayload
        );


    }



}
