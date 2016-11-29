using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using SimpleVideoRecorder.Core.ScreenCapture.Stream;
using SimpleVideoRecorder.Core.ScreenDetails;

namespace SimpleVideoRecorder.Core.ScreenCapture
{
    public class RecordingService : IRecordingService
    {
        private readonly AutoResetEvent videoFrameWritten = new AutoResetEvent(false);
        private readonly AutoResetEvent audioBlockWritten = new AutoResetEvent(false);
        private readonly ManualResetEvent stopThread = new ManualResetEvent(false);
        private readonly IAviVideoStream videoStream;
        private readonly IAviAudioStream audioStream;
        private readonly IAviVideoWriter videoWriter;
        private readonly WaveInEvent audioSource;
        private readonly Thread recordThread;
        private readonly ScreenMetadata targetScreen;
        private readonly RegionBlock recordBlock;

        public RecordingService(ScreenMetadata targetScreen, RegionBlock recordBlock, FourCC codec, int quality)
        {
            Debug.Assert(targetScreen != null);
            Debug.Assert(recordBlock != null);

            this.targetScreen = targetScreen;
            this.recordBlock = recordBlock;

            videoWriter = new AviWriter($"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.avi", 10)
            {
                EmitIndex1 = true
            };

            videoStream = CreateVideoStream(codec, quality);
            videoStream.Name = "Screencast";

            recordThread = new Thread(RecordScreen)
            {
                Name = typeof(RecordingService).Name + ".RecordScreen",
                IsBackground = true
            };
        }

        public void Start()
        {
            recordThread.Start();
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            stopThread.Set();
            recordThread.Join();
            if (audioSource != null)
            {
                audioSource.StopRecording();
                audioSource.DataAvailable -= OnAudioDataAvailable;
            }

            videoWriter.Close();
            stopThread.Close();
        }

        private void OnAudioDataAvailable(object sender, WaveInEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void RecordScreen()
        {
            var stopwatch = new Stopwatch();
            var buffer = new byte[recordBlock.Width * recordBlock.Height * 4];

            Task videoWriteTask = null;

            var isFirstFrame = true;
            var shotsTaken = 0;
            var timeTillNextFrame = TimeSpan.Zero;
            stopwatch.Start();

            while (!stopThread.WaitOne(timeTillNextFrame))
            {
                GetScreenshot(buffer);
                shotsTaken++;

                // Wait for the previous frame is written
                if (!isFirstFrame)
                {
                    videoWriteTask.Wait();
                    videoFrameWritten.Set();
                }

                if (audioStream != null)
                {
                    var signalled = WaitHandle.WaitAny(new WaitHandle[] { audioBlockWritten, stopThread });
                    if (signalled == 1)
                        break;
                }

                // Start asynchronous (encoding and) writing of the new frame
                videoWriteTask = videoStream.WriteFrameAsync(true, buffer, 0, buffer.Length);

                timeTillNextFrame = TimeSpan.FromSeconds(shotsTaken / (double)videoWriter.FramesPerSecond - stopwatch.Elapsed.TotalSeconds);
                if (timeTillNextFrame < TimeSpan.Zero)
                {
                    timeTillNextFrame = TimeSpan.Zero;
                }

                isFirstFrame = false;
            }

            stopwatch.Stop();

            // Wait for the last frame is written
            if (!isFirstFrame)
            {
                videoWriteTask.Wait();
            }
        }

        private void GetScreenshot(byte[] buffer)
        {
            using (var bitmap = new Bitmap(recordBlock.Width, recordBlock.Height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(recordBlock.X, recordBlock.Y, 0, 0, new System.Drawing.Size(recordBlock.Width, recordBlock.Height));
                var bits = bitmap.LockBits(new Rectangle(0, 0, recordBlock.Width, recordBlock.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
                Marshal.Copy(bits.Scan0, buffer, 0, buffer.Length);
                bitmap.UnlockBits(bits);

                // Should also capture the mouse cursor here, but skipping for simplicity
                // For those who are interested, look at http://www.codeproject.com/Articles/12850/Capturing-the-Desktop-Screen-with-the-Mouse-Cursor
            }
        }

        private IAviVideoStream CreateVideoStream(FourCC codec, int quality)
        {
            if (codec == KnownFourCC.Codecs.MotionJpeg)
            {
                return videoWriter.AddMotionJpegVideoStream(recordBlock.Width, recordBlock.Height, quality);
            }

            throw new NotSupportedException($"Codec {codec.Code} is not supported");
        }
    }
}
