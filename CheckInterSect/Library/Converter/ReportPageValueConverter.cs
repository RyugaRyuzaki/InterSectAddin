using System;
using System.Diagnostics;
using System.Globalization;
using WpfCustomControls;
using Autodesk.Revit.DB;
using System.Collections.ObjectModel;

namespace CheckInterSect.Library.Converter
{
    public class ReportPageValueConverter : BaseValueConverter<ReportPageValueConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int Total = (int)values[0];

            Document Doc = (Document)values[1];
            Document SelectedRevitLink = (Document)values[2];

            ElementModel ElementModelSet = (ElementModel)values[3];
            ObservableCollection<ElementModel> ElementModelIntersects = (ObservableCollection<ElementModel>)values[4];
            if (Total == 0 && ElementModelSet == null && ElementModelIntersects.Count == 0)
            {

                return null;
            }
            else
            {

                //ObservableCollection<ReportView> reportViews = new ObservableCollection<ReportView>();
                ReportViewModel reportViewModel = new ReportViewModel(Doc, SelectedRevitLink, ElementModelSet, ElementModelIntersects);
                return new ReportView(reportViewModel);
                //return reportViews;
                // nếu tạo ra nhiều page cùng lúc sẽ là collection
            }
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
