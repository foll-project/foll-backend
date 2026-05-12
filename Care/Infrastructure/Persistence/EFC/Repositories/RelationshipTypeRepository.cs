using foll_backend.Care.Domain.Model.Entities;
using foll_backend.Care.Domain.Repositories;
using foll_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace foll_backend.Care.Infrastructure.Persistence.EFC.Repositories;

public class RelationshipTypeRepository : IRelationshipTypeRepository
{
    private readonly AppDbContext _context;

    public RelationshipTypeRepository(AppDbContext context) => _context = context;

    public async Task<RelationshipType?> FindByIdAsync(short id)
    {
        if (id <= 0) return null;
        return await _context.Set<RelationshipType>().FindAsync(id);
    }

    public async Task<IEnumerable<RelationshipType>> ListAsync()
    {
        return await _context.Set<RelationshipType>().OrderBy(r => r.RelationshipTypeId).ToListAsync();
    }
}
