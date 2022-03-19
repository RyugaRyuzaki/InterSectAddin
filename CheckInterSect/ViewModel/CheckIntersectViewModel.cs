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
using System.Diagnostics;
using System;
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
                //if (SelectedLinkCategory != null)
                //{
                //    AllLinkElementModels = GetElementModelsOneCategory(SelectedRevitLink, SelectedLinkCategory);
                //    SelectedLinkElemenModel = AllLinkElementModels[0];
                //}
                OnPropertyChanged();
            }
        }
        //private ObservableCollection<ElementModel> _AllLinkElementModels;
        //public ObservableCollection<ElementModel> AllLinkElementModels { get { if (_AllLinkElementModels == null) _AllLinkElementModels = new ObservableCollection<ElementModel>(); return _AllLinkElementModels; } set { _AllLinkElementModels = value; OnPropertyChanged(); } }
        //private ElementModel _SelectedLinkElemenModel;
        //public ElementModel SelectedLinkElemenModel { get { return _SelectedLinkElemenModel; } set { _SelectedLinkElemenModel = value; OnPropertyChanged(); } }



        private ObservableCollection<Category> _AllMainCategory;
        public ObservableCollection<Category> AllMainCategory { get { if (_AllMainCategory == null) _AllMainCategory = new ObservableCollection<Category>(); return _AllMainCategory; } set { _AllMainCategory = value; OnPropertyChanged(); } }
        private Category _SelectedMainCategory;
        public Category SelectedMainCategory
        {
            get { return _SelectedMainCategory; }
            set
            {
                _SelectedMainCategory = value;
                //if (SelectedMainCategory != null)
                //{
                //    AllMainElementModels = GetElementModelsOneCategory(Doc, SelectedMainCategory);
                //    SelectedMainElemenModel = AllMainElementModels[0];
                //}
                OnPropertyChanged();
            }
        }

        //private ObservableCollection<ElementModel> _AllMainElementModels;
        //public ObservableCollection<ElementModel> AllMainElementModels { get { if (_AllMainElementModels == null) _AllMainElementModels = new ObservableCollection<ElementModel>(); return _AllMainElementModels; } set { _AllMainElementModels = value; OnPropertyChanged(); } }
        //private ElementModel _SelectedMainElemenModel;
        //public ElementModel SelectedMainElemenModel { get { return _SelectedMainElemenModel; } set { _SelectedMainElemenModel = value; OnPropertyChanged(); } }





        //private int _Total;
        //public int Total { get { return _Total; } set { _Total = value; OnPropertyChanged(); } }

        //private int _Current;
        //public int Current { get { return _Current; } set { _Current = value; OnPropertyChanged(); } }


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

            #region Command
            CloseWindowCommand = new RelayCommand<CheckIntersectWindow>((p) => { return true; }, (p) =>
            {
                p.DialogResult = true;

            });
            CheckCommand = new RelayCommand<CheckIntersectWindow>((p) => { return SelectedRevitLink != null && SelectedLinkCategory != null && SelectedMainCategory != null&& AllReportViewModels.Count==0; }, (p) =>
              {
                  double a = DateTime.Now.Minute*60+DateTime.Now.Second;
                  AllReportViewModels = GetAllReportViewModelCategory();
                  if (AllReportViewModels.Count == 0)
                  {
                      System.Windows.Forms.MessageBox.Show(string.Format("there is no Element Intersect of {0}",SelectedMainCategory.Id.ToString()));return;
                  }
                  EnabledCategory = AllReportViewModels.Count == 0;
                  double b = DateTime.Now.Minute * 60 + DateTime.Now.Second;
                  System.Windows.Forms.MessageBox.Show((b-a)+"");
              });
            ModifyCommand = new RelayCommand<CheckIntersectWindow>((p) => { return SelectedRevitLink != null && SelectedLinkCategory != null && SelectedMainCategory != null && AllReportViewModels.Count != 0; }, (p) =>
            {
                AllReportViewModels.Clear();
                EnabledCategory = AllReportViewModels.Count == 0;
            });
            ExportPDFCommand = new RelayCommand<CheckIntersectWindow>((p) => { return AllReportViewModels.Count != 0; }, (p) =>
            {


            });
            ExportExcelCommand = new RelayCommand<CheckIntersectWindow>((p) => { return AllReportViewModels.Count != 0; }, (p) =>
            {


            });
            #endregion
        }

        private ObservableCollection<ReportViewModel> GetAllReportViewModelCategory()
        {
            ObservableCollection<ReportViewModel> reportViewModels = new ObservableCollection<ReportViewModel>();
            ObservableCollection<ElementModel> elementModels = GetElementModelsOneCategory(Doc, SelectedMainCategory);
            string folderName = GetFolderImage();
            if (elementModels.Count != 0)
            {
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
                        a.GetClashPoints(Doc,Unit);
                        a.GetImageSource(Doc, folderName);
                        reportViewModels.Add(a);
                    }
                }
            }
            return reportViewModels;
        }
        #region Image
        private string GetFolderImage()
        {
            string folderName = Doc.PathName.Replace(Doc.Title + ".rvt", "ImageIntersect");
            Directory.CreateDirectory(folderName);
            return folderName;
        }
        #endregion


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
        #region Method
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
