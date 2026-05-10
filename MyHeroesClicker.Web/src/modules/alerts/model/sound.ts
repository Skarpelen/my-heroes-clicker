import type { AlertEventKind } from './types'

type AudibleAlertEventKind = Extract<AlertEventKind, 'captcha' | 'fatalError'>

type Tone = {
  frequency: number
  durationMs: number
  delayMs?: number
}

const alertTones: Record<AudibleAlertEventKind, Tone[]> = {
  captcha: [
    { frequency: 1040, durationMs: 180 },
    { frequency: 1040, durationMs: 180, delayMs: 260 },
    { frequency: 1040, durationMs: 260, delayMs: 520 },
  ],
  fatalError: [
    { frequency: 320, durationMs: 220 },
    { frequency: 240, durationMs: 320, delayMs: 260 },
  ],
}

export function isAudibleAlertKind(kind: AlertEventKind): kind is AudibleAlertEventKind {
  return kind === 'captcha' || kind === 'fatalError'
}

export async function playAlertSound(kind: AudibleAlertEventKind, volume: number) {
  const audioContext = new AudioContext()
  const now = audioContext.currentTime
  const normalizedVolume = Math.max(0, Math.min(1, volume))

  for (const tone of alertTones[kind]) {
    const start = now + ((tone.delayMs ?? 0) / 1000)
    const end = start + (tone.durationMs / 1000)
    const oscillator = audioContext.createOscillator()
    const gain = audioContext.createGain()

    oscillator.type = 'sine'
    oscillator.frequency.value = tone.frequency
    gain.gain.setValueAtTime(0, start)
    gain.gain.linearRampToValueAtTime(0.18 * normalizedVolume, start + 0.015)
    gain.gain.exponentialRampToValueAtTime(0.001, end)

    oscillator.connect(gain)
    gain.connect(audioContext.destination)
    oscillator.start(start)
    oscillator.stop(end)
  }

  const totalDurationMs = Math.max(...alertTones[kind].map((tone) => (tone.delayMs ?? 0) + tone.durationMs))

  window.setTimeout(() => {
    void audioContext.close()
  }, totalDurationMs + 120)
}
