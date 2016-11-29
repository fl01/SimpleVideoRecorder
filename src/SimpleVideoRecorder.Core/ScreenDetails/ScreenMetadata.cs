namespace SimpleVideoRecorder.Core.ScreenDetails
{
    public class ScreenMetadata
    {
        public int X { get; private set; }

        public int Y { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool IsPrimary { get; private set; }

        public string Name { get; set; }

        public ScreenMetadata(string name, int x, int y, int width, int height, bool isPrimary)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsPrimary = isPrimary;
            Name = name;
        }
    }
}
