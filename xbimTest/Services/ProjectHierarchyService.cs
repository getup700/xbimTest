using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;

namespace xbimTest.Services
{
    internal class ProjectHierarchyService
    {
        private readonly XbimEditorCredentials credentials;
        private readonly ILogger<ProjectHierarchyService> logger;
        public ProjectHierarchyService(XbimEditorCredentials credentials, ILogger<ProjectHierarchyService> logger)
        {
            this.credentials = credentials;
            this.logger = logger;
        }

        public void Show(string fullFileName)
        {
            using var model = IfcStore.Open(fullFileName);
            var project = model.Instances.FirstOrDefault<IIfcProject>();
            PrintHierarchy(project, 0);
        }
        /// <summary>
        /// =>编号[IfcProject]
        ///     =>Default[IfcSite]
        ///         =>[IfcBuilding]
        ///             =>CONTAINER[IfcBuildingStorey]
        ///                 =>Floor
        ///             =>Level 1
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="level"></param>
        private static void PrintHierarchy(IIfcObjectDefinition obj, int level)
        {
            Console.WriteLine(string.Format("{0}{1} [{2}]", GetIndent(level), obj.Name, obj.GetType().Name));

            //只有空间元素可以包含建筑元素
            if (obj is IIfcSpatialStructureElement spatialStructureElement)
            {
                //使用 IfcRelContainedInSpatialElement 获取包含的元素
                var containedElements = spatialStructureElement
                    .ContainsElements
                    .SelectMany(rel => rel.RelatedElements);
                foreach (var element in containedElements)
                {
                    Console.WriteLine(string.Format("{0}    ->{1} [{2}]", GetIndent(level), element.Name, element.GetType().Name));
                }
                
            }

            //利用 IfcRelAggregares 获取空间结构元素的空间分解
            foreach (var item in obj.IsDecomposedBy.SelectMany(r => r.RelatedObjects))
            {
                PrintHierarchy(item, level + 1);
            }
        }

        private static string GetIndent(int level)
        {
            var indent = "";
            for (int i = 0; i < level; i++)
                indent += "  ";
            return indent;
        }
    }
}
