using System;
using System.Data.SqlClient;

namespace OpenTracing.Contrib.Instrumentation.SqlClientCore
{
    public class SqlClientDiagnosticEventData
    {
        public SqlCommand SqlCommand { get; set; }

        public string Operation { get; set; }

        public Exception Exception { get; set; }

        public SqlConnection SqlConnection { get; set; }
    }
}