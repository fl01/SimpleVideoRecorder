using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public interface IAviStreamInternal : IAviStream
    {
        FourCC StreamType { get; }

        FourCC ChunkId { get; }

        void PrepareForWriting();

        void FinishWriting();

        void WriteHeader();

        void WriteFormat();
    }
}
