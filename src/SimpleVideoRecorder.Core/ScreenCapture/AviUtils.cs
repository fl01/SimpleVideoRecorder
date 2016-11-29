using System;

namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public static class AviUtils
    {
        public static void SplitFrameRate(decimal frameRate, out uint rate, out uint scale)
        {
            if (Decimal.Round(frameRate) == frameRate)
            {
                rate = (uint)Decimal.Truncate(frameRate);
                scale = 1;
            }
            else if (Decimal.Round(frameRate, 1) == frameRate)
            {
                rate = (uint)Decimal.Truncate(frameRate * 10m);
                scale = 10;
            }
            else if (Decimal.Round(frameRate, 2) == frameRate)
            {
                rate = (uint)Decimal.Truncate(frameRate * 100m);
                scale = 100;
            }
            else
            {
                rate = (uint)Decimal.Truncate(frameRate * 1000m);
                scale = 1000;
            }

            // Make mutually prime (needed for some hardware players)
            while (rate % 2 == 0 && scale % 2 == 0)
            {
                rate /= 2;
                scale /= 2;
            }
            while (rate % 5 == 0 && scale % 5 == 0)
            {
                rate /= 5;
                scale /= 5;
            }
        }
    }
}
