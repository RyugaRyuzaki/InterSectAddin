#region Namespaces
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using WpfCustomControls;
using WpfCustomControls.ViewModel;
using CheckInterSect.Library;
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
        public UIApplication UiApp{ get; }
        public Document Doc { get; }
        private TransactionGroup _TransactionGroup;
        public TransactionGroup TransactionGroup { get => _TransactionGroup; set { _TransactionGroup = value; OnPropertyChanged(); } }


        private ObservableCollection<Document> _RevitLinks;
        public ObservableCollection<Document> RevitLinks { get { if (_RevitLinks == null) _RevitLinks = new ObservableCollection<Document>(); return _RevitLinks; } set { _RevitLinks = value;OnPropertyChanged(); } }
        private Document _SelectedRevitLink;
        public Document SelectedRevitLink { get { return _SelectedRevitLink; } set { _SelectedRevitLink = value;
                if (SelectedRevitLink != null)
                {
                    AllLinkCategory = GetAllCategory(SelectedRevitLink);
                    SelectedLinkCategory = AllLinkCategory[0];
                    if (SelectedLinkCategory != null)
                    {
                        AllLinkElementModels = GetElementModels(SelectedRevitLink, SelectedLinkCategory);
                        SelectedLinkElemenModel = AllLinkElementModels[0];
                     }
                }
                 OnPropertyChanged();
            } }

        private ObservableCollection<Category> _AllLinkCategory;
        public ObservableCollection<Category> AllLinkCategory { get { if (_AllLinkCategory == null) _AllLinkCategory = new ObservableCollection<Category>(); return _AllLinkCategory; } set { _AllLinkCategory = value; OnPropertyChanged(); } }
        private Category _SelectedLinkCategory;
        public Category SelectedLinkCategory { get { return _SelectedLinkCategory; } set { _SelectedLinkCategory = value;
                if (SelectedLinkCategory != null)
                {
                    AllLinkElementModels = GetElementModels(SelectedRevitLink, SelectedLinkCategory);
                    SelectedLinkElemenModel = AllLinkElementModels[0];
                }
                OnPropertyChanged(); } }
        private ObservableCollection<ElementModel> _AllLinkElementModels;
        public ObservableCollection<ElementModel> AllLinkElementModels { get { if (_AllLinkElementModels == null) _AllLinkElementModels = new ObservableCollection<ElementModel>(); return _AllLinkElementModels; } set { _AllLinkElementModels = value; OnPropertyChanged(); } }


        private ElementModel _SelectedLinkElemenModel;
        public ElementModel SelectedLinkElemenModel { get { return _SelectedLinkElemenModel; } set { _SelectedLinkElemenModel = value; OnPropertyChanged(); } }



        private ObservableCollection<Category> _AllMainCategory;
        public ObservableCollection<Category> AllMainCategory { get { if (_AllMainCategory == null) _AllMainCategory = new ObservableCollection<Category>(); return _AllMainCategory; } set { _AllMainCategory = value;OnPropertyChanged(); } }
        private Category _SelectedMainCategory;
        public Category SelectedMainCategory { get { return _SelectedMainCategory; } set { _SelectedMainCategory = value; OnPropertyChanged(); } }





        private TaskBarViewModel _TaskBarViewModel;
        public TaskBarViewModel TaskBarViewModel { get { return _TaskBarViewModel; } set { _TaskBarViewModel = value; OnPropertyChanged(); } }

        private int _Total;
        public int Total { get { return _Total; } set { _Total = value; OnPropertyChanged(); } }

        private int _Current;
        public int Current { get { return _Current; } set { _Current = value; OnPropertyChanged(); } }

        #region Icommand
        public ICommand LoadWindowCommand { get; set; }
        public ICommand SelectionMenuCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ICommand CreateFoundationCommand { get; set; }
        public ICommand CreatePileDetailCommand { get; set; }
        public ICommand CreateReinforcementCommand { get; set; }
        public ICommand CloseWindowCommand { get; set; }
        public ICommand SelectionLanguageChangedCommand { get; set; }
        #endregion
        public CheckIntersectViewModel(UIApplication uiapp, Document doc)
        {
            UiApp = uiapp;
            Doc = doc;
            GetRevitLinkTypes();
            SelectedRevitLink = RevitLinks[0];
            AllMainCategory = GetAllCategory(Doc);
            if (AllMainCategory.Count != 0) SelectedMainCategory = AllMainCategory[0];
            TaskBarViewModel = new TaskBarViewModel();
            Total = 1;
            #region Command
            CloseWindowCommand = new RelayCommand<CheckIntersectWindow>((p) => { return true; }, (p) =>
            {
                p.DialogResult = true;
              
            });
            #endregion
        }

        private void GetRevitLinkTypes()
        {
            List<RevitLinkType> revitLinkTypes = new FilteredElementCollector(Doc).OfCategory(RevitLinkID).OfClass(typeof(RevitLinkType)).Cast<RevitLinkType>().ToList();
            for (int i = 0; i < revitLinkTypes.Count; i++)
            {
                foreach (Document item1 in UiApp.Application.Documents)
                {
                    if (item1.Title.Equals(revitLinkTypes[i].Name.Replace(".rvt","")))
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
                if (item.CategoryType == CategoryModelID &&item.CanAddSubcategory&&(item.Id.IntegerValue!=(int)DetailItemID)&&(item.Id.IntegerValue != (int)HVACZoneID) && (item.Id.IntegerValue != (int)LineID)) categories.Add(item);
            }
            List<Element> elements = new FilteredElementCollector(document).WhereElementIsNotElementType().Cast<Element>().ToList();

            ObservableCollection<Category> categories1 = new ObservableCollection<Category>();
            foreach (Category item in categories)
            {
                foreach (var item1 in elements)
                {
                    Category category = item1.Category;
                    if ((category!=null)&&(category.Name.Equals(item.Name))) categories1.Add(item);
                }
            }
            categories1 = new ObservableCollection<Category>(categories1.Distinct(new DistinctCategory()));
            categories1 = new ObservableCollection<Category>(categories1.OrderBy(x => x.Name));
            return categories1;
        }
        private ObservableCollection<ElementModel> GetElementModels(Document document, Category category)
        {
            
            ObservableCollection<ElementModel> elementModels = new ObservableCollection<ElementModel>();
            List<Element> elements = new FilteredElementCollector(document).WhereElementIsNotElementType().OfCategory((BuiltInCategory)(category.Id.IntegerValue)).Cast<Element>().ToList();
            foreach (var item in elements)
            {
                elementModels.Add(new ElementModel(document, item));
            }
            return elementModels;
        }
    }
}
