namespace foll_backend.Care.Domain.Model.Commands;

public record RejectInvitationCommand(long ActorUserId, long InvitationId);
