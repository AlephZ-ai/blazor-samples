using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Clear;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Connected;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Mark;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Start;
using BlazorSamples.Shared.Twilio.GrpcAudioStream.Stop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using KellermanSoftware.CompareNetObjects;
using Azure.Core;

namespace BlazorSamples.Tests
{
    [TestClass]
    public class TwilioTests
    {
        private static TestContext _context = null!;
    
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _context = context;
        }

        [TestMethod]
        public void StrongJsonTest()
        {
            var rand = new Random(777);
            JsonTest(new InboundConnectedEvent
            {
                Protocol = "Call",
                Version = "1.0.0",
            });

            var streamSid = Guid.NewGuid().ToString();
            JsonTest(new InboundStartEvent
            {
                SequenceNumber = 420,
                StreamSid = streamSid,
                Start = new()
                {
                    StreamSid = streamSid,
                    AccountSid = Guid.NewGuid().ToString(),
                    CallSid = Guid.NewGuid().ToString(),
                    Tracks = [ "inbound", "outbound" ],
                    CustomParameters = new(),
                    MediaFormat = new()
                    {
                        Encoding = "Encoding",
                        SampleRate = 317,
                        Channels = 13,
                    },
                },
            });

            JsonTest(new InboundMediaEvent
            {
                SequenceNumber = 69,
                StreamSid = Guid.NewGuid().ToString(),
                Media = new()
                {
                    Chunk = 42,
                    Timestamp = 1000,
                    Track = "inbound",
                    Payload = [ 0x00, 0x01, 0x02, 0x03 ],
                },
            });

            JsonTest(new InboundStopEvent
            {
                SequenceNumber = (uint)rand.Next(),
                StreamSid = Guid.NewGuid().ToString(),
                Stop = new()
                {
                    AccountSid = Guid.NewGuid().ToString(),
                    CallSid = Guid.NewGuid().ToString(),
                },
            });

            JsonTest(new InboundMarkEvent
            {
                SequenceNumber = (uint)rand.Next(),
                Mark = new()
                {
                    Name = "Mark",
                },
            });

            JsonTest(new OutboundMediaEvent
            {
                StreamSid = Guid.NewGuid().ToString(),
                Media = new()
                {
                    Payload = [ 0x03, 0x02, 0x01, 0x00 ],
                },
            });

            JsonTest(new OutboundMarkEvent
            {
                StreamSid = Guid.NewGuid().ToString(),
                Mark = new()
                {
                    Name = "Rubio",
                },
            });

            JsonTest(new OutboundClearEvent
            {
                StreamSid = Guid.NewGuid().ToString(),
            });
        }

        T JsonTest<T>(T input)
            where T : IEvent
        {
            var json = JsonSerializer.Serialize(input);
            var output = JsonSerializer.Deserialize<T>(json);
            input.ShouldCompare(output);
            return output!;
        }
    }
}
