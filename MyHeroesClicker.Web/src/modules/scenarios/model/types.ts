export type ScenarioStatus = {
  isInitialized: boolean
  isRunning: boolean
  isPaused: boolean
  pauseReason: string | null
  targetIterations: number | null
  completedIterations: number | null
  maxHealth: number | null
  lastError: string | null
}

export type FarmScenarioOptions = {
  iterations: number
}
