using System.Collections.Generic;

namespace SimpleVideoRecorder.Core.ScreenDetails
{
    public interface IScreenMetadataService
    {
        IReadOnlyCollection<ScreenMetadata> GetActiveScreens();
    }
}
