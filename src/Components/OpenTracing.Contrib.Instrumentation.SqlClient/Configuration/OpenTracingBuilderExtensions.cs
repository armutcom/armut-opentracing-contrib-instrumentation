using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTracing.Contrib.Instrumentation.SqlClient.Configuration
{
    public static class OpenTracingBuilderExtensions
    {
        /// <summary>
        /// Adds instrumentation for System.Data.SqlClient.
        /// </summary>
        public static IOpenTracingBuilder AddSqlClient(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddDiagnosticSubscriber<SqlClientDiagnostics>();
            builder.ConfigureGenericDiagnostics(options => options.IgnoredListenerNames.Add(SqlClientDiagnostics.DiagnosticListenerName));

            return builder;
        }

        /// <summary>
        /// Configuration options for the instrumentation of System.Data.SqlClient.
        /// </summary>
        public static IOpenTracingBuilder ConfigureSqlClient(this IOpenTracingBuilder builder, Action<SqlClientDiagnosticOptions> options)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (options != null)
            {
                builder.Services.Configure(options);
            }

            return builder;
        }
    }
}
