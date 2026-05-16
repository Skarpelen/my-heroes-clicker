import { useState } from 'react'
import { ShieldAlert, Square } from 'lucide-react'
import { Button } from '../../../shared/ui/Button'
import { startWarRegistrationScenario, stopScenarioByKey } from '../api/scenariosApi'
import type { ScenarioStatus } from '../model/types'

type WarScenarioPanelProps = {
  status: ScenarioStatus | null
  onRefreshStatus: () => Promise<void>
}

export function WarScenarioPanel({ status, onRefreshStatus }: WarScenarioPanelProps) {
  const [error, setError] = useState<string | null>(null)
  const isWarRunning = status?.runningScenarioKeys?.includes('warRegistration') ?? false
  const nextCheckAt = formatDateTime(status?.warNextCheckAt ?? null)

  async function startWarRegistration() {
    setError(null)

    try {
      await startWarRegistrationScenario()
      await onRefreshStatus()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось запустить авто войну.')
    }
  }

  async function stopWarRegistration() {
    setError(null)

    try {
      await stopScenarioByKey('warRegistration')
      await onRefreshStatus()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось остановить авто войну.')
    }
  }

  return (
    <section className="scenario-card scenario-card-compact">
      <div className="scenario-header scenario-header-compact">
        <div>
          <span className="eyebrow">Войны</span>
          <h2>Авто война</h2>
        </div>

        <div className="scenario-mark scenario-mark-compact" aria-hidden="true">
          <img className="scenario-mark-image" src="/images/war.png" alt="" />
        </div>
      </div>

      {error && <p className="panel-error">{error}</p>}

      <dl className="war-status">
        <div>
          <dt>Состояние</dt>
          <dd>{isWarRunning ? status?.warState ?? 'Работает' : 'Остановлена'}</dd>
        </div>

        <div>
          <dt>Следующая проверка</dt>
          <dd>{nextCheckAt ?? 'Нет'}</dd>
        </div>
      </dl>

      <div className="actions">
        <Button disabled={isWarRunning} onClick={() => void startWarRegistration()}>
          <ShieldAlert size={18} />
          Запустить авто войну
        </Button>

        <Button variant="danger" disabled={!isWarRunning} onClick={() => void stopWarRegistration()}>
          <Square size={18} />
          Остановить
        </Button>
      </div>
    </section>
  )
}

function formatDateTime(value: string | null) {
  if (!value) {
    return null
  }

  return new Intl.DateTimeFormat('ru-RU', {
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    month: '2-digit',
    second: '2-digit',
  }).format(new Date(value))
}
