namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public class RegionBlock
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public RegionBlock()
        {
        }

        public RegionBlock(int width, int height) : this(0, 0, width, height)
        {
        }

        public RegionBlock(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
