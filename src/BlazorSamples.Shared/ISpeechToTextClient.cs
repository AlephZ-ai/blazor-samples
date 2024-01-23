﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public interface ISpeechToTextClient
    {
        Task ReceiveResult(RegularResult result);
        Task ReceivePartialResult(PartialResult message);
        Task ReceiveFinalResult(FinalResult message);
    }
}
