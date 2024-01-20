import { MediaRecorder, register } from 'extendable-media-recorder';
import { connect } from 'extendable-media-recorder-wav-encoder';
await register(await connect());
// TODO: Fix any type to MediaRecorder, https://github.com/chrisguttandin/extendable-media-recorder causing issues
let recorder;
export async function getAudioInputDevices() {
    try {
        const devices = await navigator.mediaDevices.enumerateDevices();
        return devices.filter((device) => device.kind === 'audioinput').map((device) => ({
            DeviceId: device.deviceId,
            Label: device.label || 'Unknown Audio Device',
            Kind: device.kind,
            GroupId: device.groupId
        }));
    }
    catch (error) {
        console.error('Error fetching audio input devices', error);
        return [];
    }
}
export async function requestMicrophonePermission() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        stream.getTracks().forEach((track) => track.stop());
        return true;
    }
    catch (error) {
        return false;
    }
}
export function getSupportedMimeType() {
    // audio/webm=chrome/edge/firefox, audio/mp4=safari
    const types = ['audio/wav'];
    for (const type of types) {
        if (MediaRecorder.isTypeSupported(type)) {
            return type;
        }
    }
    return null;
}
// TODO: Fix to reference blazor.d.ts and use DotNet.DotNetObject instead of any for page
export async function startRecording(page, deviceId) {
    const mimeType = getSupportedMimeType();
    if (mimeType) {
        const constraints = { audio: { deviceId: deviceId, channelCount: 1 } };
        const options = { mimeType: mimeType, audioBitsPerSecond: 16000 };
        const stream = await navigator.mediaDevices.getUserMedia(constraints);
        recorder = new MediaRecorder(stream, options);
        let stopped = false;
        recorder.addEventListener('dataavailable', async (e) => {
            const buffer = await e.data.arrayBuffer();
            const base64 = btoa(String.fromCharCode.apply(null, new Uint8Array(buffer)));
            await page.invokeMethodAsync("DataAvailable", base64);
            if (stopped) {
                await page.invokeMethodAsync("RecordingStopped");
            }
        });
        recorder.addEventListener("stop", () => {
            stopped = true;
        });
        recorder.start(1000);
    }
    else {
        console.error('No supported audio formats found');
    }
    return mimeType;
}
// TODO: Check if mic still hangs open in Mac
export function stopRecording() {
    recorder.stop();
    recorder.stream.getTracks().forEach((track) => track.stop());
    recorder = null;
}
//# sourceMappingURL=app.js.map