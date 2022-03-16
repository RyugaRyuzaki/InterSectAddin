
#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfCustomControls;
using Application = Autodesk.Revit.ApplicationServices.Application;
#endregion


namespace CheckInterSect.Command
{
    [Transaction(TransactionMode.Manual)]
    public class CheckIntersectCmd : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
            ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // code



            using (TransactionGroup transGr = new TransactionGroup(doc))
            {
                transGr.Start("RAPI00TransGr");

                CheckIntersectViewModel viewModel = new CheckIntersectViewModel(uidoc, doc);
                CheckIntersectWindow window = new CheckIntersectWindow(viewModel);
                if (window.ShowDialog() == false) return Result.Cancelled;

                transGr.Assimilate();
            }

            return Result.Succeeded;
        }
    }
}
