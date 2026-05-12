using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Model.Queries;

namespace foll_backend.Care.Domain.Services;

public interface IRelationshipTypeQueryService
{
    Task<IEnumerable<RelationshipType>> Handle(ListRelationshipTypesQuery query);
}
