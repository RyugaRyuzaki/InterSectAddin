using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
namespace CheckInterSect
{
    public class ImageExporterModel
    {

        private static readonly Color colorRed = new Color(255, 0, 0);
        private static readonly Color colorBlack = new Color(255, 255, 255);
        object[][] DataExport =
        {
             new object[] { "Isometric", 1, 45, 35 }, // 35.264
             new object[] { "North", 1, 0, 0 },
             new object[] { "East", 1, 90, 0 },
             new object[] { "Top", 1, 0, 90, 0 }};

        //List<Autodesk.Revit.DB.View> ViewExport;
        public View3D View3D { get; set; }
        BuiltInCategory[] HideCategories = new BuiltInCategory[]{ BuiltInCategory.OST_Cameras, BuiltInCategory.OST_IOS_GeoSite,BuiltInCategory.OST_Levels,BuiltInCategory.OST_ProjectBasePoint};

        ViewOrientation3D GetOrientationFor(double yaw_degrees, double pitch_degrees)
        {
            if (Util.IsEqual(270, yaw_degrees) || 270 > yaw_degrees) yaw_degrees += 90.0;
            double angle_in_xy_plane = yaw_degrees * Math.PI / 180.0;
            double angle_up_down = pitch_degrees * Math.PI / 180.0;

            XYZ eye = new XYZ(Math.Cos(angle_in_xy_plane),Math.Sin(angle_in_xy_plane),Math.Cos(angle_up_down));

            XYZ forward = -eye;

            XYZ left = Util.IsVertical(forward) ? -XYZ.BasisX: XYZ.BasisZ.CrossProduct(forward);

            XYZ up = forward.CrossProduct(left);
            ViewOrientation3D orientation= new ViewOrientation3D(eye, up, forward);
            return orientation;
        }

        public ImageExporterModel(Document doc,DirectShape directShape)
        {
            
            ViewFamilyType viewFamilyType= new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(x =>x.ViewFamily == ViewFamily.ThreeDimensional);

            View3D = View3D.CreateIsometric(doc, viewFamilyType.Id);
            View3D.Name = "Isometric";
            
           
        }
       

        public string ExportToImage(Document doc, DirectShape directShape, Element elementSet, Element e, string folderName,List<ElementId> elementIds,XYZ centroid)
        {

            OverrideGraphicSettings ogs = GetOverrideSettingDirectShape(doc);
            OverrideGraphicSettings ogs1 = GetOverrideSettingUnUseElements(doc);
            List<ElementId> hideElements
                  = new FilteredElementCollector(doc, View3D.Id).Where(a => a.CanBeHidden(View3D) && a.Id.IntegerValue != elementSet.Id.IntegerValue).Select(b => b.Id).ToList();


            View3D.HideElements(hideElements);
            List<ElementId> ids = new List<ElementId>(1) { directShape.Id };

            View3D.UnhideElements(ids);


            View3D.DetailLevel = ViewDetailLevel.Fine;
            
            View3D.DisplayStyle = DisplayStyle.Shading;
            View3D.SetElementOverrides(directShape.Id, ogs);
            foreach (var item in elementIds)
            {
                View3D.SetCategoryOverrides(item, ogs1);
            }

            GetCropRegionView3D(centroid);


            List<ElementId> ids1 = new List<ElementId>(); ids1.Add(View3D.Id);
            doc.Regenerate();

            string filepath = Path.Combine(folderName, e.Id.ToString() + ".jpg");
            var ieo = new ImageExportOptions
            {
                FilePath = filepath,
                FitDirection = FitDirectionType.Horizontal,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ImageResolution = ImageResolution.DPI_600,
                ShouldCreateWebSite = false
            };
            ieo.SetViewsAndSheets(ids1);
            ieo.ExportRange = ExportRange.SetOfViews;
            ieo.ZoomType = ZoomFitType.FitToPage;
            ieo.ViewName = "tmp";

            filepath = filepath.Replace(".jpg", " - 3D View - Isometric.jpg");
             
            try
            {
               
                doc.ExportImage(ieo);
            }
            catch (Exception )
            {
                return filepath;
            }
            
            return filepath;
        }

        private void GetCropRegionView3D( XYZ centroid)
        {
            View3D.CropBoxActive = true;
            XYZ up = View3D.UpDirection;
            XYZ dir = View3D.ViewDirection;
            XYZ cross = dir.CrossProduct(up);
            
            XYZ p0 = centroid + 5.0 * View3D.RightDirection;
            XYZ p1 = p0+5.0*XYZ.BasisZ;
            XYZ p2 = p1 - 10.0 * View3D.RightDirection;
            XYZ p3 = p2 - 10.0 * XYZ.BasisZ;
            XYZ p4 = p3+ 10.0 * View3D.RightDirection;
            CurveLoop curves = new CurveLoop();
            curves.Append(Line.CreateBound(p1, p2));
            curves.Append(Line.CreateBound(p2, p3));
            curves.Append(Line.CreateBound(p3, p4));
            curves.Append(Line.CreateBound(p4, p1));
            //XYZ min = p3 + dir * 5.0;
            //XYZ max = p1 - dir * 5.0;

            //View3D.CropBox.Max = max;
            //View3D.CropBox.Min = min;
            //System.Windows.Forms.MessageBox.Show(max + "\n" + min);
            //try
            //{
            //    View3D.GetCropRegionShapeManager().SetCropShape(curves);
            //}
            //catch (Exception e)
            //{

            //    System.Windows.Forms.MessageBox.Show(e.Message);
            //}


        }

        private OverrideGraphicSettings GetOverrideSettingDirectShape(Document document)
        {
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            ogs.SetSurfaceForegroundPatternColor(colorRed);
            Element aa = new FilteredElementCollector(document).OfClass(typeof(FillPatternElement)).Where(x => x.Name.Equals("<Solid fill>")).FirstOrDefault();
            Element aa1 = new FilteredElementCollector(document).OfClass(typeof(LinePatternElement)).Where(x => x.Name.Equals("_S4R_Center Line 1/4")).FirstOrDefault();
            ogs.SetCutForegroundPatternId(aa.Id);
            ogs.SetSurfaceForegroundPatternId(aa.Id);
            ogs.SetCutForegroundPatternColor(colorRed);
            ogs.SetSurfaceForegroundPatternColor(colorRed);

            ogs.SetProjectionLinePatternId(aa1.Id);
            ogs.SetProjectionLineColor(colorBlack);
            ogs.SetProjectionLineWeight(5);
            
            return ogs;
        }
        private OverrideGraphicSettings GetOverrideSettingUnUseElements(Document document)
        {
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            ogs.SetSurfaceTransparency(95);

            return ogs;
        }
    }
    class Util
    {
        public const double _eps = 1.0e-9;

        public static double Eps
        {
            get
            {
                return _eps;
            }
        }

        public static bool IsZero(
          double a,
          double tolerance = _eps)
        {
            return tolerance > Math.Abs(a);
        }

        public static bool IsEqual(
          double a,
          double b,
          double tolerance = _eps)
        {
            return IsZero(b - a, tolerance);
        }

        public static bool IsVertical(XYZ v)
        {
            return IsZero(v.X) && IsZero(v.Y);
        }

    }
}
