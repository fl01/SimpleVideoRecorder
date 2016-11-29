using SimpleVideoRecorder.Core.ScreenCapture;

namespace SimpleVideoRecorder.Client.Common
{
    public interface IShowSelectionViewModel
    {
        void OnSelectionSet(RegionBlock selectedRegion);
    }
}
