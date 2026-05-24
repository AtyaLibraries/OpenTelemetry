// <copyright file="TelemetryNameNormalizer.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Foundation.Guards;

namespace Atya.Diagnostics.OpenTelemetry.Internal;

internal static class TelemetryNameNormalizer
{
    public static IReadOnlyList<string> Normalize(string primaryName, IEnumerable<string?> additionalNames)
    {
        var normalizedPrimaryName = Guard.AgainstNullOrWhiteSpace(primaryName).Trim();
        _ = Guard.AgainstNull(additionalNames);

        var names = new List<string> { normalizedPrimaryName };
        var seen = new HashSet<string>(StringComparer.Ordinal)
        {
            normalizedPrimaryName,
        };

        foreach (var additionalName in additionalNames)
        {
            if (string.IsNullOrWhiteSpace(additionalName))
            {
                continue;
            }

            var normalizedName = additionalName.Trim();
            if (seen.Add(normalizedName))
            {
                names.Add(normalizedName);
            }
        }

        return names;
    }
}
