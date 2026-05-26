using System.Text;
using Atya.Diagnostics.Metrics.Abstractions;
using Atya.Diagnostics.Metrics.Options;
using Atya.Diagnostics.OpenTelemetry.Internal;
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Diagnostics.Observation.Models;
using Atya.Diagnostics.Tracing.Abstractions;
using Atya.Diagnostics.Tracing.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

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
            options.Observation.ServiceName = "Orders.Service";
            options.Observation.ServiceVersion = "1.0.0";
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
            options.Observation.ServiceName = " Orders.Service ";
            options.Observation.ActivitySourceName = " Orders.Tracing ";
            options.Observation.MeterName = " Orders.Metrics ";
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

        _ = services.AddAtyaOpenTelemetry(options => options.Observation.ServiceName = "Orders.Service");

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<IActivitySourceAccessor>().Should().NotBeNull();
        _ = provider.GetService<IMeterAccessor>().Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Let_OpenTelemetry_Toggles_Override_Observation_Toggles()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.Observation.ServiceName = "Orders.Service";
            options.Observation.ConfigureTracing = false;
            options.EnableTracing = true;
        });

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<IActivitySourceAccessor>().Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Allow_Disabling_Tracing()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.Observation.ServiceName = "Orders.Service";
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
            options.Observation.ServiceName = "Orders.Service";
            options.EnableMetrics = false;
        });

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<IActivitySourceAccessor>().Should().NotBeNull();
        _ = provider.GetService<IMeterAccessor>().Should().BeNull();
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Register_OpenTelemetry_Logging_When_Enabled()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.Observation.ServiceName = "Orders.Service";
            options.EnableLogging = true;
        });

        using var provider = services.BuildServiceProvider();
        var resolvedOptions = provider.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;
        var loggerProviders = provider.GetServices<ILoggerProvider>();

        _ = resolvedOptions.EnableLogging.Should().BeTrue();
        _ = loggerProviders.Should().Contain(provider => provider is OpenTelemetryLoggerProvider);
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Not_Register_OpenTelemetry_Logging_By_Default()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options => options.Observation.ServiceName = "Orders.Service");

        using var provider = services.BuildServiceProvider();
        var loggerProviders = provider.GetServices<ILoggerProvider>();

        _ = loggerProviders.Should().NotContain(provider => provider is OpenTelemetryLoggerProvider);
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

        _ = services.AddAtyaOpenTelemetry(options => options.Observation.ServiceName = "Orders.Service");
        _ = services.AddAtyaOpenTelemetry(options => options.Observation.ServiceName = "Orders.Service");

        _ = services.Count(d => d.ServiceType == typeof(ObservationIdentity)).Should().Be(1);
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Register_Validator_Through_Enumerable()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidateOptions<OpenTelemetryOptions>, TestOpenTelemetryOptionsValidator>();

        _ = services.AddAtyaOpenTelemetry(options => options.Observation.ServiceName = "Orders.Service");

        using var provider = services.BuildServiceProvider();
        var validators = provider.GetServices<IValidateOptions<OpenTelemetryOptions>>().ToArray();

        _ = validators.Should().ContainSingle(validator => validator is TestOpenTelemetryOptionsValidator);
        _ = validators.Should().ContainSingle(validator => validator is OpenTelemetryOptionsValidator);
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Register_OpenTelemetryOptions()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(options =>
        {
            options.Observation.ServiceName = "Orders.Service";
            options.Observation.ServiceVersion = "2.0.0";
            options.EnableLogging = true;
            options.EnableObservationLogging = true;
            options.Logging.IncludeFormattedMessage = false;
            options.Logging.IncludeScopes = false;
            options.Logging.ParseStateValues = false;
            options.ActivitySources.Add("Orders.Workflows");
            options.Meters.Add("Orders.Business");
            options.Exporters.Console.Enabled = true;
            options.Exporters.Otlp.Enabled = true;
            options.Exporters.Otlp.Endpoint = "http://localhost:4317";
            options.Exporters.Otlp.Protocol = OtlpExportProtocol.Grpc;
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

        _ = resolvedOptions.Observation.ServiceName.Should().Be("Orders.Service");
        _ = resolvedOptions.Observation.ServiceVersion.Should().Be("2.0.0");
        _ = resolvedOptions.EnableLogging.Should().BeTrue();
        _ = resolvedOptions.EnableObservationLogging.Should().BeTrue();
        _ = resolvedOptions.Logging.IncludeFormattedMessage.Should().BeFalse();
        _ = resolvedOptions.Logging.IncludeScopes.Should().BeFalse();
        _ = resolvedOptions.Logging.ParseStateValues.Should().BeFalse();
        _ = resolvedOptions.ActivitySources.Should().ContainSingle("Orders.Workflows");
        _ = resolvedOptions.Meters.Should().ContainSingle("Orders.Business");
        _ = resolvedOptions.Exporters.Console.Enabled.Should().BeTrue();
        _ = resolvedOptions.Exporters.Otlp.Enabled.Should().BeTrue();
        _ = resolvedOptions.Exporters.Otlp.Endpoint.Should().Be("http://localhost:4317");
        _ = resolvedOptions.Exporters.Otlp.Protocol.Should().Be(OtlpExportProtocol.Grpc);
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
                ["OpenTelemetry:Observation:ServiceName"] = "Billing.Service",
                ["OpenTelemetry:Observation:ServiceVersion"] = "3.1.4",
                ["OpenTelemetry:EnableLogging"] = "true",
                ["OpenTelemetry:EnableObservationLogging"] = "true",
                ["OpenTelemetry:Logging:IncludeFormattedMessage"] = "false",
                ["OpenTelemetry:Logging:IncludeScopes"] = "false",
                ["OpenTelemetry:Logging:ParseStateValues"] = "false",
                ["OpenTelemetry:ActivitySources:0"] = "Billing.Workflows",
                ["OpenTelemetry:Meters:0"] = "Billing.Business",
                ["OpenTelemetry:Instrumentations:HttpClient:Enabled"] = "true",
                ["OpenTelemetry:Instrumentations:SqlClient:Enabled"] = "true",
                ["OpenTelemetry:Instrumentations:SqlClient:CaptureSqlText"] = "true",
                ["OpenTelemetry:Instrumentations:EntityFrameworkCore:Enabled"] = "true",
                ["OpenTelemetry:Instrumentations:EntityFrameworkCore:CaptureSqlText"] = "true",
                ["OpenTelemetry:Instrumentations:GrpcClient:Enabled"] = "true",
                ["OpenTelemetry:Exporters:Console:Enabled"] = "true",
                ["OpenTelemetry:Exporters:Otlp:Enabled"] = "true",
                ["OpenTelemetry:Exporters:Otlp:Endpoint"] = "http://collector:4317",
                ["OpenTelemetry:Exporters:Otlp:Protocol"] = "HttpProtobuf",
                ["OpenTelemetry:Exporters:Otlp:Headers:tenant"] = "billing",
            })
            .Build();
        var services = new ServiceCollection();

        _ = services.AddAtyaOpenTelemetry(configuration);

        using var provider = services.BuildServiceProvider();
        var resolvedOptions = provider.GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;

        _ = resolvedOptions.Observation.ServiceName.Should().Be("Billing.Service");
        _ = resolvedOptions.Observation.ServiceVersion.Should().Be("3.1.4");
        _ = resolvedOptions.EnableLogging.Should().BeTrue();
        _ = resolvedOptions.EnableObservationLogging.Should().BeTrue();
        _ = resolvedOptions.Logging.IncludeFormattedMessage.Should().BeFalse();
        _ = resolvedOptions.Logging.IncludeScopes.Should().BeFalse();
        _ = resolvedOptions.Logging.ParseStateValues.Should().BeFalse();
        _ = resolvedOptions.ActivitySources.Should().ContainSingle("Billing.Workflows");
        _ = resolvedOptions.Meters.Should().ContainSingle("Billing.Business");
        _ = resolvedOptions.Instrumentations.HttpClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.SqlClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.SqlClient.CaptureSqlText.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.EntityFrameworkCore.Enabled.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.EntityFrameworkCore.CaptureSqlText.Should().BeTrue();
        _ = resolvedOptions.Instrumentations.GrpcClient.Enabled.Should().BeTrue();
        _ = resolvedOptions.Exporters.Console.Enabled.Should().BeTrue();
        _ = resolvedOptions.Exporters.Otlp.Enabled.Should().BeTrue();
        _ = resolvedOptions.Exporters.Otlp.Endpoint.Should().Be("http://collector:4317");
        _ = resolvedOptions.Exporters.Otlp.Protocol.Should().Be(OtlpExportProtocol.HttpProtobuf);
        _ = resolvedOptions.Exporters.Otlp.Headers.Should().Contain("tenant", "billing");
    }

    [Fact]
    public void OpenTelemetryOptions_Should_Bind_Nested_Observation_From_Json()
    {
        const string Json = """
            {
              "OpenTelemetry": {
                "Observation": {
                  "ServiceName": "Catalog.Service"
                }
              }
            }
            """;

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Json));
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();
        var options = new OpenTelemetryOptions();

        configuration.GetSection("OpenTelemetry").Bind(options);

        _ = options.Observation.ServiceName.Should().Be("Catalog.Service");
    }

    [Fact]
    public void AddAtyaOpenTelemetry_Should_Bind_Options_From_Custom_Configuration_Section()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Diagnostics:Observation:ServiceName"] = "Shipping.Service",
                ["Diagnostics:Observation:ActivitySourceName"] = "Shipping.Tracing",
                ["Diagnostics:Observation:MeterName"] = "Shipping.Metrics",
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

        _ = options.Observation.ServiceName.Should().BeEmpty();
        _ = options.Observation.ServiceVersion.Should().BeNull();
        _ = options.Observation.ActivitySourceName.Should().BeNull();
        _ = options.Observation.MeterName.Should().BeNull();
        _ = options.ActivitySources.Should().BeEmpty();
        _ = options.Meters.Should().BeEmpty();
        _ = options.EnableLogging.Should().BeFalse();
        _ = options.EnableTracing.Should().BeTrue();
        _ = options.EnableMetrics.Should().BeTrue();
        _ = options.EnableObservationLogging.Should().BeFalse();
        _ = options.Logging.IncludeFormattedMessage.Should().BeTrue();
        _ = options.Logging.IncludeScopes.Should().BeTrue();
        _ = options.Logging.ParseStateValues.Should().BeTrue();
        _ = options.Instrumentations.AspNetCore.Enabled.Should().BeFalse();
        _ = options.Instrumentations.HttpClient.Enabled.Should().BeFalse();
        _ = options.Instrumentations.SqlClient.Enabled.Should().BeFalse();
        _ = options.Instrumentations.SqlClient.CaptureSqlText.Should().BeFalse();
        _ = options.Instrumentations.EntityFrameworkCore.Enabled.Should().BeFalse();
        _ = options.Instrumentations.EntityFrameworkCore.CaptureSqlText.Should().BeFalse();
        _ = options.Instrumentations.GrpcClient.Enabled.Should().BeFalse();
        _ = options.Instrumentations.Runtime.Enabled.Should().BeFalse();
        _ = options.Exporters.Console.Enabled.Should().BeFalse();
        _ = options.Exporters.Otlp.Enabled.Should().BeFalse();
        _ = options.Exporters.Otlp.Endpoint.Should().BeNull();
        _ = options.Exporters.Otlp.Protocol.Should().BeNull();
        _ = options.Exporters.Otlp.Headers.Should().BeEmpty();
        _ = options.Resource.ServiceNamespace.Should().BeNull();
        _ = options.Resource.ServiceInstanceId.Should().BeNull();
        _ = options.Resource.DeploymentEnvironment.Should().BeNull();
        _ = options.Resource.Attributes.Should().BeEmpty();
    }

    private sealed class TestOpenTelemetryOptionsValidator : IValidateOptions<OpenTelemetryOptions>
    {
        public ValidateOptionsResult Validate(string? name, OpenTelemetryOptions options)
        {
            _ = name;
            _ = options;
            return ValidateOptionsResult.Success;
        }
    }
}
