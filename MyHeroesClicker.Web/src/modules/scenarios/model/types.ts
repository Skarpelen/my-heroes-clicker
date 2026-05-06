export type ScenarioStatus = {
  isInitialized: boolean
  isRunning: boolean
  targetIterations: number | null
  completedIterations: number | null
  maxHealth: number | null
  lastError: string | null
}

export type CharacterHealth = {
  maxHealth: number
}

export type FarmScenarioOptions = {
  iterations: number
  maxHealth: number | null
  disableTechniques: boolean
  prepareFarmSet: boolean
  returnToCombatSet: boolean
}