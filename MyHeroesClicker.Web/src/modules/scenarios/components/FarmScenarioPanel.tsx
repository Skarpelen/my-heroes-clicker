import { useState } from 'react'
import { Map, Play, Shield, Square, Sprout, Swords, WandSparkles } from 'lucide-react'
import { Button } from '../../../shared/ui/Button'
import {
  prepareCombatMode,
  prepareFarmMode,
  resumeScenario,
  startAdventureFarmScenario,
  startBattleFarmScenario,
  stopScenarioByKey,
} from '../api/scenariosApi'
import type { FarmScenarioOptions, ScenarioStatus } from '../model/types'

type FarmScenarioPanelProps = {
  status: ScenarioStatus | null
  onRefreshStatus: () => Promise<void>
}

export function FarmScenarioPanel({ status, onRefreshStatus }: FarmScenarioPanelProps) {
  const [options, setOptions] = useState<FarmScenarioOptions>({
    iterations: 500,
  })

  const [isPreparing, setIsPreparing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const isRunning = status?.isRunning ?? false
  const runningScenarioKeys = status?.runningScenarioKeys ?? []
  const activeFarmScenarioKey = runningScenarioKeys.find((key) => key === 'farmCycle' || key === 'adventureFarmCycle' || key === 'farmBattle') ?? null
  const isFarmRunning = activeFarmScenarioKey !== null
  const isPaused = status?.isPaused ?? false
  const canStart = !isFarmRunning

  async function prepareMode(mode: 'farm' | 'combat') {
    setError(null)
    setIsPreparing(true)

    try {
      if (mode === 'farm') {
        await prepareFarmMode()
      } else {
        await prepareCombatMode()
      }

      await onRefreshStatus()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось сменить сет.')
    } finally {
      setIsPreparing(false)
    }
  }

  async function startFarm(location: 'battle' | 'adventure') {
    setError(null)

    try {
      if (location === 'battle') {
        await startBattleFarmScenario(options)
      } else {
        await startAdventureFarmScenario(options)
      }

      await onRefreshStatus()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось запустить фарм.')
    }
  }

  async function stopFarm() {
    setError(null)

    try {
      if (activeFarmScenarioKey === null) {
        return
      }

      await stopScenarioByKey(activeFarmScenarioKey)
      await onRefreshStatus()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось остановить фарм.')
    }
  }

  async function resumeFarm() {
    setError(null)

    try {
      await resumeScenario()
      await onRefreshStatus()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось снять паузу.')
    }
  }

  return (
    <section className="scenario-card scenario-card-active">
      <div className="scenario-header">
        <div>
          <span className="eyebrow">Фарм</span>
          <h2>Основной фарм</h2>
        </div>

        <div className="scenario-mark" aria-hidden="true">
          <WandSparkles size={42} />
        </div>
      </div>

      <div className="form-grid form-grid-single">
        <label>
          Атак выполнить
          <input
            type="number"
            min={1}
            value={options.iterations}
            onChange={(event) =>
              setOptions((current) => ({
                ...current,
                iterations: Number(event.target.value),
              }))
            }
          />
        </label>
      </div>

      <div className="actions actions-secondary">
        <Button variant="ghost" disabled={isPreparing || isRunning} onClick={() => void prepareMode('farm')}>
          <Sprout size={18} />
          Фарм сет
        </Button>

        <Button variant="ghost" disabled={isPreparing || isRunning} onClick={() => void prepareMode('combat')}>
          <Shield size={18} />
          Боевой сет
        </Button>
      </div>

      {error && <p className="panel-error">{error}</p>}

      <div className="farm-starts">
        <Button disabled={!canStart} onClick={() => void startFarm('battle')}>
          <Swords size={18} />
          Запустить драку
        </Button>

        <Button disabled={!canStart} onClick={() => void startFarm('adventure')}>
          <Map size={18} />
          Запустить приключения
        </Button>
      </div>

      <div className="actions">
        <Button variant="ghost" disabled={!isPaused} onClick={resumeFarm}>
          <Play size={18} />
          Продолжить
        </Button>

        <Button variant="danger" disabled={!isFarmRunning} onClick={stopFarm}>
          <Square size={18} />
          Остановить
        </Button>
      </div>
    </section>
  )
}
