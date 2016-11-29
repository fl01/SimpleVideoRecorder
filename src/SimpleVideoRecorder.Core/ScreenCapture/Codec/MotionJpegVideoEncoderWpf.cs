using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleVideoRecorder.Core.ScreenCapture.Codec
{
    public class MotionJpegVideoEncoderWpf : IVideoEncoder
    {
        private readonly Int32Rect rect;
        private readonly int quality;
        private readonly ThreadLocal<WriteableBitmap> bitmapHolder;

        public FourCC Codec
        {
            get { return KnownFourCC.Codecs.MotionJpeg; }
        }

        public BitsPerPixel BitsPerPixel
        {
            get { return BitsPerPixel.Bpp24; }
        }

        public int MaxEncodedSize
        {
            get
            {
                // Assume that JPEG is always less than raw bitmap when dimensions are not tiny
                return Math.Max(rect.Width * rect.Height * 4, 1024);
            }
        }

        public MotionJpegVideoEncoderWpf(int width, int height, int quality)
        {
            rect = new Int32Rect(0, 0, width, height);
            this.quality = quality;

            bitmapHolder = new ThreadLocal<WriteableBitmap>(
                () => new WriteableBitmap(rect.Width, rect.Height, 96, 96, PixelFormats.Bgr32, null),
                false);
        }

        public int EncodeFrame(byte[] source, int srcOffset, byte[] destination, int destOffset, out bool isKeyFrame)
        {
            WriteableBitmap bitmap = bitmapHolder.Value;

            bitmap.WritePixels(rect, source, rect.Width * 4, srcOffset);

            var encoderImpl = new JpegBitmapEncoder
            {
                QualityLevel = quality
            };

            encoderImpl.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new MemoryStream(destination))
            {
                stream.Position = srcOffset;
                encoderImpl.Save(stream);
                stream.Flush();

                var length = stream.Position - srcOffset;
                isKeyFrame = true;

                return (int)length;
            }
        }
    }
}
