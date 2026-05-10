import clsx from 'clsx'
import { Volume2 } from 'lucide-react'
import { Button } from '../../../shared/ui/Button'
import { ToggleRow } from '../../../shared/ui/ToggleRow'
import type { AlertEvent, AlertSoundSettings } from '../model/types'

type AlertSoundPanelProps = {
  settings: AlertSoundSettings
  lastAlert: AlertEvent | null
  onSettingsChange: (settings: AlertSoundSettings) => void
  onTest: () => void
}

export function AlertSoundPanel({
  settings,
  lastAlert,
  onSettingsChange,
  onTest,
}: AlertSoundPanelProps) {
  return (
    <section className="alert-sound-panel">
      <div className="alert-sound-header">
        <div>
          <span className="eyebrow">Тревоги</span>
          <h2>Звук тревог</h2>
        </div>

        <Button variant="ghost" onClick={onTest}>
          <Volume2 size={18} />
          Тест
        </Button>
      </div>

      <div className="alert-sound-controls">
        <ToggleRow
          title="Звук включен"
          description="Явный сигнал для captcha и отдельный сигнал для фатальной ошибки."
          checked={settings.enabled}
          onChange={(enabled) => onSettingsChange({ ...settings, enabled })}
        />

        <label className="volume-row">
          <span>
            <strong>Громкость</strong>
            <small>{Math.round(settings.volume * 100)}%</small>
          </span>

          <input
            type="range"
            min="0"
            max="1"
            step="0.05"
            value={settings.volume}
            onChange={(event) => onSettingsChange({ ...settings, volume: Number(event.target.value) })}
          />
        </label>
      </div>

      <dl className={clsx('alert-last-event', lastAlert && `alert-last-event-${lastAlert.kind}`)}>
        <div>
          <dt>Последняя тревога</dt>
          <dd>{lastAlert ? formatAlert(lastAlert) : 'Нет'}</dd>
        </div>
      </dl>
    </section>
  )
}

function formatAlert(alertEvent: AlertEvent) {
  const createdAt = new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    month: '2-digit',
    second: '2-digit',
  }).format(new Date(alertEvent.createdAt))

  return `${formatKind(alertEvent.kind)}: ${alertEvent.message}, ${createdAt}`
}

function formatKind(kind: AlertEvent['kind']) {
  if (kind === 'captcha') {
    return 'Captcha'
  }

  if (kind === 'authenticationRequired') {
    return 'Авторизация'
  }

  if (kind === 'fatalError') {
    return 'Ошибка'
  }

  return 'Инфо'
}
