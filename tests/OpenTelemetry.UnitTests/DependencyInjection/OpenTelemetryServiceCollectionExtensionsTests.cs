using Atya.Diagnostics.Metrics.Abstractions;
using Atya.Diagnostics.Metrics.Options;
using Atya.Diagnostics.Observation.Models;
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Diagnostics.Tracing.Abstractions;
using Atya.Diagnostics.Tracing.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OpenTelemetry.UnitTests.DependencyInjection;

public sealed class OpenTelemetryServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAtyaOpenTelemetry_Should_Throw_When_Services_Is_Null()
    {
        var act = () => OpenTelemetryServiceCollectionExtensions.AddAtyaOpenTelemetry(null!);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("services");
    }

    [Fact]
    public void AddAtyaOpenTelemetry_WithConfiguration_Should_Throw_When_Configuration_Is_Null()
    {
        var services = new ServiceCollection();

        var act = () => services.AddAtyaOpenTelemetry(configuration: null!);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configuration");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddAtyaOpenTelemetry_WithConfiguration_Should_Throw_When_SectionName_Is_Invalid(string? sectionName)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var act = () => services.AddAtyaOpenTelemetry(configuration, sectionName!);

        _ = act.Should().Throw<ArgumentException>()
            .And.ParamName.Should().Be(nameof(sectionName));
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Register_ObservationIdentity_And_Map_Default_Names()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.ServiceName = "Orders.Service";
            options.ServiceVersion = "1.0.0";
        });

        using var provider = services.BuildServiceProvider();

        var identity = provider.GetRequiredService<ObservationIdentity>();
        var tracingOptions = provider.GetRequiredService<IOptions<TracingOptions>>().Value;
        var metricsOptions = provider.GetRequiredService<IOptions<MetricsOptions>>().Value;

        _ = identity.ServiceName.Should().Be("Orders.Service");
        _ = identity.ActivitySourceName.Should().Be("Orders.Service");
        _ = identity.MeterName.Should().Be("Orders.Service");
        _ = identity.ServiceVersion.Should().Be("1.0.0");
        _ = tracingOptions.ActivitySourceName.Should().Be("Orders.Service");
        _ = tracingOptions.ActivitySourceVersion.Should().Be("1.0.0");
        _ = metricsOptions.MeterName.Should().Be("Orders.Service");
        _ = metricsOptions.MeterVersion.Should().Be("1.0.0");
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Trim_Service_And_Use_Explicit_ActivitySource_And_Meter_Names()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.ServiceName = " Orders.Service ";
            options.ActivitySourceName = " Orders.Tracing ";
            options.MeterName = " Orders.Metrics ";
        });

        using var provider = services.BuildServiceProvider();

        var identity = provider.GetRequiredService<ObservationIdentity>();
        _ = identity.ServiceName.Should().Be("Orders.Service");
        _ = identity.ActivitySourceName.Should().Be("Orders.Tracing");
        _ = identity.MeterName.Should().Be("Orders.Metrics");
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Register_Tracing_And_Metrics_By_Default()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options => options.ServiceName = "Orders.Service");

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<IActivitySourceAccessor>().Should().NotBeNull();
        _ = provider.GetService<IMeterAccessor>().Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Allow_Disabling_Tracing()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.ServiceName = "Orders.Service";
            options.EnableTracing = false;
        });

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<IActivitySourceAccessor>().Should().BeNull();
        _ = provider.GetService<IMeterAccessor>().Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Allow_Disabling_Metrics()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.ServiceName = "Orders.Service";
            options.EnableMetrics = false;
        });

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<IActivitySourceAccessor>().Should().NotBeNull();
        _ = provider.GetService<IMeterAccessor>().Should().BeNull();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Register_Logging_When_Enabled()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.ServiceName = "Orders.Service";
            options.EnableObservationLogging = true;
        });

        using var provider = services.BuildServiceProvider();
        var resolvedOptions = provider.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;

        _ = resolvedOptions.EnableObservationLogging.Should().BeTrue();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Throw_When_ServiceName_Is_Missing()
    {
        var services = new ServiceCollection();

        var act = () => services.AddAtyaOpenTelemetry();

        _ = act.Should().Throw<OptionsValidationException>()
            .WithMessage("*ServiceName*");
        _ = services.Should().BeEmpty();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Be_Idempotent_For_Core_Identity_Service()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options => options.ServiceName = "Orders.Service");
        _ = services.AddAtyaOpenTelemetry(options => options.ServiceName = "Orders.Service");

        _ = services.Count(d => d.ServiceType == typeof(ObservationIdentity)).Should().Be(1);
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Register_OpenTelemetryOptions()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.ServiceName = "Orders.Service";
            options.ServiceVersion = "2.0.0";
            options.EnableObservationLogging = true;
            options.ActivitySources.Add("Orders.Workflows");
            options.Meters.Add("Orders.Business");
            options.Exporters.Otlp.Enabled = true;
            options.Exporters.Otlp.Endpoint = "http://localhost:4317";
            options.Exporters.Otlp.Protocol = "grpc";
            options.Exporters.Otlp.Headers["authorization"] = "Bearer token";
            options.Instrumentations.AspNetCore.Enabled = true;
            options.Instrumentations.HttpClient.Enabled = true;
            options.Instrumentations.SqlClient.Enabled = true;
            options.Instrumentations.SqlClient.CaptureSqlText = true;
            options.Instrumentations.EntityFrameworkCore.Enabled = true;
            options.Instrumentations.EntityFrameworkCore.CaptureSqlText = true;
            options.Instrumentations.GrpcClient.Enabled = true;
            options.Instrumentations.Runtime.Enabled = true;
        });

        using var provider = services.BuildServiceProvider();

        var resolvedOptions = provider.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;

        _ = resolvedOptions.ServiceName.Should().Be("Orders.Service");
        _ = resolvedOptions.ServiceVersion.Should().Be("2.0.0");
        _ = resolvedOptions.EnableObservationLogging.Should().BeTrue();
        _ = resolvedOptions.ActivitySources.Should().ContainSingle("Orders.Workflows");
        _ = resolvedOptions.Meters.Should().ContainSingle("Orders.Business");
        _ = resolvedOptions.Exporters.Otlp.Enabled.Should().BeTrue();
        _ = resolvedOptions.Exporters.Otlp.Endpoint.Should().Be("http://localhost:4317");
        _ = resolvedOptions.Exporters.Otlp.Protocol.Should().Be("grpc");
        _ = resolvedOptions.Exporters.Otlp.Headers.Should().ContainKey("authorization");
        _ = resolvedOptions.Instrumentations.AspNetCore.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.HttpClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.SqlClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.SqlClient.CaptureSqlText.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.EntityFrameworkCore.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.EntityFrameworkCore.CaptureSqlText.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.GrpcClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.Runtime.Enabled.Should().BeTrue();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Bind_Options_From_Default_Configuration_Section()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenTelemetry:ServiceName"] = "Billing.Service",
                ["OpenTelemetry:ServiceVersion"] = "3.1.4",
                ["OpenTelemetry:EnableObservationLogging"] = "true",
                ["OpenTelemetry:ActivitySources:0"] = "Billing.Workflows",
                ["OpenTelemetry:Meters:0"] = "Billing.Business",
                ["OpenTelemetry:Instrumentations:HttpClient:Enabled"] = "true",
                ["OpenTelemetry:Instrumentations:SqlClient:Enabled"] = "true",
                ["OpenTelemetry:Instrumentations:SqlClient:CaptureSqlText"] = "true",
                ["OpenTelemetry:Instrumentations:EntityFrameworkCore:Enabled"] = "true",
                ["OpenTelemetry:Instrumentations:EntityFrameworkCore:CaptureSqlText"] = "true",
                ["OpenTelemetry:Instrumentations:GrpcClient:Enabled"] = "true",
                ["OpenTelemetry:Exporters:Otlp:Enabled"] = "true",
                ["OpenTelemetry:Exporters:Otlp:Endpoint"] = "http://collector:4317",
                ["OpenTelemetry:Exporters:Otlp:Protocol"] = "http/protobuf",
                ["OpenTelemetry:Exporters:Otlp:Headers:tenant"] = "billing",
            })
            .Build();
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(configuration);

        using var provider = services.BuildServiceProvider();
        var resolvedOptions = provider.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;

        _ = resolvedOptions.ServiceName.Should().Be("Billing.Service");
        _ = resolvedOptions.ServiceVersion.Should().Be("3.1.4");
        _ = resolvedOptions.EnableObservationLogging.Should().BeTrue();
        _ = resolvedOptions.ActivitySources.Should().ContainSingle("Billing.Workflows");
        _ = resolvedOptions.Meters.Should().ContainSingle("Billing.Business");
        _ = resolvedOptions.Instrumentations.HttpClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.SqlClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.SqlClient.CaptureSqlText.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.EntityFrameworkCore.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.EntityFrameworkCore.CaptureSqlText.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.GrpcClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Exporters.Otlp.Enabled.Should().BeTrue();
        _ = resolvedOptions.Exporters.Otlp.Endpoint.Should().Be("http://collector:4317");
        _ = resolvedOptions.Exporters.Otlp.Protocol.Should().Be("http/protobuf");
        _ = resolvedOptions.Exporters.Otlp.Headers.Should().Contain("tenant", "billing");
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Bind_Options_From_Custom_Configuration_Section()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Diagnostics:ServiceName"] = "Shipping.Service",
                ["Diagnostics:ActivitySourceName"] = "Shipping.Tracing",
                ["Diagnostics:MeterName"] = "Shipping.Metrics",
            })
            .Build();
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(configuration, " Diagnostics ");

        using var provider = services.BuildServiceProvider();
        var identity = provider.GetRequiredService<ObservationIdentity>();

        _ = identity.ServiceName.Should().Be("Shipping.Service");
        _ = identity.ActivitySourceName.Should().Be("Shipping.Tracing");
        _ = identity.MeterName.Should().Be("Shipping.Metrics");
    }

    [Fact]
    public void OpenTelemetryOptions_Defaults_Should_Be_Production_Safe()
    {
        var options = new OpenTelemetryOptions();

        _ = options.ServiceName.Should().BeEmpty();
        _ = options.ServiceVersion.Should().BeNull();
        _ = options.ActivitySourceName.Should().BeNull();
        _ = options.MeterName.Should().BeNull();
        _ = options.ActivitySources.Should().BeEmpty();
        _ = options.Meters.Should().BeEmpty();
        _ = options.EnableTracing.Should().BeTrue();
        _ = options.EnableMetrics.Should().BeTrue();
        _ = options.EnableObservationLogging.Should().BeFalse();
        _ = options.Instrumentations.AspNetCore.Enabled.Should().BeFalse();
        _ = options.Instrumentations.HttpClient.Enabled.Should().BeFalse();
        _ = options.Instrumentations.SqlClient.Enabled.Should().BeFalse();
        _ = options.Instrumentations.SqlClient.CaptureSqlText.Should().BeFalse();
        _ = options.Instrumentations.EntityFrameworkCore.Enabled.Should().BeFalse();
        _ = options.Instrumentations.EntityFrameworkCore.CaptureSqlText.Should().BeFalse();
        _ = options.Instrumentations.GrpcClient.Enabled.Should().BeFalse();
        _ = options.Instrumentations.Runtime.Enabled.Should().BeFalse();
        _ = options.Exporters.Otlp.Enabled.Should().BeFalse();
        _ = options.Exporters.Otlp.Endpoint.Should().BeNull();
        _ = options.Exporters.Otlp.Protocol.Should().BeNull();
        _ = options.Exporters.Otlp.Headers.Should().BeEmpty();
        _ = options.Resource.ServiceNamespace.Should().BeNull();
        _ = options.Resource.ServiceInstanceId.Should().BeNull();
        _ = options.Resource.DeploymentEnvironment.Should().BeNull();
        _ = options.Resource.Attributes.Should().BeEmpty();
    }
}
