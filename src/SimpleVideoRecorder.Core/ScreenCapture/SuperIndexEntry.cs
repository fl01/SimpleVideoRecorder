namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public sealed class SuperIndexEntry
    {
        public long ChunkOffset { get; set; }

        public int ChunkSize { get; set; }

        public int Duration { get; set; }
    }
}
