import { Square } from 'lucide-react'
import { Button } from '../../../shared/ui/Button'
import type { ScenarioStatus } from '../model/types'

type ScenarioStatusBarProps = {
  status: ScenarioStatus | null
  onStopAll: () => void
  onStopScenario: (scenarioKey: string) => void
}

export function ScenarioStatusBar({ status, onStopAll, onStopScenario }: ScenarioStatusBarProps) {
  const iterationLimit = status?.iterationLimit ?? 0
  const completedIterations = status?.completedIterations ?? 0
  const pauseRequestedAt = formatDateTime(status?.pauseRequestedAt ?? null)
  const lastUserEventAt = formatDateTime(status?.lastUserEventAt ?? null)
  const scenarioRuns = status?.scenarioRuns ?? []
  const progress = iterationLimit > 0
    ? Math.min(100, Math.round((completedIterations / iterationLimit) * 100))
    : 0

  return (
    <section className="status-bar">
      <div>
        <span className="eyebrow">Статус</span>
        <h2>{status?.isPaused ? 'Пауза' : status?.isRunning ? 'Сценарии запущены' : 'Остановлено'}</h2>
      </div>

      <div className="status-actions">
        <Button variant="danger" disabled={!status?.isRunning} onClick={onStopAll}>
          <Square size={18} />
          Остановить все
        </Button>
      </div>

      <div className="status-progress" aria-label="Прогресс сценария">
        <div className="progress-track">
          <div className="progress-value" style={{ width: `${progress}%` }} />
        </div>

        <span>
          {completedIterations} / {iterationLimit}
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

      <div className="active-runs">
        <h3>Запуски сценариев</h3>

        {scenarioRuns.length === 0 && <p className="muted-line">Нет запусков сценариев.</p>}

        {scenarioRuns.map((run) => {
          const runProgress = run.progressPercent ?? 0
          const startedAt = formatDateTime(run.startedAt)
          const nextCheckAt = formatDateTime(run.nextCheckAt)
          const canStop = run.state !== 'stopped' && run.state !== 'failed' && run.state !== 'stopping'

          return (
            <article className="active-run-card" key={run.runId}>
              <div className="active-run-header">
                <div>
                  <strong>{run.scenarioName}</strong>
                  <span>{run.browserTabName}</span>
                </div>

                <div className="active-run-actions">
                  <span className={`run-state run-state-${run.state}`}>{formatRunState(run.state)}</span>

                  <Button variant="danger" disabled={!canStop} onClick={() => onStopScenario(run.scenarioKey)}>
                    <Square size={16} />
                    Остановить
                  </Button>
                </div>
              </div>

              {run.iterationLimit !== null && (
                <div className="status-progress" aria-label="Прогресс запуска">
                  <div className="progress-track">
                    <div className="progress-value" style={{ width: `${runProgress}%` }} />
                  </div>

                  <span>
                    {run.completedIterations} / {run.iterationLimit}
                  </span>
                </div>
              )}

              <dl>
                <div>
                  <dt>Run id</dt>
                  <dd>{run.runId}</dd>
                </div>

                <div>
                  <dt>Старт</dt>
                  <dd>{startedAt ?? 'Нет'}</dd>
                </div>

                <div>
                  <dt>Статус</dt>
                  <dd>{run.statusMessage ?? 'Работает'}</dd>
                </div>

                <div>
                  <dt>Следующая проверка</dt>
                  <dd>{nextCheckAt ?? 'Нет'}</dd>
                </div>
              </dl>

              {run.lastError && <p className="status-error">{run.lastError}</p>}
            </article>
          )
        })}
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

function formatRunState(state: string) {
  switch (state) {
    case 'running':
      return 'Работает'
    case 'paused':
      return 'Пауза'
    case 'stopping':
      return 'Останавливается'
    case 'stopped':
      return 'Остановлен'
    case 'failed':
      return 'Ошибка'
    default:
      return state
  }
}
