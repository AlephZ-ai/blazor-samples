import { MediaRecorder, register } from 'extendable-media-recorder';
import { connect } from 'extendable-media-recorder-wav-encoder';
await register(await connect());
// TODO: Fix any type to MediaRecorder, https://github.com/chrisguttandin/extendable-media-recorder causing issues
let recorder: any;
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

export function getSupportedMimeType(): string {
    // audio/webm=chrome/edge/firefox, audio/mp4=safari
    const types: string[] = ['audio/wav'];
    for (const type of types) {
        if (MediaRecorder.isTypeSupported(type)) {
            return type;
        }
    }

    return null;
}

// TODO: Fix to reference blazor.d.ts and use DotNet.DotNetObject instead of any for page
export async function startRecording(page: any, deviceId: string): Promise<string> {
    const mimeType: string = getSupportedMimeType();
    if (mimeType) {
        const constraints: MediaStreamConstraints = { audio: { deviceId: deviceId, channelCount: 1 } };
        const options: MediaRecorderOptions = { mimeType: mimeType, audioBitsPerSecond: 16000 };
        const stream: MediaStream = await navigator.mediaDevices.getUserMedia(constraints);
        recorder = new MediaRecorder(stream, options)
        let stopped: boolean = false;
        recorder.addEventListener('dataavailable', async (e: BlobEvent) => {
            const buffer: ArrayBuffer = await e.data.arrayBuffer();
            const base64: string = btoa(String.fromCharCode.apply(null, new Uint8Array(buffer)));
            await page.invokeMethodAsync("DataAvailable", base64);
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

// TODO: Check if mic still hangs open in Mac
export function stopRecording(): void {
    recorder.stop();
    recorder.stream.getTracks().forEach((track: MediaStreamTrack) => track.stop());
    recorder = null;
}
