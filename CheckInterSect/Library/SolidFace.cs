using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
namespace CheckInterSect.Library
{
    public class SolidFace
    {
        private static List<Solid> GetSolidsElement(Element element)
        {
            List<Solid> a = new List<Solid>();
            List<Solid> b = new List<Solid>();
            Options options = new Options();
            options.ComputeReferences = true;
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geometryElement = element.get_Geometry(options);
            foreach (GeometryObject geometryObject in geometryElement)
            {
                Solid solid = geometryObject as Solid;
                if (solid != null)
                {

                    a.Add(solid);

                }
                else
                {
                    GeometryInstance geometryInstance = geometryObject as GeometryInstance;
                    GeometryElement geometryElement1 = geometryInstance.GetInstanceGeometry();
                    foreach (GeometryObject geometryObject1 in geometryElement1)
                    {

                        try
                        {
                            Solid solid1 = geometryObject1 as Solid;
                            if (solid1 != null)
                            {
                               
                                a.Add(solid1);

                            }
                        }
                        catch (System.Exception e)
                        {

                            System.Windows.Forms.MessageBox.Show(e.Message);
                        }
                       
                    }
                }

            }
            
            foreach (Solid item in a)
            {
                if (item.Volume != 0) { b.Add(item); }
            }
            return b;
        }
        private static Solid MergeSolid(List<Solid> solids)
        {
            Solid mergeSolid = null;
            foreach (var item in solids)
            {
                if (mergeSolid == null)
                {
                    mergeSolid = item;
                }
                else
                {
                    mergeSolid = BooleanOperationsUtils.ExecuteBooleanOperation(mergeSolid, item, BooleanOperationsType.Union);
                }
            }
            return mergeSolid;
        }
        public static Solid MergeSolid(Element element)
        {
            List<Solid> solids = GetSolidsElement(element);
            Solid mergeSolid = null;
            foreach (var item in solids)
            {
                if (mergeSolid == null)
                {
                    mergeSolid = item;
                }
                else
                {
                    mergeSolid = BooleanOperationsUtils.ExecuteBooleanOperation(mergeSolid, item, BooleanOperationsType.Union);
                }
            }
            return mergeSolid;
        }
        public static List<Element> GetElementSolidIntersectLink(Document document, Element elementMain, Category category=null)
        {
            List<ElementId> elementIds = new List<ElementId>();
            elementIds.Add(elementMain.Id);
            List<Solid> solids = GetSolidsElement(elementMain);
            Solid mergeSolid = MergeSolid(solids);
            List<Element> intersect
                = new FilteredElementCollector(document)
                    .WherePasses(new ElementIntersectsSolidFilter(mergeSolid) )
                    .WherePasses(new ExclusionFilter(elementIds))
                    .Where(x=>(category==null)?true:(x.Category.Name==category.Name))
                    .ToList();
            intersect = intersect.Distinct(new DistinctID()).ToList();
            return intersect;
        }

    }
}
