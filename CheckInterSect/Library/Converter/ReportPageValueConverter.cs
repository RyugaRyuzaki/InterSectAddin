using System;
using System.Diagnostics;
using System.Globalization;
using WpfCustomControls;

namespace CheckInterSect.Library.Converter
{
    public class ReportPageValueConverter : BaseValueConverter<ReportPageValueConverter>
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int Total = (int)values[0];
            int Current = (int)values[1];
            if (Total==0)
            {
               
                return null;
            }
            else
            {

                ReportViewModel reportViewModel = new ReportViewModel();
                return new ReportView(reportViewModel);
            }
        }

        public override object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
