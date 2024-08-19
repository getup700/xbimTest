using System;
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
