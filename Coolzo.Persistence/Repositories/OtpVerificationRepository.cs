using Coolzo.Application.Common.Interfaces;
using Coolzo.Domain.Entities;
using Coolzo.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Coolzo.Persistence.Repositories;

public sealed class OtpVerificationRepository : IOtpVerificationRepository
{
    private readonly CoolzoDbContext _dbContext;

    public OtpVerificationRepository(CoolzoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(OtpVerification otpVerification, CancellationToken cancellationToken)
    {
        return _dbContext.OtpVerifications.AddAsync(otpVerification, cancellationToken).AsTask();
    }

    public Task<OtpVerification?> GetActiveByCodeAsync(
        string purpose,
        string code,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return _dbContext.OtpVerifications
            .FirstOrDefaultAsync(
                otpVerification => !otpVerification.IsDeleted &&
                    !otpVerification.IsConsumed &&
                    otpVerification.Purpose == purpose &&
                    otpVerification.OtpCode == code &&
                    otpVerification.ExpiresAtUtc > utcNow,
                cancellationToken);
    }

    public Task<OtpVerification?> GetActiveByUserAndCodeAsync(
        long userId,
        string purpose,
        string code,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return _dbContext.OtpVerifications
            .FirstOrDefaultAsync(
                otpVerification => !otpVerification.IsDeleted &&
                    !otpVerification.IsConsumed &&
                    otpVerification.UserId == userId &&
                    otpVerification.Purpose == purpose &&
                    otpVerification.OtpCode == code &&
                    otpVerification.ExpiresAtUtc > utcNow,
                cancellationToken);
    }
}
