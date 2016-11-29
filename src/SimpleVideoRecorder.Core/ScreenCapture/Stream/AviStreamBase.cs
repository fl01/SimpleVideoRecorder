using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public abstract class AviStreamBase : IAviStream, IAviStreamInternal
    {
        private bool isFrozen;
        private readonly int index;
        private string name;
        private FourCC chunkId;

        public int Index
        {
            get { return index; }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                CheckNotFrozen();
                name = value;
            }
        }

        public abstract FourCC StreamType { get; }

        public FourCC ChunkId
        {
            get
            {
                if (!isFrozen)
                {
                    throw new InvalidOperationException("Chunk ID is not defined until the stream is frozen.");
                }

                return chunkId;
            }
        }

        public AviStreamBase(int index)
        {
            this.index = index;
        }

        
        public abstract void WriteHeader();

        public abstract void WriteFormat();

        public virtual void PrepareForWriting()
        {
            if (!isFrozen)
            {
                isFrozen = true;

                chunkId = GenerateChunkId();
            }
        }

        public virtual void FinishWriting()
        {
        }


        protected abstract FourCC GenerateChunkId();

        protected void CheckNotFrozen()
        {
            if (isFrozen)
            {
                throw new InvalidOperationException("No stream information can be changed after starting to write frames.");
            }
        }
    }
}
