using System.Windows.Shapes;

namespace SimpleVideoRecorder.Client.Common
{
    public interface IShowMouseSelectionView
    {
        Rectangle SelectionRectangle { get; }

        IShowSelectionViewModel SelectionViewModel { get; }
    }
}
