using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public interface IRecordingServiceProvider
    {
        IRecordingService Create(RegionBlock recordBlock, FourCC codec, int quality);
    }
}
