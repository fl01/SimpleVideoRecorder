﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleVideoRecorder.Core.ScreenCapture.Stream
{
    public interface IAviStream
    {
        int Index { get; }

        string Name { get; set; }
    }
}
