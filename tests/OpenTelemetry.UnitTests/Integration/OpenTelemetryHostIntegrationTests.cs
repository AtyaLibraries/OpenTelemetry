using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenTelemetry.UnitTests.Integration;

public sealed class OpenTelemetryHostIntegrationTests
{
    private static readonly Action<ILogger, Exception?> OpenTelemetryLoggingPipelineStarted =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(1000, nameof(OpenTelemetryLoggingPipelineStarted)),
            "OpenTelemetry logging pipeline started.");

    [Fact]
    public async Task AddAtyaOpenTelemetry_Should_Start_And_Stop_OpenTelemetry_Hosted_Service()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.Logging.ClearProviders();
        _ = builder.Services.AddAtyaOpenTelemetry(options =>
        {
            options.Observation.ServiceName = "Orders.Service";
            options.Observation.ServiceVersion = "1.0.0";
            options.Resource.ServiceNamespace = "orders";
            options.Resource.DeploymentEnvironment = "test";
            options.Resource.Attributes["team"] = "platform";
            options.EnableLogging = true;
            options.Instrumentations.HttpClient.Enabled = true;
            options.Instrumentations.Runtime.Enabled = true;
        });

        using var host = builder.Build();

        await host.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            var hostedServices = host.Services.GetServices<IHostedService>().ToArray();
            var logger = host.Services.GetRequiredService<ILogger<OpenTelemetryHostIntegrationTests>>();

            OpenTelemetryLoggingPipelineStarted(logger, null);

            _ = hostedServices.Should().NotBeEmpty();
        }
        finally
        {
            await host.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Reject_Missing_ServiceName_Before_Host_Build()
    {
        var builder = Host.CreateApplicationBuilder();
        _ = builder.Logging.ClearProviders();

        var act = () => builder.Services.AddAtyaOpenTelemetry();

        _ = act.Should().Throw<OptionsValidationException>()
            .WithMessage("*ServiceName*");
    }
}
