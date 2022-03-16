using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCustomControls;
using Autodesk.Revit.DB;
namespace CheckInterSect
{
    public class ElementModel  :BaseViewModel
    {
        private static readonly BuiltInParameter TypeID = (BuiltInParameter)(-1002050);
        private Element _Element;
        public Element Element { get { return _Element; } set { _Element = value; OnPropertyChanged(); } }

        private ElementType _ElementType;
        public ElementType ElementType { get { return _ElementType; } set { _ElementType = value; OnPropertyChanged(); } }

        public ElementModel(Document document, Element element)
        {
            Element = element;
            ElementType = document.GetElement(Element.get_Parameter(TypeID).AsElementId()) as ElementType;
        }
    }
}
