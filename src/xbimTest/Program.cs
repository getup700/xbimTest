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
        services.AddSingleton<SimpleProjectService>();
        services.AddSingleton<DemoService>();
        services.AddSingleton<LinqService>();
        services.AddSingleton<TransferService>();
        services.AddSingleton<ProjectHierarchyService>();

        services.AddSingleton<LinqViewModel>();
        services.AddSingleton<ProjectService>();
        services.AddSingleton<MainViewModel>();


        var provider = services.BuildServiceProvider();

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "武汉台北路万象城.ifc");
        var exportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "武汉台北路万象城.xlsx");

        var main = provider.GetService<MainViewModel>();
        while (true)
        {
            var prompt = Console.ReadLine();
            main.Run(prompt);
        }






        //transferService.Export(path, exportPath);

        var transferService = provider.GetService<TransferService>();



        var projectHierarchyService = provider.GetService<ProjectHierarchyService>();
        //projectHierarchyService.Show(path);


    }
}