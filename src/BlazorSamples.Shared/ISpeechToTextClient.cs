﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public interface ISpeechToTextClient
    {
        Task ReceiveMessage(string message);
    }
}