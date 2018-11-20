using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing.Contrib.Instrumentation;
using OpenTracing.Contrib.Instrumentation.SqlClient.Configuration;
using OpenTracing.Util;

namespace OpenTracing.Contrib.Tests.Sandbox.SqlClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connStr =@"Data Source=(LocalDB)\MSSQLLocalDB;Integrated Security=True;Database=TestSandbox";

            IHost host = new HostBuilder()
                .ConfigureServices(collection =>
                {
                    collection.AddTransient<IDapperContext>(provider => new DapperContext(connStr));
                    collection.AddTransient<IDbManager, DbManager>();
                    collection.AddOpenTracing(builder =>
                    {
                        builder
                            .AddBcl()
                            .AddSqlClient()
                            .AddLoggerProvider();
                    });

                    collection.AddSingleton<ITracer>(cli =>
                    {
                        string applicationName = "SandboxSqlClient";
                        string environment = "Test";
                        string jaegerAgentHost = "localhost";

                        Environment.SetEnvironmentVariable("JAEGER_SERVICE_NAME", applicationName);
                        Environment.SetEnvironmentVariable("JAEGER_AGENT_HOST", jaegerAgentHost);
                        Environment.SetEnvironmentVariable("JAEGER_AGENT_PORT", "6831");
                        Environment.SetEnvironmentVariable("JAEGER_SAMPLER_TYPE", "const");

                        var loggerFactory = new LoggerFactory();

                        Jaeger.Configuration config = Jaeger.Configuration.FromEnv(loggerFactory);

                        config.WithTracerTags(new Dictionary<string, string>()
                        {
                            {"environment", environment}
                        });

                        ITracer tracer = config.GetTracer();

                        if (!GlobalTracer.IsRegistered())
                        {
                            GlobalTracer.Register(tracer);
                        }

                        return tracer;
                    });
                }).UseConsoleLifetime().Build();


            await host.StartAsync();

            var dbManager = host.Services.GetService<IDbManager>();

            dbManager.CreateDb();
        }
    }
}
