using Coolzo.Domain.Entities;
using Coolzo.Domain.Enums;
using Coolzo.Application.Common.Security;

namespace Coolzo.Application.Common.Interfaces;

public interface ICustomerPasswordPolicyService
{
    Task<CustomerPasswordPolicySnapshot> ResolvePolicyAsync(CancellationToken cancellationToken);

    Task<PreparedCustomerPassword> PreparePasswordAsync(
        string? providedPassword,
        CustomerPasswordChangeSource changeSource,
        long? existingUserId,
        CancellationToken cancellationToken);

    Task<bool> VerifyPasswordAsync(User user, string providedPassword, CancellationToken cancellationToken);

    Task<CustomerPasswordStateSnapshot> GetPasswordStateAsync(User user, CancellationToken cancellationToken);

    Task ApplyPasswordAsync(
        User user,
        PreparedCustomerPassword preparedPassword,
        string actorName,
        string ipAddress,
        CancellationToken cancellationToken);
}
