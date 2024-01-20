//import { MediaRecorder, register } from 'extendable-media-recorder';
//import { connect } from 'extendable-media-recorder-wav-encoder';
//await register(await connect());
let recorder: MediaRecorder | null;
export interface BrowserMediaDevice {
    DeviceId: string;
    Label: string;
    Kind: string;
    GroupId: string;
}

export async function getAudioInputDevices(): Promise<BrowserMediaDevice[]> {
    try {
        const devices: MediaDeviceInfo[] = await navigator.mediaDevices.enumerateDevices();
        return devices.filter((device: MediaDeviceInfo) => device.kind === 'audioinput').map((device: MediaDeviceInfo) => ({
            DeviceId: device.deviceId,
            Label: device.label || 'Unknown Audio Device',
            Kind: device.kind,
            GroupId: device.groupId
        }));
    } catch (error: unknown) {
        console.error('Error fetching audio input devices', error);
        return [];
    }
}

export async function requestMicrophonePermission(): Promise<boolean> {
    try {
        const stream: MediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        stream.getTracks().forEach((track: MediaStreamTrack) => track.stop());
        return true;
    } catch (error: unknown) {
        return false;
    }
}

export function getSupportedMimeType(): string | null {
    // audio/webm=chrome/edge/firefox, audio/mp4=safari
    //const types: string[] = ['audio/wav'];
    const types: string[] = ['audio/webm', 'audio/mp4'];
    for (const type of types) {
        if (MediaRecorder.isTypeSupported(type)) {
            return type;
        }
    }

    return null;
}

export async function startRecording(page: DotNet.DotNetObject, deviceId: string): Promise<string | null> {
    const mimeType: string | null = getSupportedMimeType();
    if (mimeType) {
        const constraints: MediaStreamConstraints = { audio: { deviceId: deviceId, channelCount: 1 } };
        const options: MediaRecorderOptions = { mimeType: mimeType, audioBitsPerSecond: 16000 };
        const stream: MediaStream = await navigator.mediaDevices.getUserMedia(constraints);
        recorder = new MediaRecorder(stream, options) as MediaRecorder
        let stopped: boolean = false;
        recorder.addEventListener('dataavailable', async (e: BlobEvent) => {
            const buffer: ArrayBuffer = await e.data.arrayBuffer();
            const uint8Array: Uint8Array = new Uint8Array(buffer);
            await page.invokeMethodAsync("DataAvailable", uint8Array);
            if (stopped) {
                await page.invokeMethodAsync("RecordingStopped");
            }
        });

        recorder.addEventListener("stop", () => {
            stopped = true;
        });

        recorder.start(1000);
    } else {
        console.error('No supported audio formats found');
    }

    return mimeType;
}

export function stopRecording(): void {
    if (recorder) {
        recorder.stop();
        recorder.stream.getTracks().forEach((track: MediaStreamTrack) => track.stop());
        recorder = null;
    }
}
