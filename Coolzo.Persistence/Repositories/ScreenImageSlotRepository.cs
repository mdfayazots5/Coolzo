using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class ScreenImageSlotRepository : IScreenImageSlotRepository
{
    private readonly CoolzoDbContext _dbContext;

    public ScreenImageSlotRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(ScreenImageSlot screenImageSlot, CancellationToken cancellationToken)
    {
        return _dbContext.ScreenImageSlots.AddAsync(screenImageSlot, cancellationToken).AsTask();
    }

    public Task<ScreenImageSlot?> GetByIdAsync(long screenImageSlotId, CancellationToken cancellationToken)
    {
        return _dbContext.ScreenImageSlots
            .FirstOrDefaultAsync(entity => entity.ScreenImageSlotId == screenImageSlotId && !entity.IsDeleted, cancellationToken);
    }

    public Task<bool> ExistsAsync(string pageKey, string slotKey, string breakpoint, long? excludedId, CancellationToken cancellationToken)
    {
        return _dbContext.ScreenImageSlots.AnyAsync(
            entity =>
                !entity.IsDeleted &&
                entity.PageKey == pageKey &&
                entity.SlotKey == slotKey &&
                entity.Breakpoint == breakpoint &&
                (!excludedId.HasValue || entity.ScreenImageSlotId != excludedId.Value),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<ScreenImageSlot>> SearchAsync(string? pageKey, bool? isActive, CancellationToken cancellationToken)
    {
        var query = _dbContext.ScreenImageSlots
            .AsNoTracking()
            .Where(entity => !entity.IsDeleted);

        if (!string.IsNullOrWhiteSpace(pageKey))
        {
            query = query.Where(entity => entity.PageKey == pageKey);
        }

        if (isActive.HasValue)
        {
            query = query.Where(entity => entity.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(entity => entity.PageKey)
            .ThenBy(entity => entity.SlotKey)
            .ThenBy(entity => entity.Breakpoint)
            .ToArrayAsync(cancellationToken);
    }
}
