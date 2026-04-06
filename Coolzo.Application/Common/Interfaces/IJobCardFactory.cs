using Coolzo.Domain.Entities;

namespace Coolzo.Application.Common.Interfaces;

public interface IJobCardFactory
{
    JobCard EnsureCreated(ServiceRequest serviceRequest);
}
