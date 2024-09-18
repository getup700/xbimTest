using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.UtilityResource;
using Xbim.IO;
using xbimTest.Extensions;

namespace xbimTest.Services
{
    internal class ProjectService
    {
        private readonly XbimEditorCredentials _credentials;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(XbimEditorCredentials credentials, ILogger<ProjectService> logger)
        {
            _credentials = credentials;
            _logger = logger;
        }

        public IfcStore OpenProject(string path)
        {
            var store = IfcStore.Open(path, _credentials);
            return store;
        }

        public IfcStore CreateStore(string projectName, Action<IfcStore> action = null)
        {
            var store = CreateAndInitModel(projectName);
            var building = CreateBuilding(store, "BuildingName");
            store.NewTransaction(store =>
            {
                var app = store.Instances.New<IfcApplication>();
                app.ApplicationIdentifier = "xbimTest";
                app.ApplicationFullName = "fullFileName";
                app.Version = "1.0.1";
                app.ApplicationDeveloper = store.Instances.New<IfcOrganization>(org =>
                {
                    org.Name = "StarkCompany";
                    org.Identification = "Identification";
                });

                action?.Invoke(store);
            });

            return store;
        }

        public void Save(IfcStore store,string fullFileName)
        {
            try
            {
                _logger.LogTrace($"start saving the ifc project...");
                store.SaveAs(fullFileName, StorageType.Ifc);
                _logger.LogTrace($"{fullFileName} has been successfully written");
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to save {fullFileName}");
                _logger.LogError(e.Message);
            }
        }

        private IfcBuilding CreateBuilding(IfcStore model, string name)
        {
            IfcBuilding building = null;
            model.NewTransaction(model =>
            {
                building = model.Instances.New<IfcBuilding>();
                building.Name = name;
                building.CompositionType = IfcElementCompositionEnum.ELEMENT;

                var localPlacement = model.Instances.New<IfcLocalPlacement>();
                building.ObjectPlacement = localPlacement;
                var placement = model.Instances.New<IfcAxis2Placement3D>();
                localPlacement.RelativePlacement = placement;

                placement.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0));
                //get the project there should only be one and it should exist
                var project = model.Instances.OfType<IfcProject>().FirstOrDefault();
                project?.AddBuilding(building);
            }, "Create building");
            return building;
        }

        /// <summary>
        /// Sets up the basic parameters any model must provide, units, ownership etc
        /// </summary>
        /// <param name="projectName">Name of the project</param>
        /// <returns></returns>
        private IfcStore CreateAndInitModel(string projectName)
        {
            //now we can create an IfcStore, it is in Ifc4 format and will be held in memory rather than in a database
            //database is normally better in performance terms if the model is large >50MB of Ifc or if robust transactions are required
            var model = IfcStore.Create(_credentials, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

            //Begin a transaction as all changes to a model are ACID
            using (var txn = model.BeginTransaction("Initialise Model"))
            {
                //create a project
                var project = model.Instances.New<IfcProject>();
                //set the units to SI (mm and metres)
                project.Initialize(ProjectUnits.SIUnitsUK);
                project.Name = projectName;
                //now commit the changes, else they will be rolled back at the end of the scope of the using statement
                txn.Commit();
            }
            return model;

        }

    }
}
