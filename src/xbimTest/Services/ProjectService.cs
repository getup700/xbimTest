using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProcessExtension;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.UtilityResource;
using Xbim.IO;
using xbimTest.Extensions;

namespace xbimTest.Services
{
    internal class ProjectService
    {
        private readonly XbimEditorCredentials _credentials;

        public ProjectService(XbimEditorCredentials credentials)
        {
            _credentials = credentials;
        }

        public IfcStore OpenProject(string path)
        {
            var store = IfcStore.Open(path, _credentials);
            return store;
        }

        public IfcStore CreateStore(Action<IfcStore> action = null)
        {
            var store = IfcStore.Create(XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);
            store.NewTransaction(store =>
            {
                var app = store.Instances.New<IfcApplication>();
                app.ApplicationIdentifier = "xbimTest";
                app.ApplicationFullName = "fullFileName";
                app.Version ="1.0.1";
                app.ApplicationDeveloper = store.Instances.New<IfcOrganization>(org =>
                {
                    org.Name = "StartCompany";
                    org.Identification = "Identification";
                });

                action?.Invoke(store);
            });

            return store;
        }

       


    }
}
