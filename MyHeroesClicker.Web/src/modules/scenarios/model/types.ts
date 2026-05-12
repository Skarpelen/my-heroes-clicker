export type ScenarioStatus = {
  isInitialized: boolean
  isRunning: boolean
  isPaused: boolean
  activeScenarioKey: string | null
  activeScenarioName: string | null
  browserTabName: string | null
  runningScenarioKeys: string[]
  scenarioRuns: ScenarioRunStatus[]
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

export type ScenarioRunStatus = {
  runId: string
  scenarioKey: string
  scenarioName: string
  browserTabKind: string
  browserTabName: string
  state: 'running' | 'paused' | 'stopping' | 'stopped' | 'failed' | string
  startedAt: string
  stopRequestedAt: string | null
  iterationLimit: number | null
  completedIterations: number
  progressPercent: number | null
  statusMessage: string | null
  nextCheckAt: string | null
  lastError: string | null
}
