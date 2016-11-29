using SimpleVideoRecorder.Core.ScreenCapture.Codec;
using SimpleVideoRecorder.Core.ScreenCapture.Stream;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public interface IAviVideoWriter
    {
        IAviVideoStream AddMotionJpegVideoStream(int width, int height, int quality);

        IAviVideoStream AddEncodingVideoStream(IVideoEncoder encoder, int width, int height, bool ownsEncoder);

        void Close();

        decimal FramesPerSecond { get; }
    }
}
