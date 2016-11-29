using System;

namespace SimpleVideoRecorder.Client.Common
{
    public interface INavigationService
    {
        void ShowDialog<TView>(ShowWindowBehavior behavior, Func<IWindowView> factoryMethod)
            where TView : IWindowView;
    }
}
