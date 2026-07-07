using foll_backend.IAM.Domain.Model.Commands;

namespace foll_backend.IAM.Domain.Services;

public interface IUserCommandService
{
    Task Handle(RegisterCommand command);
    Task Handle(DeleteaccountCommand command);
}
