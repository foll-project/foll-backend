namespace foll_backend.Care.Domain.Model.Commands;

public record AcceptInvitationCommand(long ActorUserId, long InvitationId);
