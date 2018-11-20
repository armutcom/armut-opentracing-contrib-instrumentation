using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTracing.Contrib.Instrumentation.EntityFrameworkCore.Configuration
{
    public static class OpenTracingBuilderExtensions
    {
        /// <summary>
        /// Adds instrumentation for Entity Framework Core.
        /// </summary>
        public static IOpenTracingBuilder AddEntityFrameworkCore(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddDiagnosticSubscriber<EntityFrameworkCoreDiagnostics>();
            builder.ConfigureGenericDiagnostics(options => options.IgnoredListenerNames.Add(EntityFrameworkCoreDiagnostics.DiagnosticListenerName));

            return builder;
        }

        /// <summary>
        /// Configuration options for the instrumentation of Entity Framework Core.
        /// </summary>
        public static IOpenTracingBuilder ConfigureEntityFrameworkCore(this IOpenTracingBuilder builder, Action<EntityFrameworkCoreDiagnosticOptions> options)
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
