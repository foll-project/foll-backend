using foll_backend.Care.Domain.Model.Entities;

namespace foll_backend.Care.Domain.Repositories;

public interface IRelationshipTypeRepository
{
    Task<RelationshipType?> FindByIdAsync(short id);
    Task<IEnumerable<RelationshipType>> ListAsync();
}
