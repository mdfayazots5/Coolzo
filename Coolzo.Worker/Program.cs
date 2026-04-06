using Coolzo.Application.DependencyInjection;
using Coolzo.Infrastructure.DependencyInjection;
using Coolzo.Persistence.DependencyInjection;
using Coolzo.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
