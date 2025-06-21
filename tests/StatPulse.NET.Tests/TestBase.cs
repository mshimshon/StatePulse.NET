using Microsoft.Extensions.DependencyInjection;
using StatePulse.Net;

namespace StatePulse.NET.Tests;
public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;

    protected TestBase()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddStatePulseServices(o =>
        {
            o.ScanAssemblies = new Type[] {
                typeof(TestBase)
            };

        });

        // Register your services
        ServiceProvider = services.BuildServiceProvider();
    }
    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}