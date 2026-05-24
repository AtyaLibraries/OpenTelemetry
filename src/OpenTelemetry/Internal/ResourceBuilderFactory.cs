// <copyright file="ResourceBuilderFactory.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Foundation.Guards;
using OpenTelemetry.Resources;

namespace Atya.Diagnostics.OpenTelemetry.Internal;

internal static class ResourceBuilderFactory
{
    public static ResourceBuilder Create(OpenTelemetryOptions options, string activitySourceName, string meterName)
    {
        _ = Guard.AgainstNull(options);
        _ = Guard.AgainstNullOrWhiteSpace(activitySourceName);
        _ = Guard.AgainstNullOrWhiteSpace(meterName);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: options.ServiceName.Trim(),
                serviceVersion: string.IsNullOrWhiteSpace(options.ServiceVersion) ? null : options.ServiceVersion.Trim(),
                serviceNamespace: options.Resource.ServiceNamespace,
                serviceInstanceId: options.Resource.ServiceInstanceId);

        if (!string.IsNullOrWhiteSpace(options.Resource.DeploymentEnvironment))
        {
            _ = resourceBuilder.AddAttributes(new KeyValuePair<string, object>[]
            {
                new ("deployment.environment", options.Resource.DeploymentEnvironment),
            });
        }

        if (options.Resource.Attributes.Count > 0)
        {
            _ = resourceBuilder.AddAttributes(options.Resource.Attributes);
        }

        return resourceBuilder;
    }
}
