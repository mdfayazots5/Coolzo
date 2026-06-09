using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IScreenImageSlotRepository
{
    Task AddAsync(ScreenImageSlot screenImageSlot, CancellationToken cancellationToken);

    Task<ScreenImageSlot?> GetByIdAsync(long screenImageSlotId, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string pageKey, string slotKey, string breakpoint, long? excludedId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ScreenImageSlot>> SearchAsync(string? pageKey, bool? isActive, CancellationToken cancellationToken);
}
