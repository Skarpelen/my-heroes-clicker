import { useCallback, useEffect, useState } from 'react'
import { SettingsPanel } from '../modules/configuration/components/SettingsPanel'
import { getAccounts, getAppSettings } from '../modules/configuration/api/configurationApi'
import type { Account, AppSettings } from '../modules/configuration/model/types'
import { FarmScenarioPanel } from '../modules/scenarios/components/FarmScenarioPanel'
import { ScenarioStatusBar } from '../modules/scenarios/components/ScenarioStatusBar'
import { getScenarioStatus } from '../modules/scenarios/api/scenariosApi'
import type { ScenarioStatus } from '../modules/scenarios/model/types'
import '../styles/app.css'

type AppPage = 'scenarios' | 'settings'

export function App() {
  const [page, setPage] = useState<AppPage>('scenarios')
  const [status, setStatus] = useState<ScenarioStatus | null>(null)
  const [statusError, setStatusError] = useState<string | null>(null)
  const [accounts, setAccounts] = useState<Account[]>([])
  const [settings, setSettings] = useState<AppSettings | null>(null)
  const activeAccount = accounts.find((account) => account.id === settings?.activeAccountId) ?? null

  const refreshStatus = useCallback(async () => {
    try {
      const nextStatus = await getScenarioStatus()
      setStatus(nextStatus)
      setStatusError(null)
    } catch (exception) {
      setStatusError(exception instanceof Error ? exception.message : 'Не удалось получить статус.')
    }
  }, [])

  const refreshConfiguration = useCallback(async () => {
    try {
      const [nextAccounts, nextSettings] = await Promise.all([
        getAccounts(),
        getAppSettings(),
      ])

      setAccounts(nextAccounts)
      setSettings(nextSettings)
    } catch {
      setAccounts([])
      setSettings(null)
    }
  }, [])

  useEffect(() => {
    const firstRefreshId = window.setTimeout(() => {
      void refreshStatus()
      void refreshConfiguration()
    }, 0)

    const intervalId = window.setInterval(() => {
      void refreshStatus()
    }, 2000)

    return () => {
      window.clearTimeout(firstRefreshId)
      window.clearInterval(intervalId)
    }
  }, [refreshConfiguration, refreshStatus])

  return (
    <main className="shell">
      <section className="hero-section">
        <div className="hero-copy">
          <span className="eyebrow">My Heroes Clicker</span>
          <h1>{page === 'scenarios' ? 'Сценарии' : 'Настройки'}</h1>
        </div>

        <aside className="account-card" aria-label="Текущий аккаунт">
          <span>Аккаунт</span>
          <strong>{activeAccount?.login ?? 'Не выбран'}</strong>
        </aside>
      </section>

      <nav className="page-tabs" aria-label="Разделы приложения">
        <button className={page === 'scenarios' ? 'page-tab-active' : ''} onClick={() => setPage('scenarios')}>
          Сценарии
        </button>
        <button className={page === 'settings' ? 'page-tab-active' : ''} onClick={() => setPage('settings')}>
          Настройки
        </button>
      </nav>

      {page === 'scenarios' && (
        <>
          {statusError && <p className="global-error">{statusError}</p>}

          <ScenarioStatusBar status={status} />
          <FarmScenarioPanel status={status} onRefreshStatus={refreshStatus} />
        </>
      )}

      {page === 'settings' && <SettingsPanel onConfigurationChanged={refreshConfiguration} />}
    </main>
  )
}
