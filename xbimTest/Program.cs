using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.DateTimeResource;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.PresentationOrganizationResource;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MaterialResource;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.ProfileResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.QuantityResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using xbimTest.Services;


namespace xbimTest;

class Program
{
    static void Main()
    {
        var services = new ServiceCollection();
        services.AddLogging();

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




        var provider = services.BuildServiceProvider();
        var service = provider.GetService<WallService>();
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "ifcDemo.ifc");

        service.BuildingSimpleIfcProject(path);

    }
}