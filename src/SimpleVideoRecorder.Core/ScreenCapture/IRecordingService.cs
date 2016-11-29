using System;

namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public interface IRecordingService : IDisposable
    {
        void Start();

        void Stop();
    }
}
