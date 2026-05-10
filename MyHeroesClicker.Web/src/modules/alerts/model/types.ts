export type AlertEventKind = 'captcha' | 'authenticationRequired' | 'fatalError'

export type AlertEvent = {
  id: number
  kind: AlertEventKind
  message: string
  createdAt: string
}

export type AlertSoundSettings = {
  enabled: boolean
  volume: number
}
