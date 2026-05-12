using foll_backend.IAM.Domain.Model.Commands;
using foll_backend.IAM.Interfaces.REST.Resources;

namespace foll_backend.IAM.Interfaces.REST.Transform;

public static class RegisterCommandFromResourceAssembler
{
    public static RegisterCommand ToCommandFromResource(RegisterResource resource)
    {
        return new RegisterCommand(resource.Email, resource.Password, resource.FirstName, resource.LastName, resource.PhoneNumber);
    }
}
