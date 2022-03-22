#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CheckInterSect.Library;
using CheckInterSect.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfCustomControls;
using WpfCustomControls.ViewModel;
using DSP;
using System.IO;
using System;

using System.Windows.Media;
using Microsoft.Win32;
using Excel=Microsoft.Office.Interop.Excel;
using app = Microsoft.Office.Interop.Excel.Application;
using Microsoft.Office.Core;

#endregion

namespace CheckInterSect
{
    public class CheckIntersectViewModel : BaseViewModel
    {
        private static readonly BuiltInCategory RevitLinkID = (BuiltInCategory)(-2001352);
        private static readonly BuiltInCategory DetailItemID = (BuiltInCategory)(-2002000);
        private static readonly BuiltInCategory LineID = (BuiltInCategory)(-2000051);
        private static readonly BuiltInCategory HVACZoneID = (BuiltInCategory)(-2008107);
        private static readonly CategoryType CategoryModelID = CategoryType.Model;
        public UIApplication UiApp { get; }
        public Document Doc { get; }
        private TransactionGroup _TransactionGroup;
        public TransactionGroup TransactionGroup { get => _TransactionGroup; set { _TransactionGroup = value; OnPropertyChanged(); } }
        private UnitProject _Unit;
        public UnitProject Unit { get { return _Unit; } set { _Unit = value; OnPropertyChanged(); } }

