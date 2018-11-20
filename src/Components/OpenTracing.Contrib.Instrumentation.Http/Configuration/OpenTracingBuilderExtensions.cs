using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTracing.Contrib.Instrumentation.HttpClientCore.Configuration
{
    public static class OpenTracingBuilderExtensions
    {
        /// <summary>
        /// Adds instrumentation for the .NET framework BCL.
        /// </summary>
        public static IOpenTracingBuilder AddNetHttp(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddDiagnosticSubscriber<HttpHandlerDiagnostics>();
            builder.ConfigureGenericDiagnostics(options => options.IgnoredListenerNames.Add(HttpHandlerDiagnostics.DiagnosticListenerName));

            return builder;
        }
    }
}
