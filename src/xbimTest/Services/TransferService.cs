using Microsoft.Extensions.Logging;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;

namespace xbimTest.Services
{
    internal class TransferService
    {
        private readonly XbimEditorCredentials credentials;
        private readonly ILogger<TransferService> logger;

        public TransferService(XbimEditorCredentials credentials, ILogger<TransferService> logger)
        {
            this.credentials = credentials;
            this.logger = logger;
        }

        public void Export(string fileName, string exportPath)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Spaces");

            var areaFormat = workbook.CreateDataFormat();
            var areaStyle = workbook.CreateCellStyle();
            areaStyle.DataFormat = areaFormat.GetFormat("# ##0.00 [$m²]");

            var volumeFormat = workbook.CreateDataFormat();
            var volumeFormatId = volumeFormat.GetFormat("# ##0.00 [$m³]");
            var volumeStyle = workbook.CreateCellStyle();
            volumeStyle.DataFormat = volumeFormatId;

            using var model = IfcStore.Open(fileName);
            var spaces = model.Instances.OfType<IIfcSpace>().ToList();

            //sheet.CreateRow(0).CreateCell(0)
            //    .SetCellValue($"Space Report ({spaces.Count} spaces)");
            var headerRow = sheet.GetRow(0) ?? sheet.CreateRow(0);
            headerRow.CreateCell(0).SetCellValue("Name");
            headerRow.CreateCell(1).SetCellValue("Name");
            headerRow.CreateCell(2).SetCellValue("Area");
            headerRow.CreateCell(3).SetCellValue("Volumn");
            foreach (var space in spaces)
            {
                WriteSpaceRow(space, sheet, areaStyle, volumeStyle);
            }

            using var stream = File.Create(exportPath);
            workbook.Write(stream);
            stream.Close();
        }

        private static void WriteSpaceRow(IIfcSpace space, ISheet sheet, ICellStyle areaStyle, ICellStyle volumeStyle)
        {
            var row = sheet.CreateRow(sheet.LastRowNum + 1);

            var name = space.Name;
            row.CreateCell(0).SetCellValue(name);

            var floor = GetFloor(space);
            row.CreateCell(1).SetCellValue(floor?.Name);

            var area = GetArea(space);
            if (area != null)
            {
                var cell = row.CreateCell(2);
                cell.CellStyle = areaStyle;

                if (area.UnderlyingSystemType == typeof(double))
                    cell.SetCellValue((double)(area.Value));
                else
                    cell.SetCellValue(area.ToString());
            }

            var volume = GetVolume(space);
            if (volume != null)
            {
                var cell = row.CreateCell(3);
                cell.CellStyle = volumeStyle;
                if (volume.UnderlyingSystemType == typeof(double))
                    cell.SetCellValue((double)(volume.Value));
                else
                    cell.SetCellValue(volume.ToString());
            }
        }

        private static IIfcBuildingStorey GetFloor(IIfcSpace space)
        {
            return
                //得到这个模型空间所有关系
                space.Decomposes
                //选择分解对象 （这些可能是其他空间或建筑层)
                .Select(r => r.RelatingObject)
                //建筑楼层
                .OfType<IIfcBuildingStorey>()
                .FirstOrDefault();
        }

        private static IIfcValue GetArea(IIfcProduct product)
        {
            //尝试先从数量中获取
            var area =
                //获取可以定义属性和数量集的所有关系
                product.IsDefinedBy
                //在所有属性和数量集之间搜索。
                //您可能还希望按名称搜索特定数量
                .SelectMany(r => r.RelatingPropertyDefinition.PropertySetDefinitions)
                //数量集合
                .OfType<IIfcElementQuantity>()
                //从数量集获取所有数量
                .SelectMany(qset => qset.Quantities)
                //我们只对面积感兴趣 
                .OfType<IIfcQuantityArea>()
                //我们将采取第一个。显然有一个以上的面积属性
                //所以, 要检查的名称。但是,我们将保持它简单的这个例子。
                .FirstOrDefault()?
                .AreaValue;
            if (area != null)
                return area;
            //从属性中获取值
            return GetProperty(product, "Area");
        }

        private static IIfcValue GetVolume(IIfcProduct product)
        {
            var volume = product.IsDefinedBy
                .SelectMany(r => r.RelatingPropertyDefinition.PropertySetDefinitions)
                .OfType<IIfcElementQuantity>()
                .SelectMany(qset => qset.Quantities)
                .OfType<IIfcQuantityVolume>()
                .FirstOrDefault()?.VolumeValue;
            if (volume != null)
                return volume;
            return GetProperty(product, "Volume");
        }

        private static IIfcValue GetProperty(IIfcProduct product, string name)
        {
            return
                //获取可以定义属性和数量集的所有关系
                product.IsDefinedBy
                //在所有属性和数量集之间搜索。您可能还希望在特定属性集中搜索
                .SelectMany(r => r.RelatingPropertyDefinition.PropertySetDefinitions)
                //在这种情况下, 只考虑属性集。
                .OfType<IIfcPropertySet>()
                //从所有属性集中获取所有属性
                .SelectMany(pset => pset.HasProperties)
                //只允许考虑单个值属性。还有枚举属性,
                //表属性、引用属性、复杂属性和其他
                .OfType<IIfcPropertySingleValue>()
                .Where(p =>
                    string.Equals(p.Name, name, System.StringComparison.OrdinalIgnoreCase) ||
                    p.Name.ToString().ToLower().Contains(name.ToLower()))
                .FirstOrDefault()?.NominalValue;
        }
    }
}
