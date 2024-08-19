using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;

namespace xbimTest.Extensions
{
    public static class IfcStoreExtension
    {
        public static void NewTransaction(this IfcStore model, Action<IfcStore> action, string transName = "default transaction")
        {
            using var trans = model.BeginTransaction(transName);
            action.Invoke(model);
            trans.Commit();
        }
    }
}
