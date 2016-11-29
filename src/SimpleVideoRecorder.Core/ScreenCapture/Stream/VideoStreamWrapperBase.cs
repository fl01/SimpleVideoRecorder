using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public abstract class VideoStreamWrapperBase : IAviVideoStreamInternal, IDisposable
    {
        private readonly IAviVideoStreamInternal baseStream;

        public virtual int Width
        {
            get { return baseStream.Width; }
            set { baseStream.Width = value; }
        }

        public virtual int Height
        {
            get { return baseStream.Height; }
            set { baseStream.Height = value; }
        }

        public virtual BitsPerPixel BitsPerPixel
        {
            get { return baseStream.BitsPerPixel; }
            set { baseStream.BitsPerPixel = value; }
        }

        public virtual FourCC Codec
        {
            get { return baseStream.Codec; }
            set { baseStream.Codec = value; }
        }

        protected IAviVideoStreamInternal BaseStream
        {
            get { return baseStream; }
        }

        public VideoStreamWrapperBase(IAviVideoStreamInternal baseStream)
        {
            Debug.Assert(baseStream != null);

            this.baseStream = baseStream;
        }

        public virtual void Dispose()
        {
            var baseStreamDisposable = baseStream as IDisposable;
            baseStreamDisposable?.Dispose();
        }

        public virtual void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            baseStream.WriteFrame(isKeyFrame, frameData, startIndex, length);
        }
        public virtual System.Threading.Tasks.Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            return baseStream.WriteFrameAsync(isKeyFrame, frameData, startIndex, length);
        }

        public int FramesWritten
        {
            get { return baseStream.FramesWritten; }
        }

        public int Index
        {
            get { return baseStream.Index; }
        }

        public virtual string Name
        {
            get { return baseStream.Name; }
            set { baseStream.Name = value; }
        }

        public FourCC StreamType
        {
            get { return baseStream.StreamType; }
        }

        public FourCC ChunkId
        {
            get { return baseStream.ChunkId; }
        }

        public virtual void PrepareForWriting()
        {
            baseStream.PrepareForWriting();
        }

        public virtual void FinishWriting()
        {
            baseStream.FinishWriting();
        }

        public void WriteHeader()
        {
            baseStream.WriteHeader();
        }

        public void WriteFormat()
        {
            baseStream.WriteFormat();
        }
    }
}
