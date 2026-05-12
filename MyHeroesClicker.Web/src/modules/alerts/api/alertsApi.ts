import type { AlertEvent } from '../model/types'

export function subscribeToAlerts(onAlert: (alertEvent: AlertEvent) => void) {
  const eventSource = new EventSource('/api/alert/stream')

  eventSource.addEventListener('alert', (event) => {
    onAlert(JSON.parse(event.data) as AlertEvent)
  })

  return () => {
    eventSource.close()
  }
}
