import type {
  Account,
  AccountPayload,
  AppSettings,
  EquipmentSet,
  EquipmentSetPayload,
  EquipmentSetSlot,
  TechniquePreset,
  TechniquePresetPayload,
  TechniquePresetSlot,
  UpdateAppSettingsRequest,
} from '../model/types'
import type { AlertSoundSettings } from '../../alerts/model/types'

async function readError(response: Response, fallback: string) {
  const error = await response.json().catch(() => null)

  return error?.error ?? fallback
}

async function readJson<TResponse>(url: string, fallbackError: string): Promise<TResponse> {
  const response = await fetch(url)

  if (!response.ok) {
    throw new Error(await readError(response, fallbackError))
  }

  return await response.json()
}

async function sendJson<TPayload>(
  url: string,
  method: 'POST' | 'PUT',
  payload: TPayload,
  fallbackError: string): Promise<void> {
  const response = await fetch(url, {
    method,
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error(await readError(response, fallbackError))
  }
}

async function createJson<TPayload>(
  url: string,
  payload: TPayload,
  fallbackError: string): Promise<number> {
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    throw new Error(await readError(response, fallbackError))
  }

  const result = await response.json().catch(() => null)

  return Number(result?.id)
}

export async function getAccounts(): Promise<Account[]> {
  return await readJson<Account[]>('/api/account', 'Не удалось загрузить аккаунты.')
}

export async function createAccount(account: AccountPayload): Promise<number> {
  return await createJson('/api/account', account, 'Не удалось создать аккаунт.')
}

export async function updateAccount(id: number, account: AccountPayload): Promise<void> {
  await sendJson(`/api/account/${id}`, 'PUT', account, 'Не удалось сохранить аккаунт.')
}

export async function deleteAccount(id: number): Promise<void> {
  const response = await fetch(`/api/account/${id}`, {
    method: 'DELETE',
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось удалить аккаунт.'))
  }
}

export async function getAppSettings(): Promise<AppSettings> {
  return await readJson<AppSettings>('/api/settings', 'Не удалось загрузить настройки.')
}

export async function updateAppSettings(settings: UpdateAppSettingsRequest): Promise<void> {
  await sendJson('/api/settings', 'PUT', settings, 'Не удалось сохранить настройки.')
}

export async function setActiveAccount(accountId: number | null): Promise<void> {
  await sendJson('/api/settings/active-account', 'PUT', { accountId }, 'Не удалось выбрать активный аккаунт.')
}

export async function updateAlertSoundSettings(settings: AlertSoundSettings): Promise<void> {
  await sendJson('/api/settings/alert-sound', 'PUT', settings, 'Не удалось сохранить звук тревог.')
}

export async function getEquipmentSets(): Promise<EquipmentSet[]> {
  return await readJson<EquipmentSet[]>('/api/equipment-set', 'Не удалось загрузить сеты.')
}

export async function createEquipmentSet(set: EquipmentSetPayload): Promise<number> {
  return await createJson('/api/equipment-set', set, 'Не удалось создать сет.')
}

export async function updateEquipmentSet(id: number, set: EquipmentSetPayload): Promise<void> {
  await sendJson(`/api/equipment-set/${id}`, 'PUT', set, 'Не удалось сохранить сет.')
}

export async function getEquipmentSetSlots(id: number): Promise<EquipmentSetSlot[]> {
  return await readJson<EquipmentSetSlot[]>(`/api/equipment-set/${id}/slots`, 'Не удалось загрузить слоты сета.')
}

export async function updateEquipmentSetSlot(
  setId: number,
  slotNumber: number,
  slot: Omit<EquipmentSetSlot, 'equipmentSetId' | 'slotNumber'>): Promise<void> {
  await sendJson(
    `/api/equipment-set/${setId}/slots/${slotNumber}`,
    'PUT',
    slot,
    'Не удалось сохранить слот сета.')
}

export async function readCurrentEquipmentSetSlots(setId: number): Promise<EquipmentSetSlot[]> {
  const response = await fetch(`/api/equipment-set/${setId}/slots/read-current`, {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(await readError(response, 'Не удалось считать текущий сет.'))
  }

  return await response.json()
}

export async function getTechniquePresets(): Promise<TechniquePreset[]> {
  return await readJson<TechniquePreset[]>('/api/technique-preset', 'Не удалось загрузить пресеты приемов.')
}

export async function createTechniquePreset(preset: TechniquePresetPayload): Promise<number> {
  return await createJson('/api/technique-preset', preset, 'Не удалось создать пресет приемов.')
}

export async function updateTechniquePreset(id: number, preset: TechniquePresetPayload): Promise<void> {
  await sendJson(`/api/technique-preset/${id}`, 'PUT', preset, 'Не удалось сохранить пресет приемов.')
}

export async function getTechniquePresetSlots(id: number): Promise<TechniquePresetSlot[]> {
  return await readJson<TechniquePresetSlot[]>(`/api/technique-preset/${id}/slots`, 'Не удалось загрузить приемы.')
}

export async function updateTechniquePresetSlot(
  presetId: number,
  techniqueNumber: number,
  slot: Omit<TechniquePresetSlot, 'techniquePresetId' | 'techniqueNumber'>): Promise<void> {
  await sendJson(
    `/api/technique-preset/${presetId}/slots/${techniqueNumber}`,
    'PUT',
    slot,
    'Не удалось сохранить прием.')
}
