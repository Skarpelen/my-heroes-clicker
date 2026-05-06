import type { CharacterHealth, FarmScenarioOptions, ScenarioStatus } from '../model/types'

async function readError(response: Response, fallback: string) {
  const error = await response.json().catch(() => null)

  return error?.error ?? fallback
}

export async function getScenarioStatus(): Promise<ScenarioStatus> {
  const response = await fetch('/api/scenarios/status')

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось получить статус сценария.'))
  }

  return await response.json()
}

export async function getCharacterHealth(): Promise<CharacterHealth> {
  const response = await fetch('/api/scenarios/character/health')

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось получить здоровье персонажа.'))
  }

  return await response.json()
}

export async function startFarmScenario(options: FarmScenarioOptions): Promise<void> {
  if (options.maxHealth === null) {
    throw new Error('Получите максимальное здоровье перед запуском.')
  }

  const response = await fetch('/api/scenarios/farm/start', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      iterations: options.iterations,
      maxHealth: options.maxHealth,
    }),
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось запустить фарм.'))
  }
}

export async function stopScenario(): Promise<void> {
  const response = await fetch('/api/scenarios/stop', {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось остановить сценарий.'))
  }
}