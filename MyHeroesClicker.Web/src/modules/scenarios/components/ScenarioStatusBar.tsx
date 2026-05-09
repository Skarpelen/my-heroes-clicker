import type { ScenarioStatus } from '../model/types'

type ScenarioStatusBarProps = {
  status: ScenarioStatus | null
}

export function ScenarioStatusBar({ status }: ScenarioStatusBarProps) {
  const targetIterations = status?.targetIterations ?? 0
  const completedIterations = status?.completedIterations ?? 0
  const pauseRequestedAt = formatDateTime(status?.pauseRequestedAt ?? null)
  const lastUserEventAt = formatDateTime(status?.lastUserEventAt ?? null)
  const progress = targetIterations > 0
    ? Math.min(100, Math.round((completedIterations / targetIterations) * 100))
    : 0

  return (
    <section className="status-bar">
      <div>
        <span className="eyebrow">Статус</span>
        <h2>{status?.isPaused ? 'Пауза' : status?.isRunning ? 'Фарм запущен' : 'Остановлено'}</h2>
      </div>

      <div className="status-progress" aria-label="Прогресс сценария">
        <div className="progress-track">
          <div className="progress-value" style={{ width: `${progress}%` }} />
        </div>

        <span>
          {completedIterations} / {targetIterations}
        </span>
      </div>

      {status?.isPaused && <p className="pause-warning">{status.pauseReason ?? 'Сценарий поставлен на паузу.'}</p>}
      {status?.lastError && <p className="status-error">{status.lastError}</p>}

      <dl className="status-details">
        <div>
          <dt>Сценарий</dt>
          <dd>{status?.activeScenarioName ?? 'Нет активного'}</dd>
        </div>

        <div>
          <dt>Вкладка</dt>
          <dd>{status?.browserTabName ?? 'Не открыта'}</dd>
        </div>

        <div>
          <dt>Пауза с</dt>
          <dd>{pauseRequestedAt ?? 'Нет'}</dd>
        </div>

        <div>
          <dt>Последнее действие</dt>
          <dd>
            {status?.lastUserEvent
              ? lastUserEventAt
                ? `${status.lastUserEvent}, ${lastUserEventAt}`
                : status.lastUserEvent
              : 'Нет'}
          </dd>
        </div>
      </dl>
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
