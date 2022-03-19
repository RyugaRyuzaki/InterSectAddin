using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCustomControls;
using Autodesk.Revit.DB;
using CheckInterSect.Library;
using DSP;
using System.IO;

namespace CheckInterSect
{
    public class ElementModel  :BaseViewModel
    {
        private static readonly BuiltInCategory GenericModelID = (BuiltInCategory)(-2000151);
        private static readonly BuiltInCategory StructutalFramingID = (BuiltInCategory)(1848740);
        private static readonly BuiltInParameter TypeID = (BuiltInParameter)(-1002050);
        private Element _Element;
        public Element Element { get { return _Element; } set { _Element = value; OnPropertyChanged(); } }

        private ElementType _ElementType;
        public ElementType ElementType { get { return _ElementType; } set { _ElementType = value; OnPropertyChanged(); } }


        private string _ImageSource;
        public string ImageSource { get { return _ImageSource; } set { _ImageSource = value; OnPropertyChanged(); } }

        private Solid _SolidIntersect;
        public Solid SolidIntersect { get { return _SolidIntersect; } set { _SolidIntersect = value; OnPropertyChanged(); } }


        private string _ClashPoint;
        public string ClashPoint { get { return _ClashPoint; } set { _ClashPoint = value; OnPropertyChanged(); } }



        private DirectShape _DirectShape;
        public DirectShape DirectShape { get { return _DirectShape; } set { _DirectShape = value; OnPropertyChanged(); } }


        public ElementModel(Document document, Element element)
        {
            Element = element;
            ElementType = document.GetElement(Element.get_Parameter(TypeID).AsElementId()) as ElementType;
            
        }

        public static Solid GetIntersectSolid(Element Element, Element elementSet)
        {
            Solid solidSet = SolidFace.MergeSolid(elementSet);
            if (solidSet == null) return null;
            Solid solidElement = SolidFace.MergeSolid(Element);
            return BooleanOperationsUtils.ExecuteBooleanOperation(solidSet, solidElement, BooleanOperationsType.Intersect);
        }
        public void GetClashPoint(Document document, UnitProject unit)
        {
            XYZ clashPoint = SolidIntersect.ComputeCentroid();
            double x= double.Parse(UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, (clashPoint.X), false));
            double y= double.Parse(UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, (clashPoint.Y), false));
            double z= double.Parse(UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, (clashPoint.Z), false));
            if (clashPoint!=null)
            {
                ClashPoint = String.Format("X :{0} {3} ; Y :{1} {3} ; Z :{2} {3}",Math.Round (x,3), Math.Round(y, 3), Math.Round(z, 3), unit.UnitName);
            }
        }
         private DirectShape GetDirectShape(Document document)
        {
            if (SolidIntersect == null) {  return null; }
            else
            {
                DirectShape direct = null;
                try
                {
                    direct = DirectShape.CreateElement(document, new ElementId(GenericModelID));
                    direct.ApplicationId = "1234567890";
                    direct.ApplicationDataId = "Create Temp Element";
                    direct.SetShape(new GeometryObject[] { SolidIntersect });
                    
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
                    direct = null;
                }
                return direct;

            }
        }
        #region Folder, Path Image

        public string GetImageSource(Document document,Element elementSet,string folderName)
        {
            string source = "";
            using(Transaction transaction= new Transaction(document))
            {
                transaction.Start("aaa");
                DirectShape = GetDirectShape(document);

                //try
                //{
                //    JoinGeometryUtils.JoinGeometry(document, DirectShape, elementSet);
                //}
                //catch (Exception e)
                //{

                //    System.Windows.Forms.MessageBox.Show(e.Message);
                //}

                if (DirectShape != null)
                {

                    ImageExporterModel imageExporter = new ImageExporterModel(document, DirectShape);
                    source = imageExporter.ExportToImage(document, DirectShape, elementSet, Element, folderName);
                }

                transaction.RollBack();
            }

            return source;
        }
        #endregion
    }
}
