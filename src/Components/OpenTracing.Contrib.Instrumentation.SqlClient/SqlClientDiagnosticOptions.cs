using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace OpenTracing.Contrib.Instrumentation.SqlClient
{
    public class SqlClientDiagnosticOptions
    {
        public const string DefaultComponent = "System.Data.SqlClient";

        private string _componentName = DefaultComponent;
        private Func<object, SqlClientDiagnosticEventData> _sqlClientDiagnosticArgResolver;

        /// <summary>
        /// Allows changing the "component" tag of created spans.
        /// </summary>
        public string ComponentName
        {
            get => _componentName;
            set => _componentName = value ?? throw new ArgumentNullException(nameof(ComponentName));
        }

        /// <summary>
        /// A delegate that returns the SqlClientDiagnosticEventData
        /// </summary>
        internal Func<object, SqlClientDiagnosticEventData> SqlClientDiagnosticEventDataResolver
        {
            get
            {
                return _sqlClientDiagnosticArgResolver ?? (_sqlClientDiagnosticArgResolver = (data) =>
                {
                    var argType = data.GetType();

                    var sqlClientDiagnosticEventData = new SqlClientDiagnosticEventData
                    {
                        SqlCommand = argType.GetProperty("Command")?.GetValue(data) as SqlCommand,
                        Operation = argType.GetProperty("Operation")?.GetValue(data).ToString(),
                        Exception = argType.GetProperty("Exception")?.GetValue(data) as Exception,
                        SqlConnection = argType.GetProperty("SqlConnection")?.GetValue(data) as SqlConnection
                    };

                    return sqlClientDiagnosticEventData;
                });
            }
            set => _sqlClientDiagnosticArgResolver = value ?? throw new ArgumentNullException(nameof(SqlClientDiagnosticEventDataResolver));
        }
    }
}