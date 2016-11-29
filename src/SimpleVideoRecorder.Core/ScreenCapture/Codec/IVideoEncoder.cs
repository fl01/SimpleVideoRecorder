using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture.Codec
{
    public interface IVideoEncoder
    {
        FourCC Codec { get; }

        BitsPerPixel BitsPerPixel { get; }

        int MaxEncodedSize { get; }

        int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame);
    }
}
