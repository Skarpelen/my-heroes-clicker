export type ScenarioStatus = {
  isInitialized: boolean
  isRunning: boolean
  isPaused: boolean
  activeScenarioKey: string | null
  activeScenarioName: string | null
  browserTabName: string | null
  runningScenarioKeys: string[]
  pauseReason: string | null
  pauseRequestedAt: string | null
  lastUserEvent: string | null
  lastUserEventAt: string | null
  iterationLimit: number | null
  completedIterations: number | null
  maxHealth: number | null
  warState: string | null
  warNextCheckAt: string | null
  lastError: string | null
}

export type FarmScenarioOptions = {
  iterations: number
}
