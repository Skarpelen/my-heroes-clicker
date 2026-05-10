export type ConfigurationKind = 'farm' | 'combat'

export type Account = {
  id: number
  login: string
  hasPassword: boolean
  isEnabled: boolean
}

export type AccountPayload = {
  login: string
  password: string | null
  isEnabled: boolean
}

export type AppSettings = {
  id: number
  activeAccountId: number | null
  baseUrl: string
  browserKind: string
  headless: boolean
  userDataDir: string | null
  minDelayMs: number
  maxDelayMs: number
  defaultTimeoutMs: number
  hpRecoveryDelayMultiplier: number
  minAttackHealthPercent: number
  maxAttackHealthPercent: number
  maxStepRetryCount: number
  retryDelayMs: number
  authenticationRetryDelayMs: number
}

export type UpdateAppSettingsRequest = Omit<AppSettings, 'id' | 'activeAccountId'>

export type EquipmentSet = {
  id: number
  accountId: number | null
  kind: ConfigurationKind
}

export type EquipmentSetSlot = {
  equipmentSetId: number
  slotNumber: number
  itemId: number | null
  expectedImageSrc: string
  shouldBeEmpty: boolean
}

export type EquipmentSetPayload = Omit<EquipmentSet, 'id'>

export type TechniquePreset = {
  id: number
  accountId: number | null
  kind: ConfigurationKind
}

export type TechniquePresetSlot = {
  techniquePresetId: number
  techniqueNumber: number
  techniqueName: string
  isEnabled: boolean
}

export type TechniquePresetPayload = Omit<TechniquePreset, 'id'>
