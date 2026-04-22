using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IOtpVerificationRepository
{
    Task AddAsync(OtpVerification otpVerification, CancellationToken cancellationToken);

    Task<OtpVerification?> GetActiveByCodeAsync(string purpose, string code, DateTime utcNow, CancellationToken cancellationToken);

    Task<OtpVerification?> GetActiveByUserAndCodeAsync(long userId, string purpose, string code, DateTime utcNow, CancellationToken cancellationToken);
}
