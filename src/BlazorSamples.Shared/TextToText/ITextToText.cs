using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.TextToText
{
    public interface ITextToText
    {
        public IAsyncEnumerable<string> StreamingResponse(string query, CancellationToken ct = default);
    }
}
