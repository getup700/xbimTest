# xbim

# 数据结构

```继承关系
IfcRoot
	IfcObjectDefinition
		IfcObject
			IfcProduct
				IfcElement
					IfcBuildingElement
						IfcWall
```
### IfcRoot
所有ifc实体的顶级父类
```
public interface IIfcRoot : IPersistEntity, IPersist
{
    IfcGloballyUniqueId GlobalId { get; set; }

    IIfcOwnerHistory OwnerHistory { get; set; }//实体创建、修改历史

    IfcLabel? Name { get; set; }

    IfcText? Description { get; set; }
}
```
### IfcObjectDefinition : IfcRoot
表示所有对象定义和关系的基础类。它是所有物理对象和语义对象（如构件、空间）的共同父类。
- `IfcObject`（表示具体的对象）。
- `IfcContext`（用于定义项目上下文，如`IfcProject`）。
### IfcObject : IfcObjectDefinition
表示建筑元素的通用抽象。实际建筑构件（如墙、门、梁等）都会从`IfcObject`继承。
`ObjectType`：用于指定对象的具体类型。
### IfcProduct : IfcObject
代表物理建筑元素的基类，如墙、门、梁等。
- `ObjectPlacement`：定义构件在空间中的位置。
- `Representation`：几何表示，如3D模型、形状。
### IfcElement : IfcProduct
是建筑构件（如墙、梁、柱等）的基类，包含关于构件几何形状和位置的定义。
`Tag`：用于标识构件的标签，通常用于标注编号。
# 增删查改
### IfcStore
管理和处理 IFC 模型数据的核心类之一。充当 IFC 模型的存储和管理容器，加载、保存、查询、编辑和导出 IFC 文件。`IfcStore` 在 xBIM 中起到了类似数据库的作用，提供了一个易于使用的接口来访问和操作模型中的数据。
# 初始信息
### IfcApplication
描述创建或修改 IFC 文件的应用程序信息。包含关于软件应用程序的详细信息，包括开发者、应用名称、版本等。这在 BIM（建筑信息模型）数据的交换和管理中非常重要，因为它帮助追踪谁使用了哪些软件来创建或修改模型。