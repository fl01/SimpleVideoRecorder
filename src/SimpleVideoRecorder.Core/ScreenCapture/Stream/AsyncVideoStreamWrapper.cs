﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public class AsyncVideoStreamWrapper : VideoStreamWrapperBase
    {
        private readonly SequentialInvoker writeInvoker = new SequentialInvoker();

        public AsyncVideoStreamWrapper(IAviVideoStreamInternal baseStream)
            : base(baseStream)
        {
        }

        public override void WriteFrame(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            writeInvoker.Invoke(() => base.WriteFrame(isKeyFrame, frameData, startIndex, length));
        }
        public override Task WriteFrameAsync(bool isKeyFrame, byte[] frameData, int startIndex, int length)
        {
            return writeInvoker.InvokeAsync(() => base.WriteFrame(isKeyFrame, frameData, startIndex, length));
        }

        public override void FinishWriting()
        {
            // Perform all pending writes and then let the base stream to finish
            // (possibly writing some more data synchronously)
            writeInvoker.WaitForPendingInvocations();

            base.FinishWriting();
        }
    }
}
