using System;
using Xbim.Ifc;

namespace xbimTest.Extensions
{
    public static class IfcStoreExtension
    {
        public static void NewTransaction(this IfcStore store, Action<IfcStore> action, string transName = "default transaction")
        {
            using var trans = store.BeginTransaction(transName);
            action.Invoke(store);
            trans.Commit();
        }
    }
}
