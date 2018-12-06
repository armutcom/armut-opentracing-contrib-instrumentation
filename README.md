# OpenTracing instrumentation for .NET apps

This repository is fork of the [opentracing-contrib/csharp-netcore](https://github.com/opentracing-contrib/csharp-netcore) and provides OpenTracing instrumentation libraries for .NET based applications. It can be used with any OpenTracing compatible tracer.

## Supported .NET versions

This project currently only supports apps targeting `netcoreapp2.0` (.NET Core 2.0) or higher! .NET Framework support will be added to next versions that use `System.Diagnostics.Trace` and `System.Diagnostics.Debug`.

## Continuous integration

| Build server                | Platform      | Build status                                                                                                                                                        | Integration tests                                                                                                                                                   |
|-----------------------------|---------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| AppVeyor                    | Windows       | [![Build status](https://ci.appveyor.com/api/projects/status/nrf6fenhxlutti05?svg=true)](https://ci.appveyor.com/project/Blind-Striker/armut-opentracing-contrib-instrumentation)          | |
| Travis                      | Linux / MacOS | [![Build Status](https://travis-ci.com/armutcom/armut-opentracing-contrib-instrumentation.svg?branch=master)](https://travis-ci.com/armutcom/armut-opentracing-contrib-instrumentation) | |

## Relation to opentracing-contrib/csharp-netcore

This repository is fork of the [opentracing-contrib/csharp-netcore](https://github.com/opentracing-contrib/csharp-netcore). Original repository cloned as a git submodule and related class files linked to projects. The following improvements have been made:

* Instead of combining all instrumentation under one big library, all instrumentation libraries separated.
* Because of `DiagnosticObserver` and `DiagnosticListenerObserver` classes were internal, it was preventing to create new instrumentation libraries. These classes have made public. You can use `OpenTracing.Contrib.Instrumentation` (The core library) package to create your own instrumentation libraries. See [SqlClientDiagnostics](https://github.com/armutcom/armut-opentracing-contrib-instrumentation/blob/master/src/Components/OpenTracing.Contrib.Instrumentation.SqlClientCore/SqlClientDiagnostics.cs)
* `System.Data.SqlClient` instrumentation library added for the projects that not use Entity Framework Core (for example [Dapper](https://github.com/StackExchange/Dapper)).

## Supported libraries and frameworks

### DiagnosticSource based instrumentation

This project supports any library or framework that uses .NET's [`DiagnosticSource`](https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/DiagnosticSourceUsersGuide.md)
to instrument its code. It will create a span for every [`Activity`](https://github.com/dotnet/corefx/blob/master/src/System.Diagnostics.DiagnosticSource/src/ActivityUserGuide.md)
and it will create `span.Log` calls for all other diagnostic events.

To further improve the tracing output, the library provides enhanced instrumentation
(Inject/Extract, tags, configuration options) for the following libraries / frameworks:

* ASP.NET Core
* Entity Framework Core
* [System.Data.SqlClient Core](https://github.com/dotnet/corefx/blob/master/src/System.Data.SqlClient/src/System/Data/SqlClient/SqlClientDiagnosticListenerExtensions.cs)
* .NET Core BCL types and HttpClient

### Microsoft.Extensions.Logging based instrumentation

This project also adds itself as a logger provider for logging events from the `Microsoft.Extensions.Logging` system.
It will create `span.Log` calls for each logging event, however, it will only create them if there is an active span (`ITracer.ActiveSpan`).

## Usage

This project depends on several packages from Microsofts new `Microsoft.Extensions.*` stack (e.g. Dependency Injection, Logging)
so its main use case is ASP.NET Core apps but it's also possible to instrument non-web based .NET Core apps like console apps, background services etc.
if they also use this stack.

#### 1. Add the NuGet packages to your project.

Add the ones you need from the packages below to your project. The following commands can be used to install packages.

Run the following command in the Package Manager Console

```
Install-Package OpenTracing.Contrib.Instrumentation
Install-Package OpenTracing.Contrib.Instrumentation.AspNetCore
Install-Package OpenTracing.Contrib.Instrumentation.EntityFrameworkCore
Install-Package OpenTracing.Contrib.Instrumentation.HttpClientCore
Install-Package OpenTracing.Contrib.Instrumentation.SqlClientCore
```

Or use `dotnet cli`

```
dotnet add package OpenTracing.Contrib.Instrumentation
dotnet add package OpenTracing.Contrib.Instrumentation.AspNetCore
dotnet add package OpenTracing.Contrib.Instrumentation.EntityFrameworkCore
dotnet add package OpenTracing.Contrib.Instrumentation.HttpClientCore
dotnet add package OpenTracing.Contrib.Instrumentation.SqlClientCore
```

##### 2. Add the OpenTracing services to your `IServiceCollection` via `services.AddOpenTracing()`

How you do this depends on how you've setup the `Microsoft.Extensions.DependencyInjection` system in your app.

In ASP.NET Core apps you can add the call to your `ConfigureServices` method (of your `Program.cs` file):

```csharp
public static IWebHost BuildWebHost(string[] args)
{
    return WebHost.CreateDefaultBuilder(args)
        .UseStartup<Startup>()
        .ConfigureServices(services =>
        {
            services.AddOpenTracing(builder =>
            {
                builder
                    .AddBcl()
                    .AddNetHttp()
                    .AddAspNetCore()
                    .AddEntityFrameworkCore()
                    .AddSqlClient()
                    .AddLoggerProvider();
            });
        })
        .Build();
}
```

#### 3. Make sure `InstrumentationService`, which implements `IHostedService`, is started.

`InstrumentationService` is responsible for starting and stopping the instrumentation.
The service implements `IHostedService` so **it is automatically started in ASP.NET Core**,
however if you have your own console host, you manually have to call `StartAsync` and `StopAsync`.

Note that .NET Core 2.1 will greatly simplify this setup by introducing a generic `HostBuilder` that works similar to the existing `WebHostBuilder` from ASP.NET Core. Have a look at the [OpenTracing.Contrib.Tests.Sandbox.SqlClientCore](https://github.com/armutcom/armut-opentracing-contrib-instrumentation/blob/master/src/Tests/Sandbox/OpenTracing.Contrib.Tests.Sandbox.SqlClientCore/Program.cs) sample for an example of a `HostBuilder` based console application.

## License

Licensed under MIT, see [LICENSE](LICENSE) for the full text.
