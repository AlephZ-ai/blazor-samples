using BlazorSamples.Shared.Twilio.GrpcAudioStream.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSamples.Shared.Twilio.GrpcAudioStream.Abstractions
{
    public interface IMediaEvent<T> : IStreamSidEvent
        where T : IMedia
    {
        [Required]
        [JsonPropertyOrder(101)]
        [JsonPropertyName(IMedia.MEDIA)]
        public T Media { get; }
    }
}
