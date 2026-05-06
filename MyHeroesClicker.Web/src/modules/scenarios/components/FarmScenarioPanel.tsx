import { useState } from 'react'
import { HeartPulse, Play, Square, WandSparkles } from 'lucide-react'
import { Button } from '../../../shared/ui/Button'
import { ToggleRow } from '../../../shared/ui/ToggleRow'
import { getCharacterHealth, startFarmScenario, stopScenario } from '../api/scenariosApi'
import type { FarmScenarioOptions, ScenarioStatus } from '../model/types'

type FarmScenarioPanelProps = {
  status: ScenarioStatus | null
  onRefreshStatus: () => Promise<void>
}

export function FarmScenarioPanel({ status, onRefreshStatus }: FarmScenarioPanelProps) {
  const [options, setOptions] = useState<FarmScenarioOptions>({
    iterations: 500,
    maxHealth: status?.maxHealth ?? null,
    disableTechniques: false,
    prepareFarmSet: true,
    returnToCombatSet: false,
  })

  const [isLoadingHealth, setIsLoadingHealth] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const isRunning = status?.isRunning ?? false
  const canStart = !isRunning && options.maxHealth !== null

  async function loadHealth() {
    setError(null)
    setIsLoadingHealth(true)

    try {
      const health = await getCharacterHealth()
      setOptions((current) => ({
        ...current,
        maxHealth: health.maxHealth,
      }))
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось получить здоровье.')
    } finally {
      setIsLoadingHealth(false)
    }
  }

  async function startFarm() {
    setError(null)

    try {
      await startFarmScenario(options)
      await onRefreshStatus()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось запустить фарм.')
    }
  }

  async function stopFarm() {
    setError(null)

    try {
      await stopScenario()
      await onRefreshStatus()
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось остановить фарм.')
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

      <div className="form-grid">
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

        <div className="health-field">
          <span>Максимальное здоровье</span>
          <div className="health-value">
            <strong>{options.maxHealth ?? 'Не получено'}</strong>
            <Button variant="ghost" disabled={isLoadingHealth || isRunning} onClick={loadHealth}>
              <HeartPulse size={18} />
              {isLoadingHealth ? 'Получение...' : 'Получить'}
            </Button>
          </div>
        </div>
      </div>

      <div className="option-grid option-grid-compact">
        <ToggleRow
          title="Отключить приемы"
          description="Не использовать приемы во время фарма."
          checked={options.disableTechniques}
          onChange={(checked) => setOptions((current) => ({ ...current, disableTechniques: checked }))}
        />

        <ToggleRow
          title="Надеть фарм-сет"
          description="Перед запуском переодеться в комплект для фарма."
          checked={options.prepareFarmSet}
          onChange={(checked) => setOptions((current) => ({ ...current, prepareFarmSet: checked }))}
        />

        <ToggleRow
          title="Вернуть боевой сет"
          description="Переодеться обратно после завершения."
          checked={options.returnToCombatSet}
          onChange={(checked) => setOptions((current) => ({ ...current, returnToCombatSet: checked }))}
        />
      </div>

      {error && <p className="panel-error">{error}</p>}

      <div className="actions">
        <Button disabled={!canStart} onClick={startFarm}>
          <Play size={18} />
          Запустить фарм
        </Button>

        <Button variant="danger" disabled={!isRunning} onClick={stopFarm}>
          <Square size={18} />
          Остановить
        </Button>
      </div>
    </section>
  )
}