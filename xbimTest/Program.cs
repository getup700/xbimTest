using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using Xbim.Ifc;
using xbimTest.Services;
using xbimTest.ViewModels;

namespace xbimTest;

class Program
{
    static void Main()
    {
        var services = new ServiceCollection();

        services.AddLogging(config =>
        {
            config.AddConsole().SetMinimumLevel(LogLevel.Trace);
        });

        services.AddSingleton(new XbimEditorCredentials()
        {
            ApplicationDevelopersName = "IfcExplor",
            ApplicationFullName = "IfcExplor",
            ApplicationIdentifier = "Stark",
            ApplicationVersion = "1.0",
            EditorsFamilyName = "FamilyName",
            EditorsGivenName = "Omar",
            EditorsOrganisationName = "Stark"
        });
        services.AddSingleton<WallService>();
        services.AddSingleton<DemoService>();
        services.AddSingleton<LinqService>();
        services.AddSingleton<TransferService>();
        services.AddSingleton<ProjectHierarchyService>();

        services.AddSingleton<LinqViewModel>();


        var provider = services.BuildServiceProvider();
        var transferService = provider.GetService<TransferService>();

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "武汉台北路万象城.ifc");
        var exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "武汉台北路万象城.xlsx");
        //transferService.Export(path, exportPath);


        var projectHierarchyService = provider.GetService<ProjectHierarchyService>();
        //projectHierarchyService.Show(path);

        var wallService = provider.GetService<WallService>();
        wallService.BuildNewProject(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "newProject.ifc"));

    }
}