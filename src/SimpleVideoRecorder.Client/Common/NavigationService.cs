using System;
using System.Linq;
using System.Windows;

namespace SimpleVideoRecorder.Client.Common
{
    public class NavigationService : INavigationService
    {
        public void ShowDialog<TView>(ShowWindowBehavior behavior, Func<IWindowView> factoryMethod)
             where TView : IWindowView
        {
            if (behavior == ShowWindowBehavior.Single)
            {
                var view = Application.Current.Windows.OfType<TView>().FirstOrDefault();
                if (view != null)
                {
                    view.Activate();
                    return;
                }
            }

            factoryMethod.Invoke().ShowDialog();
        }
    }
}
