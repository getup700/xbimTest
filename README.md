# xbim

# ���ݽṹ

```�̳й�ϵ
IfcRoot
	IfcObjectDefinition
		IfcObject
			IfcProduct
				IfcElement
					IfcBuildingElement
						IfcWall
```
### IfcRoot
����ifcʵ��Ķ�������
```
public interface IIfcRoot : IPersistEntity, IPersist
{
    IfcGloballyUniqueId GlobalId { get; set; }

    IIfcOwnerHistory OwnerHistory { get; set; }//ʵ�崴�����޸���ʷ

    IfcLabel? Name { get; set; }

    IfcText? Description { get; set; }
}
```
### IfcObjectDefinition : IfcRoot
��ʾ���ж�����͹�ϵ�Ļ����ࡣ������������������������繹�����ռ䣩�Ĺ�ͬ���ࡣ
- `IfcObject`����ʾ����Ķ��󣩡�
- `IfcContext`�����ڶ�����Ŀ�����ģ���`IfcProject`����
### IfcObject : IfcObjectDefinition
��ʾ����Ԫ�ص�ͨ�ó���ʵ�ʽ�����������ǽ���š����ȣ������`IfcObject`�̳С�
`ObjectType`������ָ������ľ������͡�
### IfcProduct : IfcObject
����������Ԫ�صĻ��࣬��ǽ���š����ȡ�
- `ObjectPlacement`�����幹���ڿռ��е�λ�á�
- `Representation`�����α�ʾ����3Dģ�͡���״��
### IfcElement : IfcProduct
�ǽ�����������ǽ���������ȣ��Ļ��࣬�������ڹ���������״��λ�õĶ��塣
`Tag`�����ڱ�ʶ�����ı�ǩ��ͨ�����ڱ�ע��š�
# ��ɾ���
### IfcStore
����ʹ��� IFC ģ�����ݵĺ�����֮һ���䵱 IFC ģ�͵Ĵ洢�͹������������ء����桢��ѯ���༭�͵��� IFC �ļ���`IfcStore` �� xBIM �������������ݿ�����ã��ṩ��һ������ʹ�õĽӿ������ʺͲ���ģ���е����ݡ�
# ��ʼ��Ϣ
### IfcApplication
�����������޸� IFC �ļ���Ӧ�ó�����Ϣ�������������Ӧ�ó������ϸ��Ϣ�����������ߡ�Ӧ�����ơ��汾�ȡ����� BIM��������Ϣģ�ͣ����ݵĽ����͹����зǳ���Ҫ����Ϊ������׷��˭ʹ������Щ������������޸�ģ�͡