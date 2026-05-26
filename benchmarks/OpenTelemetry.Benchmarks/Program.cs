using Atya.Diagnostics.OpenTelemetry.Options;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTelemetry.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

[MemoryDiagnoser]
[RankColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class OpenTelemetryRegistrationBenchmarks
{
    private const string ServiceName = "Benchmarks.Orders.Service";
    private static readonly Action<OpenTelemetryOptions> MinimalConfiguration = ConfigureMinimal;
    private static readonly Action<OpenTelemetryOptions> FullConfiguration = ConfigureFull;

    [Benchmark(Baseline = true)]
    public static int RegisterOptionsOnlyBaseline()
    {
        var services = new ServiceCollection();
        _ = services.AddOptions<OpenTelemetryOptions>()
            .Configure(MinimalConfiguration);

        return services.Count;
    }

    [Benchmark]
    public static int RegisterMinimalOpenTelemetry()
    {
        var services = new ServiceCollection();
        _ = services.AddAtyaOpenTelemetry(MinimalConfiguration);

        return services.Count;
    }

    [Benchmark]
    public static int RegisterFullOpenTelemetry()
    {
        var services = new ServiceCollection();
        _ = services.AddAtyaOpenTelemetry(FullConfiguration);

        return services.Count;
    }

    [Benchmark]
    public static int BuildMinimalServiceProvider()
    {
        var services = new ServiceCollection();
        _ = services.AddAtyaOpenTelemetry(MinimalConfiguration);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
        });

        return services.Count + (provider.GetService<IServiceProvider>() is null ? 0 : 1);
    }

    private static void ConfigureMinimal(OpenTelemetryOptions options)
    {
        options.Observation.ServiceName = ServiceName;
        options.EnableObservationLogging = false;
        options.EnableTracing = true;
        options.EnableMetrics = true;
    }

    private static void ConfigureFull(OpenTelemetryOptions options)
    {
        ConfigureMinimal(options);
        options.Observation.ServiceVersion = "1.0.0";
        options.Observation.ActivitySourceName = "Benchmarks.Orders.Tracing";
        options.Observation.MeterName = "Benchmarks.Orders.Metrics";
        options.Resource.ServiceNamespace = "benchmarks";
        options.Resource.DeploymentEnvironment = "production";
        options.Resource.Attributes["team"] = "platform";
        options.Instrumentations.AspNetCore.Enabled = true;
        options.Instrumentations.HttpClient.Enabled = true;
        options.Instrumentations.Runtime.Enabled = true;
        options.Exporters.Otlp.Enabled = true;
        options.Exporters.Otlp.Endpoint = "http://localhost:4317";
        options.Exporters.Otlp.Protocol = "grpc";
        options.Exporters.Otlp.Headers["x-benchmark"] = "opentelemetry";
    }
}
