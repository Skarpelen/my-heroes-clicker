export type ScenarioStatus = {
  isInitialized: boolean
  isRunning: boolean
  isPaused: boolean
  activeScenarioKey: string | null
  activeScenarioName: string | null
  browserTabName: string | null
  pauseReason: string | null
  pauseRequestedAt: string | null
  lastUserEvent: string | null
  lastUserEventAt: string | null
  targetIterations: number | null
  completedIterations: number | null
  maxHealth: number | null
  lastError: string | null
}

export type FarmScenarioOptions = {
  iterations: number
}
