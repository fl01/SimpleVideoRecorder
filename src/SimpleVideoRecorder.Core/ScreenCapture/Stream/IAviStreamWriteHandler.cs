using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public interface IAviStreamWriteHandler
    {
        void WriteVideoFrame(AviVideoStream stream, bool isKeyFrame, byte[] frameData, int startIndex, int count);

        //void WriteAudioBlock(AviAudioStream stream, byte[] blockData, int startIndex, int count);

        void WriteStreamHeader(AviVideoStream stream);

        //void WriteStreamHeader(AviAudioStream stream);

        void WriteStreamFormat(AviVideoStream stream);

        //void WriteStreamFormat(AviAudioStream stream);
    }
}
