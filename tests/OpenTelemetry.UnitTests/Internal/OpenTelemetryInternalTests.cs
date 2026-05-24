using System.Data;
using System.Diagnostics;
using Atya.Diagnostics.OpenTelemetry.Internal;
using Atya.Diagnostics.OpenTelemetry.Metrics;
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Diagnostics.OpenTelemetry.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using AtyaTracerProviderBuilderExtensions = Atya.Diagnostics.OpenTelemetry.Tracing.TracerProviderBuilderExtensions;

namespace OpenTelemetry.UnitTests.Internal;

public sealed class OpenTelemetryInternalTests
{
    [Fact]
    public void OtlpExporterConfigurator_Should_Apply_Endpoint_Grpc_And_Headers()
    {
        var otlp = new OtlpExporterOptions();
        var options = new OtlpOptions
        {
            Endpoint = "http://localhost:4317",
            Protocol = OtlpExportProtocol.Grpc,
        };
        options.Headers["authorization"] = "Bearer token";
        options.Headers["tenant"] = "orders";

        OtlpExporterConfigurator.Apply(otlp, options);

        _ = otlp.Endpoint.Should().Be(new Uri("http://localhost:4317"));
        _ = otlp.Protocol.Should().Be(OtlpExportProtocol.Grpc);
        _ = otlp.Headers.Should().Be("authorization=Bearer token,tenant=orders");
    }

    [Fact]
    public void OtlpExporterConfigurator_Should_Apply_HttpProtobuf_Protocol()
    {
        var otlp = new OtlpExporterOptions();
        var options = new OtlpOptions
        {
            Protocol = OtlpExportProtocol.HttpProtobuf,
        };

        OtlpExporterConfigurator.Apply(otlp, options);

        _ = otlp.Protocol.Should().Be(OtlpExportProtocol.HttpProtobuf);
    }

    [Fact]
    public void OtlpExporterConfigurator_Should_Leave_Defaults_When_Optional_Values_Are_Missing()
    {
        var otlp = new OtlpExporterOptions();
        var expectedEndpoint = otlp.Endpoint;
        var expectedProtocol = otlp.Protocol;
        var expectedHeaders = otlp.Headers;

        OtlpExporterConfigurator.Apply(otlp, new OtlpOptions());

        _ = otlp.Endpoint.Should().Be(expectedEndpoint);
        _ = otlp.Protocol.Should().Be(expectedProtocol);
        _ = otlp.Headers.Should().Be(expectedHeaders);
    }

    [Fact]
    public void OtlpExporterConfigurator_Should_Throw_For_Undefined_Protocol()
    {
        var otlp = new OtlpExporterOptions();
        var options = new OtlpOptions
        {
            Protocol = (OtlpExportProtocol)42,
        };

        var act = () => OtlpExporterConfigurator.Apply(otlp, options);

        _ = act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("options");
    }

    [Fact]
    public void OtlpExporterConfigurator_Should_Throw_When_Arguments_Are_Null()
    {
        var actForNullOptions = () => OtlpExporterConfigurator.Apply(new OtlpExporterOptions(), null!);
        var actForNullOtlp = () => OtlpExporterConfigurator.Apply(null!, new OtlpOptions());

        _ = actForNullOptions.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
        _ = actForNullOtlp.Should().Throw<ArgumentNullException>()
            .WithParameterName("otlp");
    }

