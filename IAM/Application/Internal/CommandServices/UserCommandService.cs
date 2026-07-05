using foll_backend.IAM.Application.OutboundServices;
using foll_backend.IAM.Domain.Model.Commands;
using foll_backend.IAM.Domain.Model.Entities;
using foll_backend.IAM.Domain.Repositories;
using foll_backend.IAM.Domain.Services;
using foll_backend.Shared.Domain.Repositories;
using foll_backend.Shared.Domain.Events;
using MediatR;

namespace foll_backend.IAM.Application.Internal.CommandServices;

public class UserCommandService : IUserCommandService
{
    private readonly IUserRepository _userRepository;
    private readonly IHashingService _hashingService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public UserCommandService(
        IUserRepository userRepository,
        IHashingService hashingService,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _userRepository = userRepository;
        _hashingService = hashingService;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task Handle(RegisterCommand command)
    {
        if (await _userRepository.ExistsByEmailAsync(command.Email))
        {
            throw new Exception($"El correo {command.Email} ya se encuentra registrado.");
        }

        var hashedPassword = _hashingService.HashPassword(command.Password);

        var user = new User(command.Email, hashedPassword, command.FirstName, command.LastName, command.PhoneNumber);

        await _userRepository.AddAsync(user);

        await _unitOfWork.CompleteAsync();
    }

    public async Task Handle(DeleteaccountCommand command)
    {
        var user = await _userRepository.FindByIdAsync(command.UserId);
        if (user == null)
            throw new InvalidOperationException("Usuario no encontrado.");

        if (!_hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new InvalidOperationException("Contraseña incorrecta. No se puede eliminar la cuenta.");

        await _userRepository.DeleteByUserIdAsync(command.UserId);
        await _unitOfWork.CompleteAsync();

        await _mediator.Publish(new foll_backend.Shared.Domain.Events.UserAccountDeletedEvent(command.UserId));
    }
}
