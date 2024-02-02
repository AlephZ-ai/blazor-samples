using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.PlayHT
{
    public static class Constants
    {

        public const int GRPC_STREAMING_LIMITS_MAX_NUMBER_OF_LINES = 6;
        public const int GRPC_STREAMING_LIMITS_LINE_MAX_LENGTH = 300;
        public const int GRPC_STREAMING_LIMITS_LINE_DESIRED_LENGTH = 200;
        public const int STREAM_SENTENCE_AGGREGATE_TIMEOUT = 150;
        public const string PUNCTUATION_REGEX = @"/[.!?:…\r\n]/m";
        public const string API_URL = "https://api.play.ht/api";
    }
}
