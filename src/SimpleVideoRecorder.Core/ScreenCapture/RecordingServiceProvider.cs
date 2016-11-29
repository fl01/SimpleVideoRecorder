using System.Linq;
using SimpleVideoRecorder.Core.ScreenDetails;

namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public class RecordingServiceProvider : IRecordingServiceProvider
    {
        private readonly IScreenMetadataService metadataService;

        public RecordingServiceProvider(IScreenMetadataService metadataService)
        {
            this.metadataService = metadataService;
        }

        public IRecordingService Create(RegionBlock recordBlock, FourCC codec, int quality)
        {
            return new RecordingService(metadataService.GetActiveScreens().First(), recordBlock, codec, quality);
        }
    }
}
