using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public interface IAviVideoStream : IAviStream
    {
        int Width { get; set; }

        int Height { get; set; }

        BitsPerPixel BitsPerPixel { get; set; }

        FourCC Codec { get; set; }

        void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length);

        Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length);

        int FramesWritten { get; }
    }
}
