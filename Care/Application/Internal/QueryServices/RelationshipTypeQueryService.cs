using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.Queries;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Care.Domain.Services;

namespace foll_backend.Care.Application.Internal.QueryServices;

public class RelationshipTypeQueryService : IRelationshipTypeQueryService
{
    private readonly IRelationshipTypeRepository _repository;

    public RelationshipTypeQueryService(IRelationshipTypeRepository repository) => _repository = repository;

    public async Task<IEnumerable<RelationshipType>> Handle(ListRelationshipTypesQuery query)
    {
        return await _repository.ListAsync();
    }
}
