using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorSamples.Shared.SpeechToText;

namespace BlazorSamples.Shared.Clients
{
    public interface ISpeechToTextClient
    {
        Task ReceiveResult(RegularResult result);
        Task ReceivePartialResult(PartialResult message);
        Task ReceiveFinalResult(FinalResult message);
    }
}
