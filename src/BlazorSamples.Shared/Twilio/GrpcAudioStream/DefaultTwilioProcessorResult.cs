using BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Start;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream
{
    public class DefaultTwilioProcessorResult
    {
        public string StreamSid { get; init; }
        public InboundMediaFormat MediaFormat { get; init; }
        public ReadOnlyMemory<byte> Payload { get; init; }

        internal static DefaultTwilioProcessorResult? From(InboundConnectedEvent connectedEvent) => null;
        internal static DefaultTwilioProcessorResult From(InboundStartEvent startEvent) => new(startEvent.StreamSid, startEvent.Start.MediaFormat);
        internal static DefaultTwilioProcessorResult From(DefaultTwilioProcessorResult previous, InboundMediaEvent mediaEvent) => new(previous) { StreamSid = mediaEvent.StreamSid, Payload = mediaEvent.Media.Payload };
        internal static DefaultTwilioProcessorResult? From(InboundStopEvent stopEvent) => null;
        internal static DefaultTwilioProcessorResult? From(InboundMarkEvent markEvent) => null;
        private DefaultTwilioProcessorResult(string streamSid, InboundMediaFormat mediaFormat)
        {
            StreamSid = streamSid;
            MediaFormat = mediaFormat;
        }

        private DefaultTwilioProcessorResult(DefaultTwilioProcessorResult previous)
        {
            StreamSid = previous.StreamSid;
            MediaFormat = previous.MediaFormat;
        }
    }
}
