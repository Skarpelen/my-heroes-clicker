import type { FarmScenarioOptions, ScenarioStatus } from '../model/types'

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

export async function startBattleFarmScenario(options: FarmScenarioOptions): Promise<void> {
  await startFarmScenario('/api/scenarios/farm/start', options, 'Не удалось запустить фарм в драке.')
}

export async function startAdventureFarmScenario(options: FarmScenarioOptions): Promise<void> {
  await startFarmScenario('/api/scenarios/adventure/start', options, 'Не удалось запустить фарм в приключениях.')
}

export async function startWarRegistrationScenario(): Promise<void> {
  const response = await fetch('/api/scenarios/war/start', {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось запустить авто войну.'))
  }
}

export async function prepareFarmMode(): Promise<void> {
  const response = await fetch('/api/scenarios/farm/prepare', {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось надеть фарм-сет.'))
  }
}

export async function prepareCombatMode(): Promise<void> {
  const response = await fetch('/api/scenarios/combat/prepare', {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось надеть боевой сет.'))
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

export async function resumeScenario(): Promise<void> {
  const response = await fetch('/api/scenarios/resume', {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось снять паузу.'))
  }
}

async function startFarmScenario(
  url: string,
  options: FarmScenarioOptions,
  fallbackError: string): Promise<void> {
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      iterations: options.iterations,
    }),
  })

  if (!response.ok) {
    throw new Error(await readError(response, fallbackError))
  }
}
