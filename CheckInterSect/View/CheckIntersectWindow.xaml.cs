using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CheckInterSect
{
    public partial class CheckIntersectWindow
    {
        private CheckIntersectViewModel _viewModel;

        public CheckIntersectWindow(CheckIntersectViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = viewModel;
        }



    }
}
