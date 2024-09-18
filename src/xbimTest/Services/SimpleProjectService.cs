using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.Ifc4.ActorResource;
using Xbim.Ifc4.DateTimeResource;
using Xbim.Ifc4.ExternalReferenceResource;
using Xbim.Ifc4.GeometricConstraintResource;
using Xbim.Ifc4.GeometricModelResource;
using Xbim.Ifc4.GeometryResource;
using Xbim.Ifc4.Interfaces;
using Xbim.Ifc4.Kernel;
using Xbim.Ifc4.MaterialResource;
using Xbim.Ifc4.MeasureResource;
using Xbim.Ifc4.PresentationOrganizationResource;
using Xbim.Ifc4.ProductExtension;
using Xbim.Ifc4.ProfileResource;
using Xbim.Ifc4.PropertyResource;
using Xbim.Ifc4.QuantityResource;
using Xbim.Ifc4.RepresentationResource;
using Xbim.Ifc4.SharedBldgElements;
using Xbim.IO;
using xbimTest.Extensions;

namespace xbimTest.Services;

/// <summary>
/// 创建一个ifc示例项目，项目包含一面墙
/// </summary>
internal class SimpleProjectService
{
    private readonly ILogger<SimpleProjectService> logger;

    public SimpleProjectService(ILogger<SimpleProjectService> logger)
    {
        this.logger = logger;
    }

    public void BuildingSimpleIfcProject(string fullFileName = null)
    {
        fullFileName ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "HelloIfcProject.ifc");
        logger.LogDebug(fullFileName);
        //create model
        using var model = CreateAndInitModel("HelloProject");
        if (model == null)
        {
            return;
        }

        //create building
        var building = CreateBuilding(model, "HelloBuilding");

        //create wall
        var wall = CreateWall(model, 4000, 300, 2400);
        if (wall != null)
        {
            AddPropertiesToWall(model, wall);
            logger.LogTrace(nameof(AddPropertiesToWall));
        }

        //store the wall in the building
        model.NewTransaction(model =>
        {
            building.AddElement(wall);
        }, "Add Wall");
        logger.LogTrace("Create building");

        //saveas model
        try
        {
            logger.LogTrace("Standard Wall successfully created....");
            model.SaveAs(fullFileName, StorageType.Ifc);
            logger.LogTrace($"{fullFileName} has been successfully written");
        }
        catch (Exception e)
        {
            logger.LogError("Failed to save HelloIfc.ifc");
            logger.LogError(e.Message);
        }


    }

    private IfcBuilding CreateBuilding(IfcStore model, string name)
    {
        IfcBuilding building = null;
        model.NewTransaction(model =>
        {
            building = model.Instances.New<IfcBuilding>();
            building.Name = name;
            building.CompositionType = IfcElementCompositionEnum.ELEMENT;

            var localPlacement = model.Instances.New<IfcLocalPlacement>();
            building.ObjectPlacement = localPlacement;
            var placement = model.Instances.New<IfcAxis2Placement3D>();
            localPlacement.RelativePlacement = placement;

            placement.Location = model.Instances.New<IfcCartesianPoint>(p => p.SetXYZ(0, 0, 0));
            //get the project there should only be one and it should exist
            var project = model.Instances.OfType<IfcProject>().FirstOrDefault();
            project?.AddBuilding(building);
        }, "Create building");
        return building;
    }

    /// <summary>
    /// Sets up the basic parameters any model must provide, units, ownership etc
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <returns></returns>
    private IfcStore CreateAndInitModel(string projectName)
    {
        //first we need to set up some credentials for ownership of data in the new model
        var credentials = new XbimEditorCredentials
        {
            ApplicationDevelopersName = "xbim developer",
            ApplicationFullName = "Hello Wall Application",
            ApplicationIdentifier = "HelloWall.exe",
            ApplicationVersion = "1.0",
            EditorsFamilyName = "Team",
            EditorsGivenName = "xbim",
            EditorsOrganisationName = "xbim developer"
        };
        //now we can create an IfcStore, it is in Ifc4 format and will be held in memory rather than in a database
        //database is normally better in performance terms if the model is large >50MB of Ifc or if robust transactions are required

        var model = IfcStore.Create(credentials, XbimSchemaVersion.Ifc4, XbimStoreType.InMemoryModel);

        //Begin a transaction as all changes to a model are ACID
        using (var txn = model.BeginTransaction("Initialise Model"))
        {

            //create a project
            var project = model.Instances.New<IfcProject>();
            //set the units to SI (mm and metres)
            project.Initialize(ProjectUnits.SIUnitsUK);
            project.Name = projectName;
            //now commit the changes, else they will be rolled back at the end of the scope of the using statement
            txn.Commit();
        }
        return model;

    }

    /// <summary>
    /// This creates a wall and it's geometry, many geometric representations are possible and extruded rectangular footprint is chosen 
    /// as this is commonly used for standard case walls
    /// </summary>
    /// <param name="store"></param>
    /// <param name="length">Length of the rectangular footprint</param>
    /// <param name="width">Width of the rectangular footprint (width of the wall)</param>
    /// <param name="height">Height to extrude the wall, extrusion is vertical</param>
    /// <returns></returns>
    private IfcWallStandardCase CreateWall(IfcStore store, double length, double width, double height)
    {
        //begin a transaction
        using (var txn = store.BeginTransaction("Create Wall"))
        {
            //创建墙体实例
            var wall = store.Instances.New<IfcWallStandardCase>();
            wall.Name = "A Standard rectangular wall";

            //represent wall as a rectangular profile
            var rectProf = store.Instances.New<IfcRectangleProfileDef>();
            rectProf.ProfileType = IfcProfileTypeEnum.AREA;
            rectProf.XDim = width;
            rectProf.YDim = length;

            var insertPoint = store.Instances.New<IfcCartesianPoint>();
            insertPoint.SetXY(0, 400); //insert at arbitrary position
            rectProf.Position = store.Instances.New<IfcAxis2Placement2D>();
            rectProf.Position.Location = insertPoint;

            //model as a swept area solid
            var body = store.Instances.New<IfcExtrudedAreaSolid>();
            body.Depth = height;
            body.SweptArea = rectProf;
            body.ExtrudedDirection = store.Instances.New<IfcDirection>();
            body.ExtrudedDirection.SetXYZ(0, 0, 1);

            //parameters to insert the geometry in the model
            var origin = store.Instances.New<IfcCartesianPoint>();
            origin.SetXYZ(0, 0, 0);
            body.Position = store.Instances.New<IfcAxis2Placement3D>();
            body.Position.Location = origin;

            //Create a Definition shape to hold the geometry
            var shape = store.Instances.New<IfcShapeRepresentation>();
            var modelContext = store.Instances.OfType<IfcGeometricRepresentationContext>().FirstOrDefault();
            shape.ContextOfItems = modelContext;
            shape.RepresentationType = "SweptSolid";
            shape.RepresentationIdentifier = "Body";
            shape.Items.Add(body);

            //Create a Product Definition and add the model geometry to the wall
            var rep = store.Instances.New<IfcProductDefinitionShape>();
            rep.Representations.Add(shape);
            wall.Representation = rep;

            //now place the wall into the model
            var lp = store.Instances.New<IfcLocalPlacement>();
            var ax3D = store.Instances.New<IfcAxis2Placement3D>();
            ax3D.Location = origin;
            ax3D.RefDirection = store.Instances.New<IfcDirection>();
            ax3D.RefDirection.SetXYZ(0, 1, 0);
            ax3D.Axis = store.Instances.New<IfcDirection>();
            ax3D.Axis.SetXYZ(0, 0, 1);
            lp.RelativePlacement = ax3D;
            wall.ObjectPlacement = lp;

            // Where Clause: The IfcWallStandard relies on the provision of an IfcMaterialLayerSetUsage 
            var ifcMaterialLayerSetUsage = store.Instances.New<IfcMaterialLayerSetUsage>();
            var ifcMaterialLayerSet = store.Instances.New<IfcMaterialLayerSet>();
            var ifcMaterialLayer = store.Instances.New<IfcMaterialLayer>();
            ifcMaterialLayer.LayerThickness = 10;
            ifcMaterialLayerSet.MaterialLayers.Add(ifcMaterialLayer);
            ifcMaterialLayerSetUsage.ForLayerSet = ifcMaterialLayerSet;
            ifcMaterialLayerSetUsage.LayerSetDirection = IfcLayerSetDirectionEnum.AXIS2;
            ifcMaterialLayerSetUsage.DirectionSense = IfcDirectionSenseEnum.NEGATIVE;
            ifcMaterialLayerSetUsage.OffsetFromReferenceLine = 150;

            // Add material to wall
            var material = store.Instances.New<IfcMaterial>();
            material.Name = "some material";
            var ifcRelAssociatesMaterial = store.Instances.New<IfcRelAssociatesMaterial>();
            ifcRelAssociatesMaterial.RelatingMaterial = material;
            ifcRelAssociatesMaterial.RelatedObjects.Add(wall);
            ifcRelAssociatesMaterial.RelatingMaterial = ifcMaterialLayerSetUsage;

            // IfcPresentationLayerAssignment is required for CAD presentation in IfcWall or IfcWallStandardCase
            var ifcPresentationLayerAssignment = store.Instances.New<IfcPresentationLayerAssignment>();
            ifcPresentationLayerAssignment.Name = "some ifcPresentationLayerAssignment";
            ifcPresentationLayerAssignment.AssignedItems.Add(shape);

            // linear segment as IfcPolyline with two points is required for IfcWall
            var ifcPolyline = store.Instances.New<IfcPolyline>();
            var startPoint = store.Instances.New<IfcCartesianPoint>();
            startPoint.SetXY(0, 0);
            var endPoint = store.Instances.New<IfcCartesianPoint>();
            endPoint.SetXY(4000, 0);
            ifcPolyline.Points.Add(startPoint);
            ifcPolyline.Points.Add(endPoint);

            var shape2D = store.Instances.New<IfcShapeRepresentation>();
            shape2D.ContextOfItems = modelContext;
            shape2D.RepresentationIdentifier = "Axis";
            shape2D.RepresentationType = "Curve2D";
            shape2D.Items.Add(ifcPolyline);
            rep.Representations.Add(shape2D);
            txn.Commit();
            return wall;
        }

    }

    /// <summary>
    /// Add some properties to the wall,
    /// </summary>
    /// <param name="model">XbimModel</param>
    /// <param name="wall"></param>
    private void AddPropertiesToWall(IfcStore model, IfcWallStandardCase wall)
    {
        using (var txn = model.BeginTransaction("Create Wall"))
        {
            CreateElementQuantity(model, wall);
            CreateSimpleProperty(model, wall);
            txn.Commit();
        }
    }

    static void CreateSimpleProperty(IfcStore model, IfcWallStandardCase wall)
    {
        var ifcPropertySingleValue = model.Instances.New<IfcPropertySingleValue>(psv =>
        {
            psv.Name = "IfcPropertySingleValue:Time";
            psv.Description = "";
            psv.NominalValue = new IfcTimeMeasure(150.0);
            psv.Unit = model.Instances.New<IfcSIUnit>(siu =>
            {
                siu.UnitType = IfcUnitEnum.TIMEUNIT;
                siu.Name = IfcSIUnitName.SECOND;
            });
        });
        var ifcPropertyEnumeratedValue = model.Instances.New<IfcPropertyEnumeratedValue>(pev =>
        {
            pev.Name = "IfcPropertyEnumeratedValue:Music";
            pev.EnumerationReference = model.Instances.New<IfcPropertyEnumeration>(pe =>
            {
                pe.Name = "Notes";
                pe.EnumerationValues.Add(new IfcLabel("Do"));
                pe.EnumerationValues.Add(new IfcLabel("Re"));
                pe.EnumerationValues.Add(new IfcLabel("Mi"));
                pe.EnumerationValues.Add(new IfcLabel("Fa"));
                pe.EnumerationValues.Add(new IfcLabel("So"));
                pe.EnumerationValues.Add(new IfcLabel("La"));
                pe.EnumerationValues.Add(new IfcLabel("Ti"));
            });
            pev.EnumerationValues.Add(new IfcLabel("Do"));
            pev.EnumerationValues.Add(new IfcLabel("Re"));
            pev.EnumerationValues.Add(new IfcLabel("Mi"));

        });
        var ifcPropertyBoundedValue = model.Instances.New<IfcPropertyBoundedValue>(pbv =>
        {
            pbv.Name = "IfcPropertyBoundedValue:Mass";
            pbv.Description = "";
            pbv.UpperBoundValue = new IfcMassMeasure(5000.0);
            pbv.LowerBoundValue = new IfcMassMeasure(1000.0);
            pbv.Unit = model.Instances.New<IfcSIUnit>(siu =>
            {
                siu.UnitType = IfcUnitEnum.MASSUNIT;
                siu.Name = IfcSIUnitName.GRAM;
                siu.Prefix = IfcSIPrefix.KILO;
            });
        });

        var definingValues = new List<IfcReal>
        {
            new IfcReal(100.0),
            new IfcReal(200.0),
            new IfcReal(400.0),
            new IfcReal(800.0),
            new IfcReal(1600.0),
            new IfcReal(3200.0),
        };
        var definedValues = new List<IfcReal>
        {
            new IfcReal(20.0),
            new IfcReal(42.0),
            new IfcReal(46.0),
            new IfcReal(56.0),
            new IfcReal(60.0),
            new IfcReal(65.0),
        };
        var ifcPropertyTableValue = model.Instances.New<IfcPropertyTableValue>(ptv =>
        {
            ptv.Name = "IfcPropertyTableValue:Sound";
            foreach (var item in definingValues)
            {
                ptv.DefiningValues.Add(item);
            }
            foreach (var item in definedValues)
            {
                ptv.DefinedValues.Add(item);
            }
            ptv.DefinedUnit = model.Instances.New<IfcContextDependentUnit>(cd =>
            {
                cd.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
                {
                    de.LengthExponent = 0;
                    de.MassExponent = 0;
                    de.TimeExponent = 0;
                    de.ElectricCurrentExponent = 0;
                    de.ThermodynamicTemperatureExponent = 0;
                    de.AmountOfSubstanceExponent = 0;
                    de.LuminousIntensityExponent = 0;
                });
                cd.UnitType = IfcUnitEnum.FREQUENCYUNIT;
                cd.Name = "dB";
            });
        });

        var listValues = new List<IfcLabel>
        {
            new IfcLabel("Red"),
            new IfcLabel("Green"),
            new IfcLabel("Blue"),
            new IfcLabel("Pink"),
            new IfcLabel("White"),
            new IfcLabel("Black"),
        };
        var ifcPropertyListValue = model.Instances.New<IfcPropertyListValue>(plv =>
        {
            plv.Name = "IfcPropertyListValue:Colours";
            foreach (var item in listValues)
            {
                plv.ListValues.Add(item);
            }
        });

        var ifcMaterial = model.Instances.New<IfcMaterial>(m =>
        {
            m.Name = "Brick";
        });
        var ifcPrValueMaterial = model.Instances.New<IfcPropertyReferenceValue>(prv =>
        {
            prv.Name = "IfcPropertyReferenceValue:Material";
            prv.PropertyReference = ifcMaterial;
        });


        var ifcMaterialList = model.Instances.New<IfcMaterialList>(ml =>
        {
            ml.Materials.Add(ifcMaterial);
            ml.Materials.Add(model.Instances.New<IfcMaterial>(m => { m.Name = "Cavity"; }));
            ml.Materials.Add(model.Instances.New<IfcMaterial>(m => { m.Name = "Block"; }));
        });


        var ifcMaterialLayer = model.Instances.New<IfcMaterialLayer>(ml =>
        {
            ml.Material = ifcMaterial;
            ml.LayerThickness = 100.0;
        });
        var ifcPrValueMatLayer = model.Instances.New<IfcPropertyReferenceValue>(prv =>
        {
            prv.Name = "IfcPropertyReferenceValue:MaterialLayer";
            prv.PropertyReference = ifcMaterialLayer;
        });

        var ifcDocumentReference = model.Instances.New<IfcDocumentReference>(dr =>
        {
            dr.Name = "Document";
            dr.Location = "c://Documents//TheDoc.Txt";
        });
        var ifcPrValueRef = model.Instances.New<IfcPropertyReferenceValue>(prv =>
        {
            prv.Name = "IfcPropertyReferenceValue:Document";
            prv.PropertyReference = ifcDocumentReference;
        });

        var ifcTimeSeries = model.Instances.New<IfcRegularTimeSeries>(ts =>
        {
            ts.Name = "Regular Time Series";
            ts.Description = "Time series of events";
            ts.StartTime = new IfcDateTime("2015-02-14T12:01:01");
            ts.EndTime = new IfcDateTime("2015-05-15T12:01:01");
            ts.TimeSeriesDataType = IfcTimeSeriesDataTypeEnum.CONTINUOUS;
            ts.DataOrigin = IfcDataOriginEnum.MEASURED;
            ts.TimeStep = 604800; //7 days in secs
        });

        var ifcPrValueTimeSeries = model.Instances.New<IfcPropertyReferenceValue>(prv =>
        {
            prv.Name = "IfcPropertyReferenceValue:TimeSeries";
            prv.PropertyReference = ifcTimeSeries;
        });

        var ifcAddress = model.Instances.New<IfcPostalAddress>(a =>
        {
            a.InternalLocation = "Room 101";
            a.AddressLines.AddRange(new[] { new IfcLabel("12 New road"), new IfcLabel("DoxField") });
            a.Town = "Sunderland";
            a.PostalCode = "DL01 6SX";
        });
        var ifcPrValueAddress = model.Instances.New<IfcPropertyReferenceValue>(prv =>
        {
            prv.Name = "IfcPropertyReferenceValue:Address";
            prv.PropertyReference = ifcAddress;
        });
        var ifcTelecomAddress = model.Instances.New<IfcTelecomAddress>(a =>
        {
            a.TelephoneNumbers.Add(new IfcLabel("01325 6589965"));
            a.ElectronicMailAddresses.Add(new IfcLabel("bob@bobsworks.com"));
        });
        var ifcPrValueTelecom = model.Instances.New<IfcPropertyReferenceValue>(prv =>
        {
            prv.Name = "IfcPropertyReferenceValue:Telecom";
            prv.PropertyReference = ifcTelecomAddress;
        });



        //lets create the IfcElementQuantity
        var ifcPropertySet = model.Instances.New<IfcPropertySet>(ps =>
        {
            ps.Name = "Test:IfcPropertySet";
            ps.Description = "Property Set";
            ps.HasProperties.Add(ifcPropertySingleValue);
            ps.HasProperties.Add(ifcPropertyEnumeratedValue);
            ps.HasProperties.Add(ifcPropertyBoundedValue);
            ps.HasProperties.Add(ifcPropertyTableValue);
            ps.HasProperties.Add(ifcPropertyListValue);
            ps.HasProperties.Add(ifcPrValueMaterial);
            ps.HasProperties.Add(ifcPrValueMatLayer);
            ps.HasProperties.Add(ifcPrValueRef);
            ps.HasProperties.Add(ifcPrValueTimeSeries);
            ps.HasProperties.Add(ifcPrValueAddress);
            ps.HasProperties.Add(ifcPrValueTelecom);
        });

        //need to create the relationship
        model.Instances.New<IfcRelDefinesByProperties>(rdbp =>
        {
            rdbp.Name = "Property Association";
            rdbp.Description = "IfcPropertySet associated to wall";
            rdbp.RelatedObjects.Add(wall);
            rdbp.RelatingPropertyDefinition = ifcPropertySet;
        });
    }

    private void CreateElementQuantity(IfcStore model, IfcWallStandardCase wall)
    {
        //Create a IfcElementQuantity
        //first we need a IfcPhysicalSimpleQuantity,first will use IfcQuantityLength
        var ifcQuantityArea = model.Instances.New<IfcQuantityLength>(qa =>
        {
            qa.Name = "IfcQuantityArea:Area";
            qa.Description = "";
            qa.Unit = model.Instances.New<IfcSIUnit>(siu =>
            {
                siu.UnitType = IfcUnitEnum.LENGTHUNIT;
                siu.Prefix = IfcSIPrefix.MILLI;
                siu.Name = IfcSIUnitName.METRE;
            });
            qa.LengthValue = 100.0;

        });


        //next quantity IfcQuantityCount using IfcContextDependentUnit
        var ifcContextDependentUnit = model.Instances.New<IfcContextDependentUnit>(cd =>
        {
            cd.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
            {
                de.LengthExponent = 1;
                de.MassExponent = 0;
                de.TimeExponent = 0;
                de.ElectricCurrentExponent = 0;
                de.ThermodynamicTemperatureExponent = 0;
                de.AmountOfSubstanceExponent = 0;
                de.LuminousIntensityExponent = 0;
            });
            cd.UnitType = IfcUnitEnum.LENGTHUNIT;
            cd.Name = "Elephants";
        });
        var ifcQuantityCount = model.Instances.New<IfcQuantityCount>(qc =>
        {
            qc.Name = "IfcQuantityCount:Elephant";
            qc.CountValue = 12;
            qc.Unit = ifcContextDependentUnit;
        });


        //next quantity IfcQuantityLength using IfcConversionBasedUnit
        var ifcConversionBasedUnit = model.Instances.New<IfcConversionBasedUnit>(cbu =>
        {
            cbu.ConversionFactor = model.Instances.New<IfcMeasureWithUnit>(mu =>
            {
                mu.ValueComponent = new IfcRatioMeasure(25.4);
                mu.UnitComponent = model.Instances.New<IfcSIUnit>(siu =>
                {
                    siu.UnitType = IfcUnitEnum.LENGTHUNIT;
                    siu.Prefix = IfcSIPrefix.MILLI;
                    siu.Name = IfcSIUnitName.METRE;
                });
            });
            cbu.Dimensions = model.Instances.New<IfcDimensionalExponents>(de =>
            {
                de.LengthExponent = 1;
                de.MassExponent = 0;
                de.TimeExponent = 0;
                de.ElectricCurrentExponent = 0;
                de.ThermodynamicTemperatureExponent = 0;
                de.AmountOfSubstanceExponent = 0;
                de.LuminousIntensityExponent = 0;
            });
            cbu.UnitType = IfcUnitEnum.LENGTHUNIT;
            cbu.Name = "Inch";
        });

        var ifcQuantityLength = model.Instances.New<IfcQuantityLength>(qa =>
        {
            qa.Name = "IfcQuantityLength:Length";
            qa.Description = "";
            qa.Unit = ifcConversionBasedUnit;
            qa.LengthValue = 24.0;
        });

        //lets create the IfcElementQuantity
        var ifcElementQuantity = model.Instances.New<IfcElementQuantity>(eq =>
        {
            eq.Name = "Test:IfcElementQuantity";
            eq.Description = "Measurement quantity";
            eq.Quantities.Add(ifcQuantityArea);
            eq.Quantities.Add(ifcQuantityCount);
            eq.Quantities.Add(ifcQuantityLength);
        });

        //need to create the relationship
        model.Instances.New<IfcRelDefinesByProperties>(rdbp =>
        {
            rdbp.Name = "Area Association";
            rdbp.Description = "IfcElementQuantity associated to wall";
            rdbp.RelatedObjects.Add(wall);
            rdbp.RelatingPropertyDefinition = ifcElementQuantity;
        });
    }

}
