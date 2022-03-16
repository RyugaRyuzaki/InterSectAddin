#region Namespaces
using System;
using System.Collections;
using System.Collections.Generic;
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
#endregion

namespace CheckInterSect
{
    public class CheckIntersectViewModel : BaseViewModel
    {
        public UIDocument UiDoc;
        public Document Doc;
        private TransactionGroup _TransactionGroup;
        public TransactionGroup TransactionGroup { get => _TransactionGroup; set { _TransactionGroup = value; OnPropertyChanged(); } }
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
        public CheckIntersectViewModel(UIDocument uiDoc, Document doc)
        {
            UiDoc = uiDoc;
            Doc = doc;
            TaskBarViewModel = new TaskBarViewModel();
            Total = 1;
            #region Command
            CloseWindowCommand = new RelayCommand<CheckIntersectWindow>((p) => { return true; }, (p) =>
            {
                p.DialogResult = true;
              
            });
            #endregion
        }

    }
}
