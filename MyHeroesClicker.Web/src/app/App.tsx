import { useCallback, useEffect, useState } from 'react'
import { subscribeToAlerts } from '../modules/alerts/api/alertsApi'
import { AlertSoundPanel } from '../modules/alerts/components/AlertSoundPanel'
import { isAudibleAlertKind, playAlertSound } from '../modules/alerts/model/sound'
import type { AlertEvent, AlertSoundSettings } from '../modules/alerts/model/types'
import { SettingsPanel } from '../modules/configuration/components/SettingsPanel'
import { getAccounts, getAppSettings } from '../modules/configuration/api/configurationApi'
import type { Account, AppSettings } from '../modules/configuration/model/types'
import { FarmScenarioPanel } from '../modules/scenarios/components/FarmScenarioPanel'
import { ScenarioStatusBar } from '../modules/scenarios/components/ScenarioStatusBar'
import { WarScenarioPanel } from '../modules/scenarios/components/WarScenarioPanel'
import { getScenarioStatus, stopScenario, stopScenarioByKey } from '../modules/scenarios/api/scenariosApi'
import type { ScenarioStatus } from '../modules/scenarios/model/types'
import '../styles/app.css'

type AppPage = 'scenarios' | 'settings'

const alertSoundSettingsKey = 'myHeroesClicker.alertSoundSettings'

export function App() {
  const [page, setPage] = useState<AppPage>('scenarios')
  const [status, setStatus] = useState<ScenarioStatus | null>(null)
  const [statusError, setStatusError] = useState<string | null>(null)
  const [accounts, setAccounts] = useState<Account[]>([])
  const [settings, setSettings] = useState<AppSettings | null>(null)
  const [lastAlert, setLastAlert] = useState<AlertEvent | null>(null)
  const [alertSoundSettings, setAlertSoundSettings] = useState<AlertSoundSettings>(() => loadAlertSoundSettings())
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

  const stopAllScenarios = useCallback(async () => {
    try {
      await stopScenario()
      await refreshStatus()
    } catch (exception) {
      setStatusError(exception instanceof Error ? exception.message : 'Не удалось остановить сценарии.')
    }
  }, [refreshStatus])

  const stopSingleScenario = useCallback(async (scenarioKey: string) => {
    try {
      await stopScenarioByKey(scenarioKey)
      await refreshStatus()
    } catch (exception) {
      setStatusError(exception instanceof Error ? exception.message : 'Не удалось остановить сценарий.')
    }
  }, [refreshStatus])

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

  useEffect(() => {
    window.localStorage.setItem(alertSoundSettingsKey, JSON.stringify(alertSoundSettings))
  }, [alertSoundSettings])

  useEffect(() => {
    return subscribeToAlerts((alertEvent) => {
      setLastAlert(alertEvent)

      if (alertSoundSettings.enabled && isAudibleAlertKind(alertEvent.kind)) {
        void playAlertSound(alertEvent.kind, alertSoundSettings.volume).catch(() => undefined)
      }
    })
  }, [alertSoundSettings])

  const testAlertSound = useCallback(() => {
    void playAlertSound('captcha', alertSoundSettings.volume).catch(() => undefined)
  }, [alertSoundSettings.volume])

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

          <div className="scenario-page-layout">
            <div className="scenario-page-main">
              <AlertSoundPanel
                settings={alertSoundSettings}
                lastAlert={lastAlert}
                onSettingsChange={setAlertSoundSettings}
                onTest={testAlertSound}
              />
              <FarmScenarioPanel status={status} onRefreshStatus={refreshStatus} />
              <WarScenarioPanel status={status} onRefreshStatus={refreshStatus} />
            </div>

            <aside className="scenario-page-sidebar" aria-label="Статусы сценариев">
              <ScenarioStatusBar
                status={status}
                onStopAll={() => void stopAllScenarios()}
                onStopScenario={(scenarioKey) => void stopSingleScenario(scenarioKey)}
              />
            </aside>
          </div>
        </>
      )}

      {page === 'settings' && <SettingsPanel onConfigurationChanged={refreshConfiguration} />}
    </main>
  )
}

function loadAlertSoundSettings(): AlertSoundSettings {
  const rawSettings = window.localStorage.getItem(alertSoundSettingsKey)

  if (!rawSettings) {
    return {
      enabled: true,
      volume: 0.55,
    }
  }

  try {
    const settings = JSON.parse(rawSettings) as Partial<AlertSoundSettings>

    return {
      enabled: settings.enabled ?? true,
      volume: typeof settings.volume === 'number' ? settings.volume : 0.55,
    }
  } catch {
    return {
      enabled: true,
      volume: 0.55,
    }
  }
}