        private ObservableCollection<Document> _RevitLinks;
        public ObservableCollection<Document> RevitLinks { get { if (_RevitLinks == null) _RevitLinks = new ObservableCollection<Document>(); return _RevitLinks; } set { _RevitLinks = value; OnPropertyChanged(); } }
        private Document _SelectedRevitLink;
        public Document SelectedRevitLink
        {
            get { return _SelectedRevitLink; }
            set
            {
                _SelectedRevitLink = value;
                if (SelectedRevitLink != null)
                {
                    AllLinkCategory = GetAllCategory(SelectedRevitLink);
                    SelectedLinkCategory = AllLinkCategory[0];

                }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Category> _AllLinkCategory;
        public ObservableCollection<Category> AllLinkCategory { get { if (_AllLinkCategory == null) _AllLinkCategory = new ObservableCollection<Category>(); return _AllLinkCategory; } set { _AllLinkCategory = value; OnPropertyChanged(); } }
        private Category _SelectedLinkCategory;
        public Category SelectedLinkCategory
        {
            get { return _SelectedLinkCategory; }
            set
            {
                _SelectedLinkCategory = value;
                OnPropertyChanged();
            }
        }
     

        private ObservableCollection<Category> _AllMainCategory;
        public ObservableCollection<Category> AllMainCategory { get { if (_AllMainCategory == null) _AllMainCategory = new ObservableCollection<Category>(); return _AllMainCategory; } set { _AllMainCategory = value; OnPropertyChanged(); } }
        private Category _SelectedMainCategory;
        public Category SelectedMainCategory
        {
            get { return _SelectedMainCategory; }
            set
            {
                _SelectedMainCategory = value;
              
                OnPropertyChanged();
            }
        }




        private ObservableCollection<ReportViewModel> _AllReportViewModels;
        public ObservableCollection<ReportViewModel> AllReportViewModels
        {
            get { if (_AllReportViewModels == null) _AllReportViewModels = new ObservableCollection<ReportViewModel>(); return _AllReportViewModels; }
            set
            {
                _AllReportViewModels = value;
                if (AllReportViewModels.Count != 0)
                {
                    ReportViewModel = AllReportViewModels[0];
                }
                
                OnPropertyChanged();
            }
        }


        private ReportViewModel _ReportViewModel;
        public ReportViewModel ReportViewModel { get { return _ReportViewModel; } set { _ReportViewModel = value; OnPropertyChanged(); } }


        private bool _EnabledCategory;
        public bool EnabledCategory { get { return _EnabledCategory; } set { _EnabledCategory = value; OnPropertyChanged(); } }
        //private ProgressModel _ProgressModel;
        //public ProgressModel ProgressModel { get => _ProgressModel; set { _ProgressModel = value; OnPropertyChanged(); } }
        #region Icommand
        public ICommand CloseWindowCommand { get; set; }
        public ICommand SelectionLanguageChangedCommand { get; set; }
        public ICommand CheckCommand { get; set; }
        public ICommand ModifyCommand { get; set; }
        public ICommand CheckAllCommand { get; set; }
        public ICommand ExportPDFCommand { get; set; }
        public ICommand ExportExcelCommand { get; set; }
        #endregion
        private TaskBarViewModel _TaskBarViewModel;
        public TaskBarViewModel TaskBarViewModel { get { return _TaskBarViewModel; } set { _TaskBarViewModel = value; OnPropertyChanged(); } }
        public CheckIntersectViewModel(UIApplication uiapp, Document doc)
        {
           
            UiApp = uiapp;
            Doc = doc;
            Unit = GetUnitProject();
            GetRevitLinkTypes();
            SelectedRevitLink = RevitLinks[0];
            AllMainCategory = GetAllCategory(Doc);
            if (AllMainCategory.Count != 0) SelectedMainCategory = AllMainCategory[0];
            TaskBarViewModel = new TaskBarViewModel();
            EnabledCategory = AllReportViewModels.Count == 0;
            //ProgressModel = new ProgressModel(0, 0);
            #region Command
            CloseWindowCommand = new RelayCommand<CheckIntersectWindow>((p) => { return true; }, (p) =>
            {
                p.DialogResult = true;

            });
            CheckCommand = new RelayCommand<CheckIntersectWindow>((p) => { return SelectedRevitLink != null && SelectedLinkCategory != null && SelectedMainCategory != null&& AllReportViewModels.Count==0; }, (p) =>
              {
                  AllReportViewModels = GetAllReportViewModelCategory(p);
                  if (AllReportViewModels.Count == 0)
                  {
                      System.Windows.Forms.MessageBox.Show(string.Format("there is no Element Intersect of {0}", SelectedMainCategory.Id.ToString())); return;
                  }
                  EnabledCategory = AllReportViewModels.Count == 0;

              });
            CheckAllCommand = new RelayCommand<CheckIntersectWindow>((p) => { return RevitLinks.Count!=0; }, (p) =>
            {                 
               if(AllReportViewModels.Count!=0) AllReportViewModels.Clear();
                AllReportViewModels = GetAllReportViewModel(p);
                if (AllReportViewModels.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("there is no Element Intersect of {0}", SelectedMainCategory.Id.ToString())); return;
                }
               
                EnabledCategory = AllReportViewModels.Count == 0;

            });
            ModifyCommand = new RelayCommand<CheckIntersectWindow>((p) => { return SelectedRevitLink != null && SelectedLinkCategory != null && SelectedMainCategory != null && AllReportViewModels.Count != 0; }, (p) =>
            {
                AllReportViewModels.Clear();
                EnabledCategory = AllReportViewModels.Count == 0;
            });
            ExportPDFCommand = new RelayCommand<CheckIntersectWindow>((p) => { return AllReportViewModels.Count != 0; }, (p) =>
            {
                ExportReportToPDF(p);
            });
            ExportExcelCommand = new RelayCommand<CheckIntersectWindow>((p) => { return AllReportViewModels.Count != 0; }, (p) =>
            {
                
                ExportReportToExcel();

            });
            #endregion
        }

      


        #region Image
        private string GetFolderImage()
        {
            string folderName = Doc.PathName.Replace(Doc.Title + ".rvt", "ImageIntersect");
            Directory.CreateDirectory(folderName);
            return folderName;
        }
        #endregion

        #region Report
        private List<ElementId> GetCategoriesHide()
        {
            ObservableCollection<Category> hideCategories = new ObservableCollection<Category>(AllMainCategory);
            for (int i = 0; i < AllLinkCategory.Count; i++)
            {
                hideCategories.Add(AllLinkCategory[i]);
            }
            hideCategories = new ObservableCollection<Category>(hideCategories.Distinct(new DistinctCategory()));
            hideCategories = new ObservableCollection<Category>(hideCategories.Where(x=>!x.Name.Equals(SelectedMainCategory.Name)));
            List<ElementId> elementIds = new List<ElementId>();
            foreach (var item in hideCategories)
            {
                elementIds.Add(item.Id);
            }
            return elementIds;
        }
        private ObservableCollection<ReportViewModel> GetAllReportViewModelCategory(CheckIntersectWindow p)
        {
           
            ObservableCollection<ReportViewModel> reportViewModels = new ObservableCollection<ReportViewModel>();
            ObservableCollection<ElementModel> elementModels = GetElementModelsOneCategory(Doc, SelectedMainCategory);
            List<ElementId> elementIds = GetCategoriesHide();
            string folderName = GetFolderImage();
            if (elementModels.Count != 0)
            {
                //System.Windows.Controls.ProgressBar uc = VisualTreeHelper.FindChild<System.Windows.Controls.ProgressBar>(p, "Progress");
                //uc.Maximum = GetProgressBarRebarCategory(elementModels) * 1.0;
                for (int i = 0; i < elementModels.Count; i++)
                {
                    List<Element> elements = SolidFace.GetElementSolidIntersectLink(SelectedRevitLink, elementModels[i].Element, SelectedLinkCategory);
                    if (elements.Count != 0)
                    {
                        ObservableCollection<ElementModel> elementModelsIntersect = new ObservableCollection<ElementModel>();
                        for (int j = 0; j < elements.Count; j++)
                        {
                            elementModelsIntersect.Add(new ElementModel(SelectedRevitLink, elements[j]));
                        }
                        var a = new ReportViewModel(Doc, SelectedRevitLink, elementModels[i], elementModelsIntersect);
                        a.GetClashPoints(Doc, Unit);
                        a.GetImageSource(Doc, folderName, elementIds);
                        reportViewModels.Add(a);
                        //ProgressModel.SetValue(uc, 1);
                    }
                }
                //ProgressModel.ResetValue(uc);
            }
            return reportViewModels;
        }

        //private double GetProgressBarRebarCategory(ObservableCollection<ElementModel> elementModels)
        //{
        //    double a = 0;
        //    for (int i = 0; i < elementModels.Count; i++)
        //    {
        //        a++;
        //    }
        //    return a;
        //}

        private ObservableCollection<ReportViewModel> GetAllReportViewModel(CheckIntersectWindow p)
        {
            
            ObservableCollection<ElementModel> elementModels = GetElementModelsAllCategory(Doc);
          
            List<ElementId> elementIds = GetCategoriesHide();
            string folderName = GetFolderImage();
            ObservableCollection<ReportViewModel> reportViewModels = new ObservableCollection<ReportViewModel>();
            if (elementModels.Count != 0)
            {
                //System.Windows.Controls.ProgressBar uc = VisualTreeHelper.FindChild<System.Windows.Controls.ProgressBar>(p, "Progress");
                //uc.Maximum = GetProgressBarRebar(elementModels) * 1.0;
                for (int i = 0; i < RevitLinks.Count; i++)
                {
                    for (int j = 0; j < elementModels.Count; j++)
                    {
                        List<Element> elements = SolidFace.GetElementSolidIntersectLink(RevitLinks[i], elementModels[j].Element);
                        if (elements.Count != 0)
                        {
                            ObservableCollection<ElementModel> elementModelsIntersect = new ObservableCollection<ElementModel>();
                            for (int k = 0; k< elements.Count; k++)
                            {
                                elementModelsIntersect.Add(new ElementModel(RevitLinks[i], elements[k]));
                            }
                            var a = new ReportViewModel(Doc, RevitLinks[i], elementModels[j], elementModelsIntersect);
                            a.GetClashPoints(Doc, Unit);
                            a.GetImageSource(Doc, folderName, elementIds);
                            reportViewModels.Add(a);
                            //ProgressModel.SetValue(uc, 1);
                        }
                    }
                }
                //ProgressModel.ResetValue(uc);
            }
            return reportViewModels;
        }
        //private double GetProgressBarRebar(ObservableCollection<ElementModel> elementModels)
        //{
        //    double a = 0;
        //    for (int i = 0; i < RevitLinks.Count; i++)
        //    {
        //        for (int j = 0; j < elementModels.Count; j++)
        //        {
        //            a++;
        //        }

        //    }
        //    return a;
        //}

        #endregion
        #region Export
        private void ExportReportToPDF(CheckIntersectWindow uc)
        {
            try
            {
                for (int i = 0; i < AllReportViewModels.Count; i++)
                {
                    ReportViewModel = AllReportViewModels[i];
                    System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        System.Windows.Media.Transform originalScale = uc.Layout.LayoutTransform;
                        System.Printing.PrintCapabilities capabilities = printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
                        double scale = Math.Min(capabilities.PageImageableArea.ExtentWidth / uc.Layout.ActualWidth, capabilities.PageImageableArea.ExtentHeight /
                                       uc.Layout.ActualHeight);
                        uc.Layout.LayoutTransform = new ScaleTransform(scale, scale);
                        System.Windows.Size sz = new System.Windows.Size(capabilities.PageImageableArea.ExtentWidth, capabilities.PageImageableArea.ExtentHeight);
                        uc.Layout.Measure(sz);
                        uc.Layout.Arrange(new System.Windows.Rect(new System.Windows.Point(capabilities.PageImageableArea.OriginWidth, capabilities.PageImageableArea.OriginHeight), sz));
                        printDialog.PrintVisual(uc.Layout, "My Print");
                        uc.Layout.LayoutTransform = originalScale;
                        printDialog.PrintVisual(uc.Layout, "test");

                    }
                }

            }
            catch (Exception e)
            {

                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }
        private void ExportReportToExcel()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Browse Text Files",
                DefaultExt = ".xlsx",
                Filter = "Excel |*.xlsx| Excel 2003 |*.xlsx",
                FilterIndex = 2,
                RestoreDirectory = true,
                FileName = "CheckClash.xlsx",
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                int start = 2;
                try
                {
                    var xlApp = new app();
                    Excel.Workbook xlWorkBook = xlApp.Workbooks.Add(Type.Missing);

                    for (int i = 0; i < AllReportViewModels.Count; i++)
                    {
                        Excel.Worksheet ws = (Excel.Worksheet)xlWorkBook.Worksheets.Add();
                        ws.Name = AllReportViewModels[i].ElementSet.Element.Id.ToString();
                        ws.Columns[1].ColumnWidth = 5;
                        ws.Columns[2].ColumnWidth = 30;
                        ws.Columns[3].ColumnWidth = 30;
                        ws.Columns[4].ColumnWidth = 30;
                        ws.Columns[5].ColumnWidth = 5;
                        int endRowTitle = GetEndRowTitle(ws, AllReportViewModels[i], start);
                        int startEndRow = endRowTitle;
                        for (int j = 0; j < AllReportViewModels[i].ElementInterSects.Count; j++)
                        {
                            startEndRow = GetEndRowIntersect(ws, AllReportViewModels[i].ElementInterSects[j], startEndRow);
                        }
                    }
                    xlApp.ActiveWorkbook.SaveCopyAs(saveFileDialog.FileName);
                    xlApp.ActiveWorkbook.Saved = true;
                }
                catch (Exception e)
                {

                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
            }
        }

