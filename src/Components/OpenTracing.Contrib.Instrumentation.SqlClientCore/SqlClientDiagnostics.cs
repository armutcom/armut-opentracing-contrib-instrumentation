using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing.Contrib.Instrumentation.Diagnostic;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.Instrumentation.SqlClientCore
{
    internal class SqlClientDiagnostics : DiagnosticListenerObserver
    {
        public const string DiagnosticListenerName = "SqlClientDiagnosticListener";

        private const string TagMethod = "db.method";
        private const string TagIsAsync = "db.async";

        private readonly SqlClientDiagnosticOptions _options;

        public SqlClientDiagnostics(ILoggerFactory loggerFactory, ITracer tracer, IOptions<SqlClientDiagnosticOptions> options)
            : base(loggerFactory, tracer)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        protected override string GetListenerName() => DiagnosticListenerName;

        protected override void OnNext(string eventName, object untypedArg)
        {
            switch (eventName)
            {
                case "System.Data.SqlClient.WriteCommandBefore":
                {
                    var eventData = _options.SqlClientDiagnosticEventDataResolver(untypedArg);

                    Tracer.BuildSpan($"DB {eventData.Operation}")
                        .WithTag(Tags.SpanKind, Tags.SpanKindClient)
                        .WithTag(Tags.Component, _options.ComponentName)
                        .WithTag(Tags.DbInstance, eventData.SqlCommand.Connection.Database)
                        .WithTag(Tags.DbStatement, eventData.SqlCommand.CommandText)
                        .WithTag(TagMethod, eventData.Operation)
                        .WithTag(TagIsAsync, eventData.Operation.Contains("Async"))
                        .StartActive();
                }
                    break;
                case "System.Data.SqlClient.WriteCommandAfter":
                {
                    DisposeActiveScope(isScopeRequired: true);
                }
                    break;

                case "System.Data.SqlClient.WriteCommandError":
                {
                    var eventData = _options.SqlClientDiagnosticEventDataResolver(untypedArg);

                    IScope scope = Tracer.ScopeManager.Active;
                    scope?.Span.SetTag(Tags.DbInstance, eventData.SqlCommand.Connection.Database);

                    // The "WriteCommandAfter" event is NOT called in case of an exception,
                    // so we have to dispose the scope here as well!
                    DisposeActiveScope(isScopeRequired: true, exception: eventData.Exception);
                }
                    break;
                default:
                {
                    ProcessUnhandledEvent(eventName, untypedArg);
                }
                    break;
            }
        }
    }
}
