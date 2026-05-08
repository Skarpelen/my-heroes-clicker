import type { ScenarioStatus } from '../model/types'

type ScenarioStatusBarProps = {
  status: ScenarioStatus | null
}

export function ScenarioStatusBar({ status }: ScenarioStatusBarProps) {
  const targetIterations = status?.targetIterations ?? 0
  const completedIterations = status?.completedIterations ?? 0
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
    </section>
  )
}
