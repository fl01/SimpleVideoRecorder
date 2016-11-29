using System.Windows;
using System.Windows.Shapes;
using SimpleVideoRecorder.Client.Common;

namespace SimpleVideoRecorder.Client.View
{
    public partial class SelectRecordAreaView : Window, ISelectRecordAreaView, IShowMouseSelectionView
    {
        public Rectangle SelectionRectangle
        {
            get { return MouseSelection; }
        }

        public IShowSelectionViewModel SelectionViewModel
        {
            get { return DataContext as IShowSelectionViewModel; }
        }

        public SelectRecordAreaView()
        {
            InitializeComponent();
        }
    }
}
