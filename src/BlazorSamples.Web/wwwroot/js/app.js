// scripts/app.ts
var recorder;
function startMediaSource(page) {
  const audioElement = document.getElementById("audioElement");
  const mediaSource = new MediaSource();
  mediaSource.addEventListener("sourceopen", () => {
    const sourceBuffer = mediaSource.addSourceBuffer("audio/mpeg");
    readBufferChunks(page, sourceBuffer);
    audioElement.play();
  }, { once: true });
  audioElement.src = URL.createObjectURL(mediaSource);
  window.onmouseup = () => {
    page.invokeMethod("EndSpeaking");
    stopRecording();
  };
}
async function readBufferChunks(page, sourceBuffer) {
  const chunk = await page.invokeMethodAsync("Pop");
  if (!chunk)
    return;
  sourceBuffer.appendBuffer(chunk);
  sourceBuffer.addEventListener("updateend", () => readBufferChunks(page, sourceBuffer), { once: true });
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
    stopRecording();
    recorder = new MediaRecorder(stream, options);
    mimeType = recorder.mimeType;
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
  getAudioInputDevices,
  getSupportedMimeType,
  requestMicrophonePermission,
  startMediaSource,
  startRecording,
  stopRecording
};
//# sourceMappingURL=app.js.map