    [Fact]
    public void OpenTelemetryOptionsValidator_Should_Return_Success_For_Valid_Options()
    {
        var options = CreateValidOptions();
        options.Exporters.Otlp.Enabled = true;
        options.Exporters.Otlp.Endpoint = "http://localhost:4317";
        options.Exporters.Otlp.Protocol = OtlpExportProtocol.Grpc;
        options.Exporters.Otlp.Headers["authorization"] = "Bearer token";

        var result = new OpenTelemetryOptionsValidator().Validate(null, options);

        _ = result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void OpenTelemetryOptionsValidator_Should_Ignore_Disabled_Otlp_Details()
    {
        var options = CreateValidOptions();
        options.Exporters.Otlp.Endpoint = "not a uri";
        options.Exporters.Otlp.Protocol = (OtlpExportProtocol)42;
        options.Exporters.Otlp.Headers["bad,key"] = "bad,value";

        var result = new OpenTelemetryOptionsValidator().Validate(null, options);

        _ = result.Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void OpenTelemetryOptionsValidator_Should_Fail_When_ServiceName_Is_Missing(string? serviceName)
    {
        var options = CreateValidOptions();
        options.ServiceName = serviceName!;

        var result = new OpenTelemetryOptionsValidator().Validate("named", options);

        _ = result.Failed.Should().BeTrue();
        _ = result.FailureMessage.Should().Contain("ServiceName");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OpenTelemetryOptionsValidator_Should_Fail_When_Additional_Telemetry_Names_Are_Invalid(bool invalidActivitySource)
    {
        var options = CreateValidOptions();
        if (invalidActivitySource)
        {
            options.ActivitySources.Add(" ");
        }
        else
        {
            options.ActivitySources.Add("Orders.Workflows");
            options.Meters.Add("");
        }

        var result = new OpenTelemetryOptionsValidator().Validate(null, options);

        _ = result.Failed.Should().BeTrue();
        _ = result.FailureMessage.Should().Contain(invalidActivitySource ? "ActivitySources" : "Meters");
    }

    [Fact]
    public void OpenTelemetryOptionsValidator_Should_Fail_When_Enabled_Otlp_Endpoint_Is_Invalid()
    {
        var options = CreateValidOptions();
        options.Exporters.Otlp.Enabled = true;
        options.Exporters.Otlp.Endpoint = "not a uri";

        var result = new OpenTelemetryOptionsValidator().Validate(null, options);

        _ = result.Failed.Should().BeTrue();
        _ = result.FailureMessage.Should().Contain("Endpoint");
    }

    [Fact]
    public void OpenTelemetryOptionsValidator_Should_Fail_When_Enabled_Otlp_Protocol_Is_Invalid()
    {
        var options = CreateValidOptions();
        options.Exporters.Otlp.Enabled = true;
        options.Exporters.Otlp.Protocol = (OtlpExportProtocol)42;

        var result = new OpenTelemetryOptionsValidator().Validate(null, options);

        _ = result.Failed.Should().BeTrue();
        _ = result.FailureMessage.Should().Contain("Protocol");
    }

    [Theory]
    [InlineData(" ", "value", "header name")]
    [InlineData("bad,key", "value", "header names")]
    [InlineData("bad=key", "value", "header names")]
    [InlineData("name", null, "header value")]
    [InlineData("name", "bad,value", "header values")]
    public void OpenTelemetryOptionsValidator_Should_Fail_When_Enabled_Otlp_Header_Is_Invalid(
        string key,
        string? value,
        string expectedMessage)
    {
        var options = CreateValidOptions();
        options.Exporters.Otlp.Enabled = true;
        options.Exporters.Otlp.Headers[key] = value!;

        var result = new OpenTelemetryOptionsValidator().Validate(null, options);

        _ = result.Failed.Should().BeTrue();
        _ = result.FailureMessage.Should().Contain(expectedMessage);
    }

    [Fact]
    public void OpenTelemetryOptionsValidator_Should_Throw_When_Options_Are_Null()
    {
        var act = () => new OpenTelemetryOptionsValidator().Validate(null, null!);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void ResourceBuilderFactory_Should_Create_Resource_With_Service_Metadata_And_Custom_Attributes()
    {
        var options = CreateValidOptions();
        options.ServiceName = " Orders.Service ";
        options.ServiceVersion = " 1.2.3 ";
        options.Resource.ServiceNamespace = "orders";
        options.Resource.ServiceInstanceId = "pod-123";
        options.Resource.DeploymentEnvironment = "production";
        options.Resource.Attributes["team"] = "platform";

        var resource = ResourceBuilderFactory.Create(options, "Orders.Tracing", "Orders.Metrics").Build();
        var attributes = resource.Attributes.ToDictionary(attribute => attribute.Key, attribute => attribute.Value);

        _ = attributes.Should().Contain("service.name", "Orders.Service");
        _ = attributes.Should().Contain("service.version", "1.2.3");
        _ = attributes.Should().Contain("service.namespace", "orders");
        _ = attributes.Should().Contain("service.instance.id", "pod-123");
        _ = attributes.Should().Contain("deployment.environment", "production");
        _ = attributes.Should().Contain("team", "platform");
    }

    [Fact]
    public void ResourceBuilderFactory_Should_Create_Minimal_Resource()
    {
        var resource = ResourceBuilderFactory.Create(CreateValidOptions(), "Orders.Tracing", "Orders.Metrics").Build();
        var attributes = resource.Attributes.ToDictionary(attribute => attribute.Key, attribute => attribute.Value);

        _ = attributes.Should().Contain("service.name", "Orders.Service");
        _ = attributes.Should().NotContainKey("deployment.environment");
    }

    [Fact]
    public void ResourceBuilderFactory_Should_Throw_When_Arguments_Are_Invalid()
    {
        var validOptions = CreateValidOptions();
        var actForNullOptions = () => ResourceBuilderFactory.Create(null!, "source", "meter");
        var actForInvalidSource = () => ResourceBuilderFactory.Create(validOptions, " ", "meter");
        var actForInvalidMeter = () => ResourceBuilderFactory.Create(validOptions, "source", "");

        _ = actForNullOptions.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
        _ = actForInvalidSource.Should().Throw<ArgumentException>()
            .WithParameterName("activitySourceName");
        _ = actForInvalidMeter.Should().Throw<ArgumentException>()
            .WithParameterName("meterName");
    }

    [Fact]
    public void TelemetryNameNormalizer_Should_Trim_Deduplicate_And_Ignore_Missing_Names()
    {
        var names = new string?[]
        {
            " Orders.Workflows ",
            "",
            "Orders.Service",
            "Orders.Workflows",
            null,
            "   ",
            "Orders.Integration",
        };

        var result = TelemetryNameNormalizer.Normalize(" Orders.Service ", names);

        _ = result.Should().Equal("Orders.Service", "Orders.Workflows", "Orders.Integration");
    }

    [Fact]
    public void TelemetryNameNormalizer_Should_Throw_When_Arguments_Are_Invalid()
    {
        var actForInvalidPrimaryName = () => TelemetryNameNormalizer.Normalize(" ", []);
        var actForNullNames = () => TelemetryNameNormalizer.Normalize("Orders.Service", null!);

        _ = actForInvalidPrimaryName.Should().Throw<ArgumentException>()
            .WithParameterName("primaryName");
        _ = actForNullNames.Should().Throw<ArgumentNullException>()
            .WithParameterName("additionalNames");
    }

    [Fact]
    public void TracerProviderBuilderExtensions_Should_Configure_All_Tracing_Branches()
    {
        var options = CreateFullOptions();
        var resourceBuilder = ResourceBuilderFactory.Create(options, "Orders.Tracing", "Orders.Metrics");
        var builder = Sdk.CreateTracerProviderBuilder();

        var result = builder.ConfigureAtyaTracing(options, resourceBuilder, "Orders.Tracing");
        using var provider = result.Build();

        _ = result.Should().BeSameAs(builder);
        _ = provider.Should().NotBeNull();
    }

    [Fact]
    public void TracerProviderBuilderExtensions_Should_Configure_Minimal_Tracing_Branches()
    {
        var options = CreateValidOptions();
        var resourceBuilder = ResourceBuilderFactory.Create(options, "Orders.Tracing", "Orders.Metrics");
        var builder = Sdk.CreateTracerProviderBuilder();

        var result = builder.ConfigureAtyaTracing(options, resourceBuilder, "Orders.Tracing");

        _ = result.Should().BeSameAs(builder);
    }

    [Fact]
    public void TracerProviderBuilderExtensions_Should_Configure_Database_Instrumentation_Without_Query_Text()
    {
        var options = CreateValidOptions();
        options.Instrumentations.SqlClient.Enabled = true;
        options.Instrumentations.EntityFrameworkCore.Enabled = true;
        var resourceBuilder = ResourceBuilderFactory.Create(options, "Orders.Tracing", "Orders.Metrics");
        var builder = Sdk.CreateTracerProviderBuilder();

        var result = builder.ConfigureAtyaTracing(options, resourceBuilder, "Orders.Tracing");
        using var provider = result.Build();

        _ = result.Should().BeSameAs(builder);
        _ = provider.Should().NotBeNull();
    }

    [Fact]
    public void TracerProviderBuilderExtensions_Should_Throw_When_Arguments_Are_Invalid()
    {
        var options = CreateValidOptions();
        var resourceBuilder = ResourceBuilderFactory.Create(options, "Orders.Tracing", "Orders.Metrics");
        var builder = Sdk.CreateTracerProviderBuilder();

        var actForNullBuilder = () => AtyaTracerProviderBuilderExtensions.ConfigureAtyaTracing(null!, options, resourceBuilder, "source");
        var actForNullOptions = () => builder.ConfigureAtyaTracing(null!, resourceBuilder, "source");
        var actForNullResource = () => builder.ConfigureAtyaTracing(options, null!, "source");
        var actForInvalidSource = () => builder.ConfigureAtyaTracing(options, resourceBuilder, " ");

        _ = actForNullBuilder.Should().Throw<ArgumentNullException>()
            .WithParameterName("builder");
        _ = actForNullOptions.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
        _ = actForNullResource.Should().Throw<ArgumentNullException>()
            .WithParameterName("resourceBuilder");
        _ = actForInvalidSource.Should().Throw<ArgumentException>()
            .WithParameterName("activitySourceName");
    }

    [Fact]
    public void MeterProviderBuilderExtensions_Should_Configure_All_Metrics_Branches()
    {
        var options = CreateFullOptions();
        var resourceBuilder = ResourceBuilderFactory.Create(options, "Orders.Tracing", "Orders.Metrics");
        var builder = Sdk.CreateMeterProviderBuilder();

        var result = builder.ConfigureAtyaMetrics(options, resourceBuilder, "Orders.Metrics");

        _ = result.Should().BeSameAs(builder);
    }

    [Fact]
    public void MeterProviderBuilderExtensions_Should_Configure_Minimal_Metrics_Branches()
    {
        var options = CreateValidOptions();
        var resourceBuilder = ResourceBuilderFactory.Create(options, "Orders.Tracing", "Orders.Metrics");
        var builder = Sdk.CreateMeterProviderBuilder();

        var result = builder.ConfigureAtyaMetrics(options, resourceBuilder, "Orders.Metrics");

        _ = result.Should().BeSameAs(builder);
    }

    [Fact]
    public void MeterProviderBuilderExtensions_Should_Throw_When_Arguments_Are_Invalid()
    {
        var options = CreateValidOptions();
        var resourceBuilder = ResourceBuilderFactory.Create(options, "Orders.Tracing", "Orders.Metrics");
        var builder = Sdk.CreateMeterProviderBuilder();

        var actForNullBuilder = () => MeterProviderBuilderExtensions.ConfigureAtyaMetrics(null!, options, resourceBuilder, "meter");
        var actForNullOptions = () => builder.ConfigureAtyaMetrics(null!, resourceBuilder, "meter");
        var actForNullResource = () => builder.ConfigureAtyaMetrics(options, null!, "meter");
        var actForInvalidMeter = () => builder.ConfigureAtyaMetrics(options, resourceBuilder, "");

        _ = actForNullBuilder.Should().Throw<ArgumentNullException>()
            .WithParameterName("builder");
        _ = actForNullOptions.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
        _ = actForNullResource.Should().Throw<ArgumentNullException>()
            .WithParameterName("resourceBuilder");
        _ = actForInvalidMeter.Should().Throw<ArgumentException>()
            .WithParameterName("meterName");
    }

    [Fact]
    public void DatabaseInstrumentationEnricher_Should_Add_Query_Text_Tags_From_Database_Command()
    {
        using var activity = new Activity("database").Start();
        var command = Substitute.For<IDbCommand>();
        command.CommandText.Returns("SELECT 1");

        DatabaseInstrumentationEnricher.EnrichWithSqlCommandText(activity, command);

        _ = activity.Tags.Should().Contain(tag => tag.Key == "db.query.text" && tag.Value == "SELECT 1");
        _ = activity.Tags.Should().Contain(tag => tag.Key == "db.statement" && tag.Value == "SELECT 1");
    }

    [Fact]
    public void DatabaseInstrumentationEnricher_Should_Add_Query_Text_Tags_From_Text()
    {
        using var activity = new Activity("database").Start();

        DatabaseInstrumentationEnricher.EnrichWithSqlText(activity, "SELECT 2");

        _ = activity.Tags.Should().Contain(tag => tag.Key == "db.query.text" && tag.Value == "SELECT 2");
        _ = activity.Tags.Should().Contain(tag => tag.Key == "db.statement" && tag.Value == "SELECT 2");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void DatabaseInstrumentationEnricher_Should_Ignore_Missing_Query_Text(string? commandText)
    {
        using var activity = new Activity("database").Start();

        DatabaseInstrumentationEnricher.EnrichWithSqlText(activity, commandText);

        _ = activity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void DatabaseInstrumentationEnricher_Should_Ignore_Non_Database_Command()
    {
        using var activity = new Activity("database").Start();
        var command = new object();

        DatabaseInstrumentationEnricher.EnrichWithSqlCommandText(activity, command);

        _ = activity.Tags.Should().BeEmpty();
    }

    [Fact]
    public void DatabaseInstrumentationEnricher_Should_Throw_When_Arguments_Are_Null()
    {
        using var activity = new Activity("database").Start();
        var command = Substitute.For<IDbCommand>();
        command.CommandText.Returns("SELECT 1");

        var actForNullActivityWithCommand = () => DatabaseInstrumentationEnricher.EnrichWithSqlCommandText(null!, command);
        var actForNullCommand = () => DatabaseInstrumentationEnricher.EnrichWithSqlCommandText(activity, null!);
        var actForNullActivityWithText = () => DatabaseInstrumentationEnricher.EnrichWithSqlText(null!, "SELECT 1");

        _ = actForNullActivityWithCommand.Should().Throw<ArgumentNullException>()
            .WithParameterName("activity");
        _ = actForNullCommand.Should().Throw<ArgumentNullException>()
            .WithParameterName("command");
        _ = actForNullActivityWithText.Should().Throw<ArgumentNullException>()
            .WithParameterName("activity");
    }

    private static OpenTelemetryOptions CreateValidOptions()
    {
        return new OpenTelemetryOptions
        {
            ServiceName = "Orders.Service",
        };
    }

    private static OpenTelemetryOptions CreateFullOptions()
    {
        var options = CreateValidOptions();
        options.ActivitySources.Add("Orders.Workflows");
        options.Meters.Add("Orders.Business");
        options.Instrumentations.AspNetCore.Enabled = true;
        options.Instrumentations.HttpClient.Enabled = true;
        options.Instrumentations.SqlClient.Enabled = true;
        options.Instrumentations.SqlClient.CaptureSqlText = true;
        options.Instrumentations.EntityFrameworkCore.Enabled = true;
        options.Instrumentations.EntityFrameworkCore.CaptureSqlText = true;
        options.Instrumentations.GrpcClient.Enabled = true;
        options.Instrumentations.Runtime.Enabled = true;
        options.Exporters.Otlp.Enabled = true;
        options.Exporters.Otlp.Endpoint = "http://localhost:4317";
        options.Exporters.Otlp.Protocol = OtlpExportProtocol.Grpc;
        return options;
    }
}
