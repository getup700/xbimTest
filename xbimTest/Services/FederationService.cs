using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;

namespace xbimTest.Services
{
    internal class FederationService
    {
        private readonly XbimEditorCredentials credentials;
        private readonly ILogger<FederationService> logger;

        public FederationService(ILogger<FederationService> logger, XbimEditorCredentials credentials)
        {
            this.logger = logger;
            this.credentials = credentials;
        }

        public void Federation()
        {
            using var model = IfcStore.Create(Xbim.Common.Step21.XbimSchemaVersion.Ifc4, Xbim.IO.XbimStoreType.InMemoryModel);
            model.AddModelReference(null);
        }
    }
}
