// <copyright file="ResourceBuilderFactory.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.OpenTelemetry.Options;
using Atya.Diagnostics.Observation.Models;
using Atya.Foundation.Guards;
using OpenTelemetry.Resources;

namespace Atya.Diagnostics.OpenTelemetry.Internal;

internal static class ResourceBuilderFactory
{
    public static ResourceBuilder Create(
        ObservationIdentity identity,
        OpenTelemetryResourceOptions resourceOptions)
    {
        _ = Guard.AgainstNull(identity);
        _ = Guard.AgainstNull(resourceOptions);

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: identity.ServiceName.Trim(),
                serviceVersion: string.IsNullOrWhiteSpace(identity.ServiceVersion) ? null : identity.ServiceVersion.Trim(),
                serviceNamespace: resourceOptions.ServiceNamespace,
                serviceInstanceId: resourceOptions.ServiceInstanceId);

        if (!string.IsNullOrWhiteSpace(resourceOptions.DeploymentEnvironment))
        {
            _ = resourceBuilder.AddAttributes(new KeyValuePair<string, object>[]
            {
                new ("deployment.environment.name", resourceOptions.DeploymentEnvironment),
                new ("deployment.environment", resourceOptions.DeploymentEnvironment),
            });
        }

        if (resourceOptions.Attributes.Count > 0)
        {
            _ = resourceBuilder.AddAttributes(resourceOptions.Attributes);
        }

        return resourceBuilder;
    }
}
