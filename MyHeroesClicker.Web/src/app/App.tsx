import { useCallback, useEffect, useState } from 'react'
import { FarmScenarioPanel } from '../modules/scenarios/components/FarmScenarioPanel'
import { ScenarioStatusBar } from '../modules/scenarios/components/ScenarioStatusBar'
import { getScenarioStatus } from '../modules/scenarios/api/scenariosApi'
import type { ScenarioStatus } from '../modules/scenarios/model/types'
import '../styles/app.css'

export function App() {
  const [status, setStatus] = useState<ScenarioStatus | null>(null)
  const [statusError, setStatusError] = useState<string | null>(null)

  const refreshStatus = useCallback(async () => {
    try {
      const nextStatus = await getScenarioStatus()
      setStatus(nextStatus)
      setStatusError(null)
    } catch (exception) {
      setStatusError(exception instanceof Error ? exception.message : 'Не удалось получить статус.')
    }
  }, [])

  useEffect(() => {
    const firstRefreshId = window.setTimeout(() => {
      void refreshStatus()
    }, 0)

    const intervalId = window.setInterval(() => {
      void refreshStatus()
    }, 2000)

    return () => {
      window.clearTimeout(firstRefreshId)
      window.clearInterval(intervalId)
    }
  }, [refreshStatus])

  return (
    <main className="shell">
      <section className="hero-section">
        <div className="hero-copy">
          <span className="eyebrow">My Heroes Clicker</span>
          <h1>Сценарии</h1>
        </div>

        <aside className="account-card" aria-label="Текущий аккаунт">
          <span>Аккаунт</span>
          <strong>Не выбран</strong>
        </aside>
      </section>

      {statusError && <p className="global-error">{statusError}</p>}

      <ScenarioStatusBar status={status} />
      <FarmScenarioPanel status={status} onRefreshStatus={refreshStatus} />
    </main>
  )
}
