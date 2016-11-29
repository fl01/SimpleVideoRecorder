namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public static class KnownFourCC
    {
        public static class Codecs
        {
            public static readonly FourCC Uncompressed = new FourCC(0);

            public static readonly FourCC MotionJpeg = new FourCC("MJPG");
        }

        public static class StreamTypes
        {
            public static readonly FourCC Video = new FourCC("vids");

            public static readonly FourCC Audio = new FourCC("auds");
        }

        public static class ListTypes
        {
            public static readonly FourCC Riff = new FourCC("RIFF");

            public static readonly FourCC List = new FourCC("LIST");
        }

        public static class Lists
        {
            public static readonly FourCC Avi = new FourCC("AVI");

            public static readonly FourCC AviExtended = new FourCC("AVIX");

            public static readonly FourCC Header = new FourCC("hdrl");

            public static readonly FourCC Stream = new FourCC("strl");

            public static readonly FourCC OpenDml = new FourCC("odml");

            public static readonly FourCC Movie = new FourCC("movi");
        }

        public static class Chunks
        {
            public static readonly FourCC AviHeader = new FourCC("avih");

            public static readonly FourCC StreamHeader = new FourCC("strh");

            public static readonly FourCC StreamFormat = new FourCC("strf");

            public static readonly FourCC StreamName = new FourCC("strn");

            public static readonly FourCC StreamIndex = new FourCC("indx");

            public static readonly FourCC Index1 = new FourCC("idx1");

            public static readonly FourCC OpenDmlHeader = new FourCC("dmlh");

            public static readonly FourCC Junk = new FourCC("JUNK");

            public static FourCC VideoFrame(int streamIndex, bool compressed)
            {
                return string.Format(compressed ? "{0:00}dc" : "{0:00}db", streamIndex);
            }

            public static FourCC AudioData(int streamIndex)
            {
                return string.Format("{0:00}wb", streamIndex);
            }

            public static FourCC IndexData(int streamIndex)
            {
                return string.Format("ix{0:00}", streamIndex);
            }
        }
    }
}
