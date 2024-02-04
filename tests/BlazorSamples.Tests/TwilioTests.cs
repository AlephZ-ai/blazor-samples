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
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using BlazorSamples.Shared.Twilio.GrpcAudioStream;

namespace BlazorSamples.Tests
{
    [TestClass]
    public class TwilioTests
    {
        private static readonly string _base = "twilio";
        private static TestContext _context = null!;
        private static JsonSerializerOptions _options = null!;
    
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _context = context;
            _options = JsonSerializerOptions.Default;
            _options = new() { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
        }

        [TestMethod]
        public async Task StrongJsonTest()
        {
            var testHandler = new TestTwilioHandler();
            var defaultProcessor = new DefaultTwilioProcessor();
            var inConnected = JsonTest(await GetFromFile<InboundEvent, InboundConnectedEvent>("inbound-connected.json"));
            inConnected = PolymorphismTest<InboundConnectedEvent>(inConnected);
            var handlerResult = testHandler.ProcessEvent(inConnected);
            var processorResult = defaultProcessor.ProcessEvent(inConnected);
            Assert.AreEqual(null, processorResult);
            Assert.AreEqual("InboundConnectedEvent", handlerResult);
            Assert.AreEqual("InboundConnectedEvent", inConnected.GetType().Name);
            Assert.AreEqual(EventDirection.Inbound, inConnected.Direction);


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
                    Tracks = new List<string>(),
                },
            });

            Assert.IsNotNull(inStart.Start.CustomParameters);
            Assert.IsNotNull(inStart.Start.MediaFormat);
            Assert.AreEqual("audio/x-mulaw", inStart.Start.MediaFormat.Encoding);
            Assert.AreEqual(8000, inStart.Start.MediaFormat.SampleRate);
            Assert.AreEqual(1, inStart.Start.MediaFormat.Channels);

            inStart = JsonTest(await GetFromFile<InboundEvent, InboundStartEvent>("inbound-start.json"));
            inStart = PolymorphismTest<InboundStartEvent>(inStart);
            handlerResult = testHandler.ProcessEvent(inStart);
            processorResult = defaultProcessor.ProcessEvent(inStart);
            Assert.AreEqual(true, processorResult!.Payload.IsEmpty);
            Assert.AreEqual(inStart.StreamSid, processorResult!.StreamSid);
            Assert.AreEqual(inStart.Start.MediaFormat.Encoding, processorResult!.MediaFormat.Encoding);
            Assert.AreEqual(inStart.Start.MediaFormat.SampleRate, processorResult!.MediaFormat.SampleRate);
            Assert.AreEqual(inStart.Start.MediaFormat.Channels, processorResult!.MediaFormat.Channels);
            Assert.AreEqual("InboundStartEvent", handlerResult);
            Assert.AreEqual("InboundStartEvent", inStart.GetType().Name);
            Assert.AreEqual(EventDirection.Inbound, inStart.Direction);


            var inMedia = JsonTest(await GetFromFile<InboundEvent, InboundMediaEvent>("inbound-media-in-track.json"));
            inMedia = PolymorphismTest<InboundMediaEvent>(inMedia);
            handlerResult = testHandler.ProcessEvent(inMedia);
            Assert.AreEqual("InboundMediaEvent", handlerResult);
            Assert.AreEqual("InboundMediaEvent", inMedia.GetType().Name);
            Assert.AreEqual(EventDirection.Inbound, inMedia.Direction);
            inMedia = JsonTest(await GetFromFile<InboundEvent, InboundMediaEvent>("inbound-media-out-track.json"));
            inMedia = PolymorphismTest<InboundMediaEvent>(inMedia);
            processorResult = defaultProcessor.ProcessEvent(inMedia);
            CollectionAssert.AreEqual(inMedia.Media.Payload.ToArray(), processorResult!.Payload.ToArray());
            Assert.AreEqual(inMedia.StreamSid, processorResult!.StreamSid);
            Assert.AreEqual(inStart.Start.MediaFormat.Encoding, processorResult!.MediaFormat.Encoding);
            Assert.AreEqual(inStart.Start.MediaFormat.SampleRate, processorResult!.MediaFormat.SampleRate);
            Assert.AreEqual(inStart.Start.MediaFormat.Channels, processorResult!.MediaFormat.Channels);
            Assert.AreEqual("InboundMediaEvent", inMedia.GetType().Name);
            Assert.AreEqual(EventDirection.Inbound, inMedia.Direction);


            var inStop = JsonTest(await GetFromFile<InboundEvent, InboundStopEvent>("inbound-stop.json"));
            inStop = PolymorphismTest<InboundStopEvent>(inStop);
            handlerResult = testHandler.ProcessEvent(inStop);
            processorResult = defaultProcessor.ProcessEvent(inStop);
            Assert.AreEqual(null, processorResult);
            Assert.AreEqual("InboundStopEvent", handlerResult);
            Assert.AreEqual("InboundStopEvent", inStop.GetType().Name);
            Assert.AreEqual(EventDirection.Inbound, inStop.Direction);


            var inMark = JsonTest(await GetFromFile<InboundEvent, InboundMarkEvent>("inbound-mark.json"));
            inMark = PolymorphismTest<InboundMarkEvent>(inMark);
            handlerResult = testHandler.ProcessEvent(inMark);
            processorResult = defaultProcessor.ProcessEvent(inMark);
            Assert.AreEqual(null, processorResult);
            Assert.AreEqual("InboundMarkEvent", handlerResult);
            Assert.AreEqual("InboundMarkEvent", inMark.GetType().Name);
            Assert.AreEqual(EventDirection.Inbound, inMark.Direction);


            var outMedia = JsonTest(await GetFromFile<OutboundEvent, OutboundMediaEvent>("outbound-media.json"));
            outMedia = PolymorphismTest<OutboundMediaEvent>(outMedia);
            Assert.AreEqual("OutboundMediaEvent", outMedia.GetType().Name);
            Assert.AreEqual(EventDirection.Outbound, outMedia.Direction);


            var outMark = JsonTest(await GetFromFile<OutboundEvent, OutboundMarkEvent>("outbound-mark.json"));
            outMark = PolymorphismTest<OutboundMarkEvent>(outMark);
            Assert.AreEqual("OutboundMarkEvent", outMark.GetType().Name);
            Assert.AreEqual(EventDirection.Outbound, outMark.Direction);


            var outClear = JsonTest(await GetFromFile<OutboundEvent, OutboundClearEvent>("outbound-clear.json"));
            outClear = PolymorphismTest<OutboundClearEvent>(outClear);
            Assert.AreEqual("OutboundClearEvent", outClear.GetType().Name);
            Assert.AreEqual(EventDirection.Outbound, outClear.Direction);
        }

        T JsonTest<T>(T input)
        {
            var json = JsonSerializer.Serialize(input, _options);
            var output = JsonSerializer.Deserialize<T>(json, _options);
            input.ShouldCompare(output);
            return output!;
        }

        T PolymorphismTest<T>(InboundEvent input)
            where T : InboundEvent
        {
            var json = JsonSerializer.Serialize(input, _options);
            var output = JsonSerializer.Deserialize<InboundEvent>(json, _options);
            input.ShouldCompare(output);
            return (T)output!;
        }

        T PolymorphismTest<T>(OutboundEvent input)
            where T : OutboundEvent
        {
            var json = JsonSerializer.Serialize(input, _options);
            var output = JsonSerializer.Deserialize<OutboundEvent>(json, _options);
            input.ShouldCompare(output);
            return (T)output!;
        }

        async Task<U> GetFromFile<T, U>(string file)
            where U : T
        {
            await using var stream = File.OpenRead($"{_base}/{file}");
            var obj = await JsonSerializer.DeserializeAsync<T>(stream, _options);
            return (U)obj!;
        }
    }
}
