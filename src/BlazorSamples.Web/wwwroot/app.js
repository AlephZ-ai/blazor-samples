async function getAudioInputDevices() {
    try {
        const devices = await navigator.mediaDevices.enumerateDevices();
        return devices.filter(device => device.kind === 'audioinput').map(device => ({
            DeviceId: device.deviceId,
            Label: device.label || 'Unknown Audio Device',
            Kind: device.kind,
            GroupId: device.groupId
        }));
    } catch (error) {
        console.error('Error fetching audio input devices', error);
        return [];
    }
}

async function requestMicrophonePermission() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        stream.getTracks().forEach(track => track.stop());
        return true;
    } catch (error) {
        return false;
    }
}

function getSupportedMimeType() {
    const types = ['audio/webm', 'audio/mp4'];

    for (const type of types) {
        if (MediaRecorder.isTypeSupported(type)) {
            return type;
        }
    }
    return null;
}

let recorder = null;
async function startRecording(dotNetObject, deviceId) {
    // TODO: Use https://github.com/chrisguttandin/extendable-media-recorder
    const mimeType = getSupportedMimeType();
    if (mimeType) {
        const constraints = { audio: { deviceId: deviceId, channelCount: 1 } };
        const options = { mimeType: mimeType, audioBitsPerSecond: 16000 };
        const stream = await navigator.mediaDevices.getUserMedia(constraints);
        recorder = new MediaRecorder(stream, options)
        let stopped = false;
        recorder.addEventListener('dataavailable', async (e) => {
            const buffer = await e.data.arrayBuffer();
            const base64 = btoa(String.fromCharCode.apply(null, new Uint8Array(buffer)));
            await dotNetObject.invokeMethodAsync("DataAvailable", base64);
            if (stopped) {
                await dotNetObject.invokeMethodAsync("RecordingStopped");
            }
        });

        recorder.addEventListener("stop", () => {
            stopped = true;
        });

        window.stopRecording = stopRecording;
        recorder.start(1000);
        return recorder;
    } else {
        console.error('No supported audio formats found');
    }
}

function stopRecording() {
    recorder.stop();
    recorder.dataavailable = null;
    recorder = null;
}
