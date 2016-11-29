using System.Diagnostics;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public struct RiffItem
    {
        public const int ITEM_HEADER_SIZE = 2 * sizeof(uint);

        private readonly long dataStart;
        private int dataSize;

        public RiffItem(long dataStart, int dataSize = -1)
        {
            Debug.Assert(dataStart >= ITEM_HEADER_SIZE);
            Debug.Assert(dataSize <= int.MaxValue - ITEM_HEADER_SIZE);

            this.dataStart = dataStart;
            this.dataSize = dataSize;
        }

        public long DataStart
        {
            get { return dataStart; }
        }

        public long ItemStart
        {
            get { return dataStart - ITEM_HEADER_SIZE; }
        }

        public long DataSizeStart
        {
            get { return dataStart - sizeof(uint); }
        }

        public int DataSize
        {
            get
            {
                return dataSize;
            }

            set
            {
                Debug.Assert(value >= 0);
                Debug.Assert(DataSize < 0);

                dataSize = value;
            }
        }

        public int ItemSize
        {
            get { return dataSize < 0 ? -1 : dataSize + ITEM_HEADER_SIZE; }
        }
    }
}
