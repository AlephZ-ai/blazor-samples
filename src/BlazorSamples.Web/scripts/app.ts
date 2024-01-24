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

export function startMediaSource(page: DotNet.DotNetObject) {
    var audioElement = document.getElementById('audioElement') as HTMLAudioElement;
    var mediaSource = new MediaSource();
    mediaSource.addEventListener('sourceopen', () => {
        var sourceBuffer = mediaSource.addSourceBuffer('audio/mpeg');
        readBufferChunks(page, sourceBuffer);
        audioElement.play();
    }, { once: true });

    audioElement.src = URL.createObjectURL(mediaSource);
}

async function readBufferChunks(page: DotNet.DotNetObject, sourceBuffer: SourceBuffer) {
    var chunk: Uint8Array | null = await page.invokeMethodAsync("Pop");
    if (!chunk) return;
    sourceBuffer.appendBuffer(chunk);
    sourceBuffer.addEventListener('updateend', () => readBufferChunks(page, sourceBuffer), { once: true });
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
    let mimeType: string | null = getSupportedMimeType();
    if (mimeType) {
        const constraints: MediaStreamConstraints = { audio: { deviceId: deviceId } };
        const options: MediaRecorderOptions = { mimeType: mimeType };
        const stream: MediaStream = await navigator.mediaDevices.getUserMedia(constraints);
        recorder = new MediaRecorder(stream, options);
        mimeType = recorder.mimeType;
        const track: MediaStreamTrack = stream.getAudioTracks()[0];
        const settings: MediaTrackSettings = track.getSettings();
        const { channelCount, sampleRate }: MediaTrackSettings = settings;
        console.log(recorder);
        console.log(stream);
        console.log(track);
        console.log(settings);
        let stopped: boolean = false;
        recorder.ondataavailable = async (e: BlobEvent) => {
            const data: ArrayBuffer = await e.data.arrayBuffer();
            const uint8Array: Uint8Array = new Uint8Array(data);
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
