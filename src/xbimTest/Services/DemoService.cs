using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.IO;

namespace xbimTest.Services
{
    internal class DemoService
    {

        private readonly XbimEditorCredentials credentials;

        public DemoService(XbimEditorCredentials credentials)
        {
            this.credentials = credentials;
        }

        public void Create(string path)
        {
            using var model = IfcStore.Create(credentials, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);
            using var txn = model.BeginTransaction();
            //创建模型前应该先创建项目
            var project = model.Instances.New<IfcProject>(p => p.Name = "Basic Creation");
            //定义基本的单位  SIUnitsUK 为英制单位 
            project.Initialize(ProjectUnits.SIUnitsUK);

            //创建简单的对象并使用lambda初始值设定名称
            var wall = model.Instances.New<IfcWall>(w => w.Name = "The very first wall");

            model.Instances.New<IfcRelDefinesByProperties>(rel =>
            {
                rel.RelatedObjects.Add(wall);
                rel.RelatingPropertyDefinition = model.Instances.New<IfcPropertySet>(pset =>
                {
                    pset.Name = "Basic set of properties";
                    pset.HasProperties.AddRange(new[] {
                    model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "Text property";
                        p.NominalValue = new IfcText("Any arbitrary text you like");
                    }),
                    model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "Length property";
                        p.NominalValue = new IfcLengthMeasure(56.0);
                    }),
                    model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "Number property";
                        p.NominalValue = new IfcNumericMeasure(789.2);
                    }),
                    model.Instances.New<IfcPropertySingleValue>(p =>
                    {
                        p.Name = "Logical property";
                        p.NominalValue = new IfcLogical(true);
                    })
                });
                });
            });

            txn.Commit();

            model.SaveAs(path);
        }
    }
}
