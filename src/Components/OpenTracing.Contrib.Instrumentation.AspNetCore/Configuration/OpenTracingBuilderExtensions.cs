using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTracing.Contrib.Instrumentation.AspNetCore.Configuration
{
    public static class OpenTracingBuilderExtensions
    {
        /// <summary>
        /// Adds instrumentation for ASP.NET Core.
        /// </summary>
        public static IOpenTracingBuilder AddAspNetCore(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddDiagnosticSubscriber<AspNetCoreDiagnostics>();
            builder.ConfigureGenericDiagnostics(options => options.IgnoredListenerNames.Add(AspNetCoreDiagnostics.DiagnosticListenerName));

            return builder;
        }

        public static IOpenTracingBuilder ConfigureAspNetCore(this IOpenTracingBuilder builder, Action<AspNetCoreDiagnosticOptions> options)
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
