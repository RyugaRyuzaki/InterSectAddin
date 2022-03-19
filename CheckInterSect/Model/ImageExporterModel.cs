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

        object[][] DataExport =
        {
             new object[] { "Isometric", 1, 45, 35 }, // 35.264
             new object[] { "North", 1, 0, 0 },
             new object[] { "East", 1, 90, 0 },
             new object[] { "Top", 1, 0, 90, 0 }};

        List<Autodesk.Revit.DB.View> ViewExport;

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
            Color color = new Color(255, 0, 0);
            Color colorBlack = new Color(255, 255, 255);
            OverrideGraphicSettings ogs = new OverrideGraphicSettings();
            ogs.SetSurfaceForegroundPatternColor(color);
            Element aa = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Where(x => x.Name.Equals("<Solid fill>")).FirstOrDefault();
            Element aa1 = new FilteredElementCollector(doc).OfClass(typeof(LinePatternElement)).Where(x => x.Name.Equals("_S4R_Center Line 1/4")).FirstOrDefault();
            ogs.SetCutForegroundPatternId(aa.Id);
            ogs.SetSurfaceForegroundPatternId(aa.Id);
            ogs.SetCutForegroundPatternColor(color);
            ogs.SetSurfaceForegroundPatternColor(color);

            ogs.SetProjectionLinePatternId(aa1.Id);
            ogs.SetProjectionLineColor(colorBlack);
            ogs.SetProjectionLineWeight(5);

            ViewFamilyType viewFamilyType= new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(x =>x.ViewFamily == ViewFamily.ThreeDimensional);

            View3D v = View3D.CreateIsometric(doc, viewFamilyType.Id);
            v.Name = "Isometric";
            v.DetailLevel = ViewDetailLevel.Fine;
            v.SetElementOverrides(directShape.Id, ogs);

            int nViews = DataExport.Length;
            ViewExport = new List<Autodesk.Revit.DB.View>(nViews) { v };
            for (int i = 1; i < nViews; ++i)
            {
                v = View3D.CreateIsometric(doc, viewFamilyType.Id);
                object[] d = DataExport[i];
                v.Name = d[0] as string;
                v.SetOrientation(GetOrientationFor((int)(d[2]), (int)(d[3])));
                v.SaveOrientation();
                v.DetailLevel = ViewDetailLevel.Fine;
                v.SetElementOverrides(directShape.Id, ogs);
                ViewExport.Add(v);
            }

            foreach (Autodesk.Revit.DB.View v2 in ViewExport)
            {
                Parameter graphicDisplayOptions = v2.get_Parameter( BuiltInParameter.MODEL_GRAPHICS_STYLE);
                graphicDisplayOptions.Set(6);
                Categories cats = doc.Settings.Categories;

                foreach (BuiltInCategory bic in HideCategories)
                {
                    Category cat = cats.get_Item(bic);
                    if (null != cat)
                    {
                        v2.SetCategoryHidden(cat.Id, true);
                    }
                }
            }
        }

        public string ExportToImage(Document doc, DirectShape directShape, Element elementSet, Element e, string folderName)
        {
            

            foreach (Autodesk.Revit.DB.View v in ViewExport)
            {
                List<ElementId> hideElements
                  = new FilteredElementCollector(doc, v.Id) .Where(a => a.CanBeHidden(v) && a.Id.IntegerValue != elementSet.Id.IntegerValue).Select(b => b.Id).ToList();

                v.HideElements(hideElements);

                List<ElementId> ids = new List<ElementId>(1) { directShape.Id };

                v.UnhideElements(ids);
            }
          

            View3D v1 = ViewExport.Where(x => x.Name.Contains("Isometric")).FirstOrDefault() as View3D;
            v1.DetailLevel = ViewDetailLevel.Fine;
            v1.DisplayStyle = DisplayStyle.Shading;


            List<ElementId> ids1 = new List<ElementId>(); ids1.Add(v1.Id);
            doc.Regenerate();

            string filepath = Path.Combine(folderName, e.Id.ToString() + ".jpg");
            var ieo = new ImageExportOptions
            {
                FilePath = filepath,
                FitDirection = FitDirectionType.Horizontal,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ImageResolution = ImageResolution.DPI_150,
                ShouldCreateWebSite = false
            };
            ieo.SetViewsAndSheets(ids1);
            ieo.ExportRange = ExportRange.SetOfViews;
            ieo.ZoomType = ZoomFitType.FitToPage;
            ieo.ViewName = "tmp";

            //int n = ViewExport.Count;

            //if (0 < n)
            //{
            //    List<ElementId> ids = new List<ElementId>(
            //      ViewExport.Select<Autodesk.Revit.DB.View, ElementId>(
            //        v => v.Id));

            //    ieo.SetViewsAndSheets(ids);
            //    ieo.ExportRange = ExportRange.SetOfViews;
            //}
            //else
            //{
            //    ieo.ExportRange = ExportRange
            //      .VisibleRegionOfCurrentView;
            //}

            try
            {
                doc.ExportImage(ieo);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            filepath = filepath.Replace(".jpg", " - 3D View - Isometric.jpg");
            return filepath;
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
