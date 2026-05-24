// <copyright file="DatabaseInstrumentationEnricher.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using System.Data;
using System.Diagnostics;
using Atya.Foundation.Guards;

namespace Atya.Diagnostics.OpenTelemetry.Internal;

internal static class DatabaseInstrumentationEnricher
{
    private const string DbQueryTextAttribute = "db.query.text";
    private const string LegacyDbStatementAttribute = "db.statement";

    public static void EnrichWithSqlCommandText(Activity activity, object command)
    {
        _ = Guard.AgainstNull(activity);
        _ = Guard.AgainstNull(command);

        if (command is IDbCommand databaseCommand)
        {
            EnrichWithSqlText(activity, databaseCommand.CommandText);
        }
    }

    public static void EnrichWithSqlText(Activity activity, string? commandText)
    {
        _ = Guard.AgainstNull(activity);

        if (string.IsNullOrWhiteSpace(commandText))
        {
            return;
        }

        _ = activity.SetTag(DbQueryTextAttribute, commandText);
        _ = activity.SetTag(LegacyDbStatementAttribute, commandText);
    }
}