        private int GetEndRowTitle(Excel.Worksheet ws, ReportViewModel reportView,int start)
        {
            ws.Rows[1].RowHeight = 20;
            ws.Rows[start].RowHeight = 20;
            ws.Rows[start+1].RowHeight = 20;
            ws.Rows[start+2].RowHeight = 20;
            ws.Rows[start+3].RowHeight = 20;
            ws.Rows[start+4].RowHeight = 20;
            ws.Cells[start, 2] = "Name";
            ws.Cells[start, 2].Font.Bold = true;
            ws.Cells[start, 2].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[start, 3] = reportView.NameClash;
            ws.Range[ws.Cells[start, 3], ws.Cells[start, 4]].Merge();
            ws.Range[ws.Cells[start, 3], ws.Cells[start, 4]].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[start+1, 2] = "Element Set";
            ws.Cells[start+1, 2].Font.Bold = true;
            ws.Range[ws.Cells[start+1, 3], ws.Cells[start+1, 4]].Merge();

            ws.Cells[start+2, 2] = "Element ID";
            ws.Cells[start+2, 2].Font.Bold = true;
            ws.Cells[start+2, 3] = reportView.ElementSet.Element.Id;
            ws.Range[ws.Cells[start+2, 3], ws.Cells[start+2, 4]].Merge();
            ws.Range[ws.Cells[start+2, 3], ws.Cells[start+2, 4]].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[start + 3, 2] = "Category";
            ws.Cells[start + 3, 2].Font.Bold = true;
            ws.Cells[start + 3, 3] = reportView.ElementSet.Element.Category.Name;
            ws.Range[ws.Cells[start + 3, 3], ws.Cells[start + 3, 4]].Merge();
            ws.Range[ws.Cells[start + 3, 3], ws.Cells[start + 3, 4]].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[start + 4, 2] = "Element Type";
            ws.Cells[start + 4, 2].Font.Bold = true;
            ws.Cells[start + 4, 3] = reportView.ElementSet.ElementType.Name;
            ws.Range[ws.Cells[start + 4, 3], ws.Cells[start + 4, 4]].Merge();
            ws.Range[ws.Cells[start + 4, 3], ws.Cells[start + 4, 4]].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Range[ws.Cells[start, 2], ws.Cells[start + 4, 4]].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;

            return start + 6;
        }
        private int GetEndRowIntersect(Excel.Worksheet ws, ElementModel elementModel, int startEndRow)
        {
            ws.Rows[startEndRow].RowHeight = 20;
            ws.Rows[startEndRow + 1].RowHeight = 20;
            ws.Rows[startEndRow + 2].RowHeight = 20;
            ws.Rows[startEndRow + 3].RowHeight = 20;
            ws.Rows[startEndRow + 4].RowHeight = 20;
            ws.Rows[startEndRow + 5].RowHeight = 20;
            ws.Rows[startEndRow + 6].RowHeight = 20;
            ws.Rows[startEndRow + 7].RowHeight = 20;
            Excel.Range range = ws.Range[ws.Cells[startEndRow, 2], ws.Cells[startEndRow + 6, 2]];
            range.Merge();
            ws.Range[ws.Cells[startEndRow, 2], ws.Cells[startEndRow + 6, 4]].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            object missing = System.Reflection.Missing.Value;
            Excel.Pictures p = ws.Pictures(missing) as Excel.Pictures;
            Excel.Picture pic = p.Insert(elementModel.ImageSource, missing);
            pic.Left = ((double)range.Left+1);
            pic.Top = ((double)range.Top+1);
            pic.Width = ((double)range.Width-2);

            ws.Cells[startEndRow, 3] = "Element ID";
            ws.Cells[startEndRow, 3].Font.Bold = true;
            ws.Cells[startEndRow, 4] = elementModel.Element.Id;
            ws.Cells[startEndRow, 4].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[startEndRow+1, 3] = "Category";
            ws.Cells[startEndRow+1, 3].Font.Bold = true;
            ws.Cells[startEndRow+1, 4] = elementModel.Element.Category.Name;
            ws.Cells[startEndRow+1, 4].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[startEndRow + 2, 3] = "Element Type";
            ws.Cells[startEndRow + 2, 3].Font.Bold = true;
            ws.Cells[startEndRow + 2, 4] = elementModel.ElementType.Name;
            ws.Cells[startEndRow + 2, 4].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[startEndRow + 3, 3] = "Clash point";
            ws.Cells[startEndRow + 3, 3].Font.Bold = true;

            ws.Cells[startEndRow + 4, 3] = "X";
            ws.Cells[startEndRow + 4, 3].Font.Bold = true;
            ws.Cells[startEndRow + 4, 4] = elementModel.X;
            ws.Cells[startEndRow + 4, 4].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[startEndRow + 5, 3] = "Y";
            ws.Cells[startEndRow + 5, 3].Font.Bold = true;
            ws.Cells[startEndRow + 5, 4] = elementModel.Y;
            ws.Cells[startEndRow + 5, 4].Interior.Color = Excel.XlRgbColor.rgbCornsilk;

            ws.Cells[startEndRow + 6, 3] = "Z";
            ws.Cells[startEndRow + 6, 3].Font.Bold = true;
            ws.Cells[startEndRow + 6, 4] = elementModel.Z;
            ws.Cells[startEndRow + 6, 4].Interior.Color = Excel.XlRgbColor.rgbCornsilk;
            return startEndRow + 8;
        }



