﻿using BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions;
        {
            EventType = EVENT_TYPE;
            Direction = EventDirection.Inbound;
        }

        T IInboundEvent.RunProcessor<T>(IInboundEventProcessor<T> processor) => processor.Handle(this);
    }