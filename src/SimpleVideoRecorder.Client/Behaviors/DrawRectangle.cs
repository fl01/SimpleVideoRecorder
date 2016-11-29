using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SimpleVideoRecorder.Client.Common;
using SimpleVideoRecorder.Core.ScreenCapture;

namespace SimpleVideoRecorder.Client.Behaviors
{
    public class DrawRectangle : IDisposable
    {
        private readonly IShowMouseSelectionView view;
        private Point startPoint;

        public bool IsStarted { get; private set; }

        public DrawRectangle(IShowMouseSelectionView view)
        {
            this.view = view;
        }

        public void Begin(Point startPoint)
        {
            this.startPoint = startPoint;

            IsStarted = true;
        }

        public void Update(Point point)
        {
            if (IsStarted)
            {
                view.SelectionRectangle.SetValue(Canvas.LeftProperty, Math.Min(point.X, startPoint.X));
                view.SelectionRectangle.SetValue(Canvas.TopProperty, Math.Min(point.Y, startPoint.Y));

                view.SelectionRectangle.Width = Math.Abs(point.X - startPoint.X);
                view.SelectionRectangle.Height = Math.Abs(point.Y - startPoint.Y);

                if (view.SelectionRectangle.Visibility != Visibility.Visible)
                {
                    view.SelectionRectangle.Visibility = Visibility.Visible;
                }
            }
        }

        public RegionBlock End(Point endPoint)
        {
            int x = (int)Math.Min(startPoint.X, endPoint.X);
            int width = (int)Math.Abs(endPoint.X - startPoint.X);
            int y = (int)Math.Min(startPoint.Y, endPoint.Y);
            int height = (int)Math.Abs(endPoint.Y - startPoint.Y);

            var result = new RegionBlock(x, y, width, height);

            return result;
        }

        public void Dispose()
        {
            EndSelection();
        }

        private void EndSelection()
        {
            view.SelectionRectangle.SetValue(Canvas.LeftProperty, 0.0);
            view.SelectionRectangle.SetValue(Canvas.TopProperty, 0.0);

            view.SelectionRectangle.Width = 0.0;
            view.SelectionRectangle.Height = 0.0;
            view.SelectionRectangle.Visibility = Visibility.Collapsed;

            IsStarted = false;
        }
    }
}
