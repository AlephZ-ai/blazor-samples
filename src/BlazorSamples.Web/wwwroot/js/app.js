// scripts/app.ts
var recorder;
var mediaSource;
var sourceBuffer;
var audioElement;
function startMediaSource() {
  if (!mediaSource) {
    audioElement = document.getElementById("audioElement");
    mediaSource = new MediaSource();
    audioElement.src = URL.createObjectURL(mediaSource);
    mediaSource.addEventListener("sourceopen", () => {
      sourceBuffer = mediaSource.addSourceBuffer("audio/mpeg");
      audioElement?.play();
    }, { once: true });
  }
}
function appendMediaSourceBuffer(buffer) {
  sourceBuffer?.appendBuffer(buffer);
}
function stopMediaSource() {
  if (mediaSource) {
    audioElement?.pause();
    mediaSource.endOfStream();
    audioElement = null;
    mediaSource = null;
  }
}
async function getAudioInputDevices() {
  try {
    const devices = await navigator.mediaDevices.enumerateDevices();
    return devices.filter((device) => device.kind === "audioinput").map((device) => ({
      DeviceId: device.deviceId,
      Label: device.label || "Unknown Audio Device",
      Kind: device.kind,
      GroupId: device.groupId
    }));
  } catch (error) {
    console.error("Error fetching audio input devices", error);
    return [];
  }
}
async function requestMicrophonePermission() {
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
    stream.getTracks().forEach((track) => track.stop());
    return true;
  } catch (error) {
    return false;
  }
}
function getSupportedMimeType() {
  const types = ["audio/webm", "audio/mp4"];
  for (const type of types) {
    if (MediaRecorder.isTypeSupported(type)) {
      return type;
    }
  }
  return null;
}
async function startRecording(page, deviceId) {
  let mimeType = getSupportedMimeType();
  if (mimeType) {
    const constraints = { audio: { deviceId } };
    const options = { mimeType };
    const stream = await navigator.mediaDevices.getUserMedia(constraints);
    recorder = new MediaRecorder(stream, options);
    mimeType = recorder.mimeType;
    const track = stream.getAudioTracks()[0];
    const settings = track.getSettings();
    const { channelCount, sampleRate } = settings;
    console.log(recorder);
    console.log(stream);
    console.log(track);
    console.log(settings);
    let stopped = false;
    recorder.ondataavailable = async (e) => {
      const data = await e.data.arrayBuffer();
      const uint8Array = new Uint8Array(data);
      await page.invokeMethodAsync("DataAvailable", uint8Array, mimeType);
      if (stopped) {
        await page.invokeMethodAsync("RecordingStopped");
      }
    };
    recorder.onstop = () => {
      stopped = true;
    };
    recorder.start(500);
  } else {
    console.error("No supported audio formats found");
  }
  return mimeType;
}
function stopRecording() {
  if (recorder) {
    recorder.stop();
    recorder.stream.getTracks().forEach((track) => track.stop());
    recorder = null;
  }
}
export {
  appendMediaSourceBuffer,
  getAudioInputDevices,
  getSupportedMimeType,
  requestMicrophonePermission,
  startMediaSource,
  startRecording,
  stopMediaSource,
  stopRecording
};
//# sourceMappingURL=app.js.map
