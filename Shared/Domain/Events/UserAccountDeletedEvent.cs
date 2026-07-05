using MediatR;

namespace foll_backend.Shared.Domain.Events;

public record UserAccountDeletedEvent(long UserId) : INotification;
