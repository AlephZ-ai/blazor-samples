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
            var inConnected = JsonTest(new InboundConnectedEvent
            {
                Protocol = "Call",
                Version = "1.0.0",
            });

            Assert.AreEqual("connected", inConnected.EventType);
            Assert.AreEqual(EventDirection.Inbound, inConnected.Direction);
            PolymorphismTest(inConnected);


            var streamSid = Guid.NewGuid().ToString();
            var inStart = JsonTest(new InboundStartEvent
            {
                SequenceNumber = 420,
                StreamSid = streamSid,
                Start = new()
                {
                    StreamSid = streamSid,
                    AccountSid = Guid.NewGuid().ToString(),
                    CallSid = Guid.NewGuid().ToString(),
                    Tracks = [],
                },
            });

            Assert.IsNotNull(inStart.Start.CustomParameters);
            Assert.IsNotNull(inStart.Start.MediaFormat);
            Assert.AreEqual("audio/x-mulaw", inStart.Start.MediaFormat.Encoding);
            Assert.AreEqual(8000, inStart.Start.MediaFormat.SampleRate);
            Assert.AreEqual(1, inStart.Start.MediaFormat.Channels);

            inStart = JsonTest(new InboundStartEvent
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

            Assert.AreEqual("start", inStart.EventType);
            Assert.AreEqual(EventDirection.Inbound, inStart.Direction);
            PolymorphismTest(inStart);


            var inMedia = JsonTest(new InboundMediaEvent
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

            Assert.AreEqual("media", inMedia.EventType);
            Assert.AreEqual(EventDirection.Inbound, inMedia.Direction);
            PolymorphismTest(inMedia);


            var inStop = JsonTest(new InboundStopEvent
            {
                SequenceNumber = (uint)rand.Next(),
                StreamSid = Guid.NewGuid().ToString(),
                Stop = new()
                {
                    AccountSid = Guid.NewGuid().ToString(),
                    CallSid = Guid.NewGuid().ToString(),
                },
            });

            Assert.AreEqual("stop", inStop.EventType);
            Assert.AreEqual(EventDirection.Inbound, inStop.Direction);
            PolymorphismTest(inStop);


            var inMark = JsonTest(new InboundMarkEvent
            {
                SequenceNumber = (uint)rand.Next(),
                Mark = new()
                {
                    Name = "Mark",
                },
            });

            Assert.AreEqual("mark", inMark.EventType);
            Assert.AreEqual(EventDirection.Inbound, inMark.Direction);
            PolymorphismTest(inMark);


            var outMedia = JsonTest(new OutboundMediaEvent
            {
                StreamSid = Guid.NewGuid().ToString(),
                Media = new()
                {
                    Payload = [ 0x03, 0x02, 0x01, 0x00 ],
                },
            });

            Assert.AreEqual("media", outMedia.EventType);
            Assert.AreEqual(EventDirection.Outbound, outMedia.Direction);
            PolymorphismTest(outMedia);


            var outMark = JsonTest(new OutboundMarkEvent
            {
                StreamSid = Guid.NewGuid().ToString(),
                Mark = new()
                {
                    Name = "Rubio",
                },
            });

            Assert.AreEqual("mark", outMark.EventType);
            Assert.AreEqual(EventDirection.Outbound, outMark.Direction);
            PolymorphismTest(outMark);


            var outClear = JsonTest(new OutboundClearEvent
            {
                StreamSid = Guid.NewGuid().ToString(),
            });

            Assert.AreEqual("clear", outClear.EventType);
            Assert.AreEqual(EventDirection.Outbound, outClear.Direction);
            PolymorphismTest(outClear);
        }

        T JsonTest<T>(T input)
            where T : IEvent
        {
            var json = JsonSerializer.Serialize(input);
            var output = JsonSerializer.Deserialize<T>(json);
            input.ShouldCompare(output);
            return output!;
        }

        void PolymorphismTest(IInboundEvent input)
        {
            var json = JsonSerializer.Serialize(input);
            var output = JsonSerializer.Deserialize<IInboundEvent>(json);
            input.ShouldCompare(output);
        }

        void PolymorphismTest(IOutboundEvent input)
        {
            var json = JsonSerializer.Serialize(input);
            var output = JsonSerializer.Deserialize<IOutboundEvent>(json);
            input.ShouldCompare(output);
        }
    }
}
