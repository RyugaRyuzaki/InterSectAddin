using Autodesk.Revit.DB;
using CheckInterSect.Library;
using DSP;
using System;
using System.Collections.Generic;
using WpfCustomControls;

namespace CheckInterSect
{
    public class ElementModel : BaseViewModel
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


        private XYZ _ClashPoint;
        public XYZ ClashPoint { get { return _ClashPoint; } set { _ClashPoint = value; OnPropertyChanged(); } }

        private string _X;
        public string X { get { return _X; } set { _X = value; OnPropertyChanged(); } }
        private string _Y;
        public string Y { get { return _Y; } set { _Y = value; OnPropertyChanged(); } }
        private string _Z;
        public string Z { get { return _Z; } set { _Z = value; OnPropertyChanged(); } }

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
            ClashPoint = SolidIntersect.ComputeCentroid();
            X = Math.Round(double.Parse(UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, (ClashPoint.X), false)),3) + " " + unit.UnitName;
            Y = Math.Round(double.Parse(UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, (ClashPoint.Y), false)), 3) + " " + unit.UnitName;
            Z = Math.Round(double.Parse(UnitFormatUtils.Format(document.GetUnits(), SpecTypeId.Length, (ClashPoint.Z), false)), 3) + " " + unit.UnitName;
           
        }
        private DirectShape GetDirectShape(Document document)
        {
            if (SolidIntersect == null) { return null; }
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

        public string GetImageSource(Document document, Element elementSet, string folderName, List<ElementId> elementIds)
        {
            string source = "";
            using (Transaction transaction = new Transaction(document))
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
                    source = imageExporter.ExportToImage(document, DirectShape, elementSet, Element, folderName, elementIds, ClashPoint);
                }

                transaction.RollBack();
            }

            return source;
        }
        #endregion
    }
}
