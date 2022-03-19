using Autodesk.Revit.DB;
using DSP;
using System.Collections.ObjectModel;
using System.Linq;
using WpfCustomControls;
namespace CheckInterSect.ViewModel
{
    public class ReportViewModel : BaseViewModel
    {

        private string _NameClash;
        public string NameClash { get { return _NameClash; } set { _NameClash = value; OnPropertyChanged(); } }

        private ElementModel _ElementSet;
        public ElementModel ElementSet { get { return _ElementSet; } set { _ElementSet = value; OnPropertyChanged(); } }

        private ObservableCollection<ElementModel> _ElementInterSects;
        public ObservableCollection<ElementModel> ElementInterSects { get { if (_ElementInterSects == null) _ElementInterSects = new ObservableCollection<ElementModel>(); return _ElementInterSects; } set { _ElementInterSects = value; OnPropertyChanged(); } }





        public ReportViewModel(Document documentSet, Document documentIntersect, ElementModel elementSet, ObservableCollection<ElementModel> elementInterSects)
        {
            NameClash = documentSet.Title + " VS " + documentIntersect.Title;
            ElementSet = elementSet;
            ElementInterSects = elementInterSects;
            GetIntesectSolids();
        }
        private void GetIntesectSolids()
        {
            for (int i = 0; i < ElementInterSects.Count; i++)
            {
                ElementInterSects[i].SolidIntersect = ElementModel.GetIntersectSolid(ElementInterSects[i].Element, ElementSet.Element);
            }
        }

        public void GetClashPoints(Document document, UnitProject unit)
        {
            for (int i = 0; i < ElementInterSects.Count; i++)
            {
                ElementInterSects[i].GetClashPoint(document, unit);
            }
        }
        public void GetImageSource(Document document,string folderName)
        {
            for (int i = 0; i < ElementInterSects.Count; i++)
            {
                ElementInterSects[i].ImageSource = ElementInterSects[i].GetImageSource(document, ElementSet.Element, folderName);
            }
        }
        #region CreateView3D

        //private View3D CreateView3D(Document document, ViewFamilyType viewFamilyType, Element element)
        //{
        //    View3D v = View3D.CreateIsometric(document, viewFamilyType.Id);
        //    v.Name = element.Id.ToString();
        //}
        private ViewFamilyType GetView3DFamilyType(Document document)
        {
            ViewFamilyType viewFamilyType= new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().FirstOrDefault(x =>x.ViewFamily == ViewFamily.ThreeDimensional);
            return viewFamilyType;
        }
        #endregion
    }
}
