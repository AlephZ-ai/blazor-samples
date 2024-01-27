﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.TextToSpeech
{
    public interface ITextToSpeech
    {
        IAsyncEnumerable<byte[]> Voice(string text, CancellationToken ct = default);
    }
}