        #endregion
        #region Method
        private void GetRevitLinkTypes()
        {
            List<RevitLinkType> revitLinkTypes = new FilteredElementCollector(Doc).OfCategory(RevitLinkID).OfClass(typeof(RevitLinkType)).Cast<RevitLinkType>().ToList();
            for (int i = 0; i < revitLinkTypes.Count; i++)
            {
                foreach (Document item1 in UiApp.Application.Documents)
                {
                    if (item1.Title.Equals(revitLinkTypes[i].Name.Replace(".rvt", "")))
                    {
                        RevitLinks.Add(item1);
                    }
                }
            }
        }
        private ObservableCollection<Category> GetAllCategory(Document document)
        {
            ObservableCollection<Category> categories = new ObservableCollection<Category>();

            Categories categoriesSet = document.Settings.Categories;
            foreach (Category item in categoriesSet)
            {
                if (item.CategoryType == CategoryModelID && item.CanAddSubcategory && (item.Id.IntegerValue != (int)DetailItemID) && (item.Id.IntegerValue != (int)HVACZoneID) && (item.Id.IntegerValue != (int)LineID)) categories.Add(item);
            }
            List<Element> elements = new FilteredElementCollector(document).WhereElementIsNotElementType().Cast<Element>().ToList();

            ObservableCollection<Category> categories1 = new ObservableCollection<Category>();
            foreach (Category item in categories)
            {
                foreach (var item1 in elements)
                {
                    Category category = item1.Category;
                    if ((category != null) && (category.Name.Equals(item.Name))) categories1.Add(item);
                }
            }
            categories1 = new ObservableCollection<Category>(categories1.Distinct(new DistinctCategory()));
            categories1 = new ObservableCollection<Category>(categories1.OrderBy(x => x.Name));
            return categories1;
        }
        private ObservableCollection<ElementModel> GetElementModelsOneCategory(Document document, Category category)
        {

            ObservableCollection<ElementModel> elementModels = new ObservableCollection<ElementModel>();
            List<Element> elements = new FilteredElementCollector(document).WhereElementIsNotElementType().OfCategory((BuiltInCategory)(category.Id.IntegerValue)).Cast<Element>().ToList();
            foreach (var item in elements)
            {

                elementModels.Add(new ElementModel(document, item));

            }
            return elementModels;
        }
        private ObservableCollection<ElementModel> GetElementModelsAllCategory(Document document)
        {

            ObservableCollection<ElementModel> elementModels = new ObservableCollection<ElementModel>();
            List<Element> elements = new FilteredElementCollector(document).WhereElementIsNotElementType().Cast<Element>().ToList();
             Categories categoriesSet = document.Settings.Categories;
            ObservableCollection<Category> categories = new ObservableCollection<Category>();
            foreach (Category item in categoriesSet)
            {
                if (item.CategoryType == CategoryModelID && item.CanAddSubcategory && (item.Id.IntegerValue != (int)DetailItemID) && (item.Id.IntegerValue != (int)HVACZoneID) && (item.Id.IntegerValue != (int)LineID)) categories.Add(item);
            }
            foreach (Category item in categories)
            {
                foreach (var item1 in elements)
                {
                    Category category = item1.Category;
                    if ((category != null) && (category.Name.Equals(item.Name))) elementModels.Add(new ElementModel(document, item1));
                }
            }
            return elementModels;
        }
        private UnitProject GetUnitProject()
        {
            UnitProject a = new UnitProject(1, "ft");
            ForgeTypeId forgeTypeId = Doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            if (forgeTypeId == UnitTypeId.Centimeters)
            {
                a.UnitInt = 1; a.UnitName = "cm";
            }
            if (forgeTypeId == UnitTypeId.Decimeters)
            {
                a.UnitInt = 2; a.UnitName = "dm";
            }
            if (forgeTypeId == UnitTypeId.Feet)
            {
                a.UnitInt = 3; a.UnitName = "ft";
            }
            if (forgeTypeId == UnitTypeId.Inches)
            {
                a.UnitInt = 4; a.UnitName = "in";
            }
            if (forgeTypeId == UnitTypeId.Meters)
            {
                a.UnitInt = 5; a.UnitName = "m";
            }
            if (forgeTypeId == UnitTypeId.Millimeters)
            {
                a.UnitInt = 6; a.UnitName = "mm";
            }
            if (forgeTypeId == UnitTypeId.Inches)
            {
                a.UnitInt = 7; a.UnitName = "inUS";
            }
            if (forgeTypeId == UnitTypeId.FeetFractionalInches)
            {
                a.UnitInt = 8; a.UnitName = "ft-in";
            }
            if (forgeTypeId == UnitTypeId.FractionalInches)
            {
                a.UnitInt = 9; a.UnitName = "inch";
            }
            if (forgeTypeId == UnitTypeId.MetersCentimeters)
            {
                a.UnitInt = 10; a.UnitName = "m";
            }
            return a;
        }
        #endregion
    }
}
