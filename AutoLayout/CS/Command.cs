using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RApplication = Autodesk.Revit.ApplicationServices.Application;
using Exceptions = Autodesk.Revit.Exceptions;

namespace Revit.SDK.Samples.AutoLayout.CS
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public class FamilyFilter : ISelectionFilter //implement filter
        {
            bool ISelectionFilter.AllowElement(Element elem)
            {
                // Allow selecting if type Beacon Family
                if (elem.Name == "Generic - 8\"")
                    return true;
                else
                    return false;
                /* Now only the Beacon Family is allowed to be selected       
                If more beacons types are desired, insert above */
            }

            bool ISelectionFilter.AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<Reference> picked = uidoc.Selection.PickObjects(ObjectType.Element, new FamilyFilter(), "Pick wall faces to select walls");

            //TaskDialog.Show("revit", string.Format("{0} objects have been picked back.", picked.Count));

            
            string str = "Coordinate: ";
            foreach (Reference r in picked)
            {                        
                Element e = doc.GetElement(r);
                Location position = e.Location;
                LocationCurve positionCurve = position as LocationCurve;
                Curve curve = positionCurve.Curve;
                XYZ endPoint0 = curve.GetEndPoint(0);
                XYZ endPoint1 = curve.GetEndPoint(1);
                str = endPoint0.X + " " + endPoint0.Y + " " + endPoint0.Z + "\n";
                str += endPoint1.X + " " + endPoint1.Y + " " + endPoint1.Z + "\n";
                TaskDialog.Show("revit", str);
                           
            }

            Family family = null;
            FamilySymbol symbol = null;
            
            FamilyInstance familyInstance = null;

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Load Family");
                /*
                doc.LoadFamily("C:/Users/jane0/Desktop/IES_revit_plugin/Beacon NEW.rfa", out family);
                doc.LoadFamily("C:/Autodesk/WI/Autodesk Revit 2017/x64/RVT/Program Files/Autodesk/Root/Samples/basic_sample_family.rfa", out family);
                if (family == null)
                    TaskDialog.Show("Revit", "family == null");
                else
                    TaskDialog.Show("Revit", "family placement type: " + family.FamilyPlacementType);
                if (!doc.LoadFamilySymbol("C:/Autodesk/WI/Autodesk Revit 2017/x64/RVT/Program Files/Autodesk/Root/Samples/basic_sample_family.rfa", "0610 x 0160mm", out symbol))
                */

                if (!doc.LoadFamilySymbol("C:/Users/jane0/Desktop/IES_revit_plugin/Beacon NEW.rfa", "High Ceiling", out symbol))
                {
                    TaskDialog.Show("Revit", "Can't load the family file.");
                }
                if (symbol == null)
                    TaskDialog.Show("Revit", "symbol == null");
                //famSym.Family.FamilyPlacementType is WorkPlaceBased
                tx.Commit();
            }
            /*
            using (Transaction t = new Transaction(doc, "Create instance"))
            {
                t.Start();
                XYZ point = new XYZ(0, 0, 0);
                if (!symbol.IsActive)
                {
                    symbol.Activate();
                    doc.Regenerate();
                }

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_Ceilings);
                FamilySymbol ceilingSym = collector.FirstElement() as FamilySymbol;

                
                if (collector.Count() == 0)
                    TaskDialog.Show("revit", "collector is empty");

                Ceiling ceiling = collector.FirstElement() as Ceiling;

                if (ceiling == null)
                    TaskDialog.Show("revit", "ceiling is null");
                // The only way to get a Face to use with this NewFamilyInstance overload
                // is from Element.Geometry with ComputeReferences turned on

                Face face = null;
                Options geomOptions = new Options();
                geomOptions.ComputeReferences = true;
                GeometryElement ceilingGeom = ceiling.get_Geometry(geomOptions);
                foreach (GeometryObject geomObj in ceilingGeom)
                {
                    Solid geomSolid = geomObj as Solid;
                    if (null != geomSolid)
                    {
                        foreach (Face geomFace in geomSolid.Faces)
                        {
                            face = geomFace;
                            break;
                        }
                        break;
                    }
                }

                // Get the center of the wall 
                BoundingBoxUV bboxUV = face.GetBoundingBox();
                UV center = (bboxUV.Max + bboxUV.Min) / 2.0;
                XYZ location = face.Evaluate(center);
                XYZ normal = face.ComputeNormal(center);
                XYZ refDir = normal.CrossProduct(XYZ.BasisZ);

                FamilyInstance instance = doc.Create.NewFamilyInstance(face, location, refDir, symbol);


                familyInstance = doc.Create.NewFamilyInstance(point, symbol, doc.ActiveView);


                t.Commit();
            }
            */
            //TaskDialog.Show("Revit", "NewFamilyInstance failed");
                
            

            return Result.Succeeded;
        }
    }
}
