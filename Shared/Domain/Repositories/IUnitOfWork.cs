namespace foll_backend.Shared.Domain.Repositories;

public interface IUnitOfWork
{
    Task CompleteAsync();
}
