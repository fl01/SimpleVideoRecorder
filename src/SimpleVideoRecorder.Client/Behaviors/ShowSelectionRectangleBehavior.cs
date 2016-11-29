using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using SimpleVideoRecorder.Client.Common;
using SimpleVideoRecorder.Core.ScreenCapture;

namespace SimpleVideoRecorder.Client.Behaviors
{
    public class ShowSelectionRectangleBehavior : Behavior<UIElement>
    {
        private DrawRectangle drawRectangle;

        protected override void OnAttached()
        {
            AssociatedObject.IsVisibleChanged += OnVisibleChanged;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftDown;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftUp;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.IsVisibleChanged -= OnVisibleChanged;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftDown;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftUp;
        }

        private void OnMouseLeftDown(object sender, MouseButtonEventArgs e)
        {
            var view = sender as IShowMouseSelectionView;

            if (view == null)
            {
                return;
            }

            // just in case it wasn't cleaned up for some reasons
            CleanUpSelectionDraw();

            drawRectangle = new DrawRectangle(view);
            drawRectangle.Begin(e.GetPosition(null));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (drawRectangle != null && drawRectangle.IsStarted)
            {
                drawRectangle.Update(e.GetPosition(null));
            }
        }

        private void OnMouseLeftUp(object sender, MouseButtonEventArgs e)
        {
            var view = sender as IShowMouseSelectionView;

            if (view == null)
            {
                return;
            }

            if (drawRectangle.IsStarted)
            {
                RegionBlock selectedArea = drawRectangle.End(e.GetPosition(null));
                CleanUpSelectionDraw();

                view.SelectionViewModel?.OnSelectionSet(selectedArea);
            }
        }

        private void CleanUpSelectionDraw()
        {
            drawRectangle?.Dispose();
            drawRectangle = null;
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool && !(bool)e.NewValue)
            {
                CleanUpSelectionDraw();
            }
        }
    }
}
