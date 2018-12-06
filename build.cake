using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

var target = Argument("target", "Default");

// Variables
var configuration = "Release";

var artifactOutput = "./artifacts";

Task("Default")
    //.IsDependentOn("Init")
    .IsDependentOn("Test");

Task("Compile")
    .Description("Builds all the projects in the solution")
    .Does(() =>
    {
        StartProcess("dotnet", new ProcessSettings {
            Arguments = "--info"
        });

        DotNetCoreBuildSettings settings = new DotNetCoreBuildSettings();
        settings.NoRestore = true;
        settings.Configuration = configuration;

        string slnPath = "./src/OpenTracing.Contrib.sln";

        Information($"Restoring projects");
        DotNetCoreRestore(slnPath);
        Information($"Building projects");
        DotNetCoreBuild(slnPath, settings);
    });

Task("Test")
    .Description("Run Tests")
    .IsDependentOn("Compile")
    .Does(() =>
    {      
        DotNetCoreTestSettings settings = new DotNetCoreTestSettings();
        settings.NoBuild = true;
        settings.NoRestore = true;
        settings.Configuration = configuration;

        string appveyor = EnvironmentVariable("APPVEYOR");
        // if(!string.IsNullOrEmpty(appveyor) && appveyor == "True")
        // {
        //     settings.ArgumentCustomization  = args => args.Append(" --test-adapter-path:. --logger:Appveyor");
        // }

        IList<TestProjMetadata> testProjMetadatas = GetProjMetadatas();

        foreach (var testProj in testProjMetadatas)
        {
           string testProjectPath = testProj.CsProjPath;

           foreach(string targetFramework in testProj.TargetFrameworks)
           {
                Information($"Running {targetFramework.ToUpper()} tests for {testProj.AssemblyName}");
                settings.Framework = targetFramework;

                // Temporary workaround. See https://github.com/armutcom/armut-opentracing-contrib-instrumentation/issues/1
                if(appveyor == "True")
                {
                    continue;
                }

                if(IsRunningOnUnix() && targetFramework == "net461")
                {
                    RunXunitUsingMono(targetFramework, $"{testProj.DirectoryPath}/bin/Release/{targetFramework}/{testProj.AssemblyName}.dll");
                }
                else
                {
                    DotNetCoreTest(testProjectPath, settings);
                }
           }
        }
    });

Task("Nuget-Pack")
    .Description("Publish to nuget")
    .Does(() =>
    {        
        // var settings = new DotNetCorePackSettings();
        // settings.Configuration = configuration;

        // settings.OutputDirectory = artifactOutput;
        // settings.WorkingDirectory = projectPath;
        // DotNetCorePack(projectProj, settings);
    });

Task("Get-Version")
    .Description("Get version")
    .Does(() =>
    {
        Information(GetProjectVersion());
    });

Task("Init")
    .Description("Initialize task prerequisites")
    .Does(() =>
    {
        if(IsRunningOnUnix())
        {
            Information("Installing nuget");
            InstallXUnitNugetPackage();
        }
    });


RunTarget(target);

/*
/ HELPER METHODS
*/

private void InstallXUnitNugetPackage()
{
    NuGetInstallSettings nugetInstallSettings = new NuGetInstallSettings();
    nugetInstallSettings.Version = "2.4.0";
    nugetInstallSettings.OutputDirectory = "testrunner";            
    nugetInstallSettings.WorkingDirectory = ".";

    NuGetInstall("xunit.runner.console", nugetInstallSettings);
}

private void RunXunitUsingMono(string targetFramework, string assemblyPath)
{
    StartProcess("mono", new ProcessSettings {
        Arguments = $"./testrunner/xunit.runner.console.2.4.0/tools/{targetFramework}/xunit.console.exe {assemblyPath}"
    });
}

private IList<TestProjMetadata> GetProjMetadatas()
{
    var testsRoot = MakeAbsolute(Directory("./src/Tests/"));
    var csProjs = GetFiles($"{testsRoot}/**/*.csproj").Where(fp => fp.FullPath.EndsWith("Tests.csproj")).ToList();

    IList<TestProjMetadata> testProjMetadatas = new List<TestProjMetadata>();

    foreach (var csProj in csProjs)
    {
        string csProjPath = csProj.FullPath;

        string[] targetFrameworks = GetProjectTargetFrameworks(csProjPath);
        string directoryPath = csProj.GetDirectory().FullPath;
        string assemblyName = GetAssemblyName(csProjPath);

        var testProjMetadata = new TestProjMetadata(directoryPath, csProjPath, targetFrameworks, assemblyName);
        testProjMetadatas.Add(testProjMetadata);
    }

    return testProjMetadatas;
}

private void UpdateProjectVersion(string version)
{
    Information("Setting version to " + version);

    if(string.IsNullOrWhiteSpace(version))
    {
        throw new CakeException("No version specified! You need to pass in --targetversion=\"x.y.z\"");
    }

    var file =  MakeAbsolute(File("./src/Directory.Build.props"));

    Information(file.FullPath);

    var project = System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8);

    var projectVersion = new Regex(@"<Version>.+<\/Version>");
    project = projectVersion.Replace(project, string.Concat("<Version>", version, "</Version>"));

    System.IO.File.WriteAllText(file.FullPath, project, Encoding.UTF8);
}

private string GetProjectVersion()
{
    var file =  MakeAbsolute(File("./src/Directory.Build.props"));

    Information(file.FullPath);

    var project = System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8);
    int startIndex = project.IndexOf("<Version>") + "<Version>".Length;
    int endIndex = project.IndexOf("</Version>", startIndex);

    string version = project.Substring(startIndex, endIndex - startIndex);
    string buildNumber = (EnvironmentVariable("TRAVIS_BUILD_NUMBER")) ?? "0";
    version = $"{version}.{buildNumber}";

    return version;
}

private string[] GetProjectTargetFrameworks(string csprojPath)
{
    var file =  MakeAbsolute(File(csprojPath));
    var project = System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8);

    bool multipleFrameworks = project.Contains("<TargetFrameworks>");
    string startElement = multipleFrameworks ? "<TargetFrameworks>" : "<TargetFramework>";
    string endElement = multipleFrameworks ? "</TargetFrameworks>" : "</TargetFramework>";

    int startIndex = project.IndexOf(startElement) + startElement.Length;
    int endIndex = project.IndexOf(endElement, startIndex);

    string targetFrameworks = project.Substring(startIndex, endIndex - startIndex);
    return targetFrameworks.Split(';');
}

private string GetAssemblyName(string csprojPath)
{
    var file =  MakeAbsolute(File(csprojPath));
    var project = System.IO.File.ReadAllText(file.FullPath, Encoding.UTF8);
    
    bool assemblyNameElementExists = project.Contains("<AssemblyName>");

    string assemblyName = string.Empty;

    if(assemblyNameElementExists)
    {
        int startIndex = project.IndexOf("<AssemblyName>") + "<AssemblyName>".Length;
        int endIndex = project.IndexOf("</AssemblyName>", startIndex);

        assemblyName = project.Substring(startIndex, endIndex - startIndex);
    }
    else
    {        
        int startIndex = csprojPath.LastIndexOf("/") + 1;
        int endIndex = csprojPath.IndexOf(".csproj", startIndex);

        assemblyName = csprojPath.Substring(startIndex, endIndex - startIndex);
    }

    return assemblyName;
}

/*
/ MODELS
 */
 public class TestProjMetadata
 {
    public TestProjMetadata(string directoryPath, string csProjPath, string[] targetFrameworks, string assemblyName) 
        => (DirectoryPath, CsProjPath, TargetFrameworks, AssemblyName) = (directoryPath, csProjPath, targetFrameworks, assemblyName);

    public string DirectoryPath { get; }
    public string CsProjPath { get; }
    public string AssemblyName { get; set; }
    public string[] TargetFrameworks { get; }
 }