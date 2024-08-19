using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace xbimTest.Services
{
    internal class LinqService
    {
        private readonly XbimEditorCredentials credentials;
        private readonly ILogger<LinqService> logger;

        public LinqService(XbimEditorCredentials credentials, ILogger<LinqService> logger)
        {
            this.credentials = credentials;
            this.logger = logger;
        }

        public void GetWallByLinqWhere(string name)
        {
            var model = IfcStore.Open(name);
            var results = model.Instances.Where(x => x is IIfcWallStandardCase
                || x is IIfcDoor
                || x is IIfcWindow);
            foreach (IIfcProduct product in results)
            {
                logger.LogDebug(product.ExpressType.ToString());
            }
        }

        public void GetWallByLinqConcat(string name)
        {
            var model = IfcStore.Open(name); 
            var requiredProducts = new IIfcProduct[0]
            .Concat(model.Instances.OfType<IIfcWallStandardCase>())
            .Concat(model.Instances.OfType<IIfcDoor>())
            .Concat(model.Instances.OfType<IIfcWindow>());
            //遍历你所需要的实体，数量大概
            foreach (var product in requiredProducts)
            {
                //相关的处理...
                logger.LogDebug(product.ExpressType.ToString());
            }
        }

        public void GetWallWithoutLinq(string name)
        {
            var model = IfcStore.Open(name);
            foreach (var product in model.Instances)
            {
                if (product is IIfcWallStandardCase
                    || product is IIfcDoor
                    || product is IIfcWindow)
                {
                    logger.LogDebug(product.ExpressType.ToString());
                }
            }
        }
    }
}
