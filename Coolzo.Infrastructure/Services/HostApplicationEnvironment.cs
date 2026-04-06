using Coolzo.Application.Common.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Coolzo.Infrastructure.Services;

public sealed class HostApplicationEnvironment : IApplicationEnvironment
{
    private readonly IHostEnvironment _hostEnvironment;

    public HostApplicationEnvironment(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public bool IsProduction()
    {
        return _hostEnvironment.IsProduction();
    }
}
