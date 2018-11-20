using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.Instrumentation.Bcl;
using OpenTracing.Contrib.Instrumentation.Diagnostic;
using OpenTracing.Contrib.Instrumentation.Logging;

namespace OpenTracing.Contrib.Instrumentation
{
    public static class OpenTracingBuilderExtensions
    {
        public static IOpenTracingBuilder AddDiagnosticSubscriber<TDiagnosticSubscriber>(this IOpenTracingBuilder builder)
            where TDiagnosticSubscriber : DiagnosticObserver
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<DiagnosticObserver, TDiagnosticSubscriber>());

            return builder;
        }

        public static IOpenTracingBuilder AddLoggerProvider(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, OpenTracingLoggerProvider>());
            builder.Services.Configure<LoggerFilterOptions>(options =>
            {
                // All interesting request-specific logs are instrumented via DiagnosticSource.
                options.AddFilter<OpenTracingLoggerProvider>("Microsoft.AspNetCore.Hosting", LogLevel.None);

                // EF Core is sending everything to DiagnosticSource AND ILogger so we completely disable the category.
                options.AddFilter<OpenTracingLoggerProvider>("Microsoft.EntityFrameworkCore", LogLevel.None);
            });

            return builder;
        }

        /// <summary>
        /// Adds instrumentation for the .NET framework BCL.
        /// </summary>
        public static IOpenTracingBuilder AddBcl(this IOpenTracingBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.AddDiagnosticSubscriber<GenericDiagnostics>();

            return builder;
        }

        public static IOpenTracingBuilder ConfigureGenericDiagnostics(this IOpenTracingBuilder builder, Action<GenericDiagnosticOptions> options)
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
