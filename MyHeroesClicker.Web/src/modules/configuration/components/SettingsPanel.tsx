import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { AlertTriangle, Save, Settings, Shield, Swords, UserPlus, Users } from 'lucide-react'
import { Button } from '../../../shared/ui/Button'
import { ToggleRow } from '../../../shared/ui/ToggleRow'
import {
  createAccount,
  createEquipmentSet,
  createTechniquePreset,
  getAccounts,
  getAppSettings,
  getEquipmentSets,
  getTechniquePresets,
  getTechniquePresetSlots,
  readCurrentEquipmentSetSlots,
  setActiveAccount,
  updateAccount,
  updateAppSettings,
  updateEquipmentSet,
  updateTechniquePreset,
  updateTechniquePresetSlot,
} from '../api/configurationApi'
import type {
  Account,
  AccountPayload,
  AppSettings,
  ConfigurationKind,
  EquipmentSet,
  EquipmentSetPayload,
  TechniquePreset,
  TechniquePresetPayload,
  TechniquePresetSlot,
  UpdateAppSettingsRequest,
} from '../model/types'

type SettingsTab = 'accounts' | 'equipment' | 'techniques' | 'advanced'

type SettingsPanelProps = {
  onConfigurationChanged: () => Promise<void>
}

const emptyAccount: AccountPayload = {
  login: '',
  encryptedPassword: '',
  isEnabled: true,
}

const defaultSettings: UpdateAppSettingsRequest = {
  baseUrl: 'https://myheroes.ru/',
  browserKind: 'chrome',
  headless: false,
  userDataDir: '',
  minDelayMs: 100,
  maxDelayMs: 250,
  defaultTimeoutMs: 10000,
  hpRecoveryDelayMultiplier: 20,
  minAttackHealthPercent: 0.25,
  maxAttackHealthPercent: 0.3,
  maxStepRetryCount: 10,
  retryDelayMs: 1000,
  authenticationRetryDelayMs: 60000,
}

const techniqueNames = [
  'Подножка',
  'Отдышка',
  'Мелочь',
  'Невезун',
  'Зализ',
  'Поддержка',
  'Чудо',
  'Рука помощи',
  'Мотиватор',
  'Зеленка',
  'Поддых',
  'Зуб за зуб',
  'Обратка',
  'Энергия У',
  'Удар подковой',
]

export function SettingsPanel({ onConfigurationChanged }: SettingsPanelProps) {
  const [tab, setTab] = useState<SettingsTab>('accounts')
  const [accounts, setAccounts] = useState<Account[]>([])
  const [settings, setSettings] = useState<AppSettings | null>(null)
  const [settingsForm, setSettingsForm] = useState<UpdateAppSettingsRequest>(defaultSettings)
  const [equipmentSets, setEquipmentSets] = useState<EquipmentSet[]>([])
  const [techniquePresets, setTechniquePresets] = useState<TechniquePreset[]>([])
  const [selectedAccountId, setSelectedAccountId] = useState<number | 'new'>('new')
  const [accountForm, setAccountForm] = useState<AccountPayload>(emptyAccount)
  const [selectedEquipmentSetId, setSelectedEquipmentSetId] = useState<number | null>(null)
  const [equipmentForm, setEquipmentForm] = useState<EquipmentSetPayload>({
    accountId: null,
    kind: 'farm',
  })
  const [selectedTechniquePresetId, setSelectedTechniquePresetId] = useState<number | null>(null)
  const [techniqueForm, setTechniqueForm] = useState<TechniquePresetPayload>({
    accountId: null,
    kind: 'farm',
  })
  const [techniqueSlots, setTechniqueSlots] = useState<Record<number, TechniquePresetSlot>>({})
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const activeAccount = useMemo(
    () => accounts.find((account) => account.id === settings?.activeAccountId) ?? null,
    [accounts, settings?.activeAccountId])

  const refreshConfiguration = useCallback(async () => {
    try {
      const [nextAccounts, nextSettings, nextEquipmentSets, nextTechniquePresets] = await Promise.all([
        getAccounts(),
        getAppSettings(),
        getEquipmentSets(),
        getTechniquePresets(),
      ])

      setAccounts(nextAccounts)
      setSettings(nextSettings)
      setSettingsForm(settingsToForm(nextSettings))
      setEquipmentSets(nextEquipmentSets)
      setTechniquePresets(nextTechniquePresets)
      syncSelectedAccount(selectedAccountId, nextAccounts)
      syncSelectedEquipmentSet(selectedEquipmentSetId, nextEquipmentSets)
      syncSelectedTechniquePreset(selectedTechniquePresetId, nextTechniquePresets)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось загрузить конфигурацию.')
    } finally {
      setIsLoading(false)
    }
  }, [selectedAccountId, selectedEquipmentSetId, selectedTechniquePresetId])

  useEffect(() => {
    const refreshId = window.setTimeout(() => {
      void refreshConfiguration()
    }, 0)

    return () => {
      window.clearTimeout(refreshId)
    }
  }, [refreshConfiguration])

  useEffect(() => {
    if (selectedTechniquePresetId === null) {
      return
    }

    void refreshTechniqueSlots(selectedTechniquePresetId)
  }, [selectedTechniquePresetId])

  function syncSelectedAccount(id: number | 'new', nextAccounts: Account[]) {
    if (id === 'new') {
      setAccountForm(emptyAccount)
      return
    }

    const account = nextAccounts.find((item) => item.id === id)

    if (account) {
      setAccountForm(accountToForm(account))
    } else {
      setSelectedAccountId('new')
      setAccountForm(emptyAccount)
    }
  }

  function syncSelectedEquipmentSet(id: number | null, nextSets: EquipmentSet[]) {
    const set = id === null ? nextSets[0] : nextSets.find((item) => item.id === id)

    setSelectedEquipmentSetId(set?.id ?? null)
    setEquipmentForm(set ? setToForm(set) : { accountId: null, kind: 'farm' })
  }

  function syncSelectedTechniquePreset(id: number | null, nextPresets: TechniquePreset[]) {
    const preset = id === null ? nextPresets[0] : nextPresets.find((item) => item.id === id)

    setSelectedTechniquePresetId(preset?.id ?? null)
    setTechniqueForm(preset ? presetToForm(preset) : { accountId: null, kind: 'farm' })
    setTechniqueSlots({})
  }

  async function refreshTechniqueSlots(id: number) {
    try {
      const slots = await getTechniquePresetSlots(id)
      setTechniqueSlots(Object.fromEntries(slots.map((slot) => [slot.techniqueNumber, slot])))
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось загрузить приемы.')
    }
  }

  async function saveAccount() {
    await saveAsync(async () => {
      if (selectedAccountId === 'new') {
        await createAccount(accountForm)
      } else {
        await updateAccount(selectedAccountId, accountForm)
      }

      await refreshConfiguration()
      await onConfigurationChanged()
    }, 'Аккаунт сохранен.')
  }

  async function chooseActiveAccount(accountId: number | null) {
    await saveAsync(async () => {
      await setActiveAccount(accountId)
      await refreshConfiguration()
      await onConfigurationChanged()
    }, accountId === null ? 'Активный аккаунт сброшен.' : 'Активный аккаунт выбран.')
  }

  async function saveSettings() {
    await saveAsync(async () => {
      await updateAppSettings(settingsForm)
      await refreshConfiguration()
      await onConfigurationChanged()
    }, 'Настройки сохранены.')
  }

  async function saveEquipmentSet() {
    await saveAsync(async () => {
      if (selectedEquipmentSetId === null) {
        await createEquipmentSet(equipmentForm)
      } else {
        await updateEquipmentSet(selectedEquipmentSetId, equipmentForm)
      }

      await refreshConfiguration()
    }, 'Сет сохранен.')
  }

  async function readCurrentEquipmentSet() {
    if (selectedEquipmentSetId === null) {
      setError('Сначала создайте или выберите сет.')
      return
    }

    await saveAsync(async () => {
      await readCurrentEquipmentSetSlots(selectedEquipmentSetId)
      await refreshConfiguration()
    }, 'Текущий сет считан и сохранен.')
  }

  async function saveTechniquePreset() {
    await saveAsync(async () => {
      let presetId = selectedTechniquePresetId

      if (selectedTechniquePresetId === null) {
        presetId = await createTechniquePreset(techniqueForm)
        setSelectedTechniquePresetId(presetId)
      } else {
        await updateTechniquePreset(selectedTechniquePresetId, techniqueForm)
      }

      if (presetId !== null) {
        await Promise.all(techniqueNames.map((techniqueName, index) => {
          const techniqueNumber = index + 1
          const slot = techniqueSlots[techniqueNumber] ?? createEmptyTechniqueSlot(presetId, techniqueNumber, techniqueName)

          return updateTechniquePresetSlot(presetId, techniqueNumber, {
            techniqueName,
            isEnabled: slot.isEnabled,
          })
        }))

        await refreshTechniqueSlots(presetId)
      }

      await refreshConfiguration()
    }, 'Пресет и приемы сохранены.')
  }

  async function saveAsync(action: () => Promise<void>, successMessage: string) {
    setIsSaving(true)
    setError(null)
    setMessage(null)

    try {
      await action()
      setMessage(successMessage)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось сохранить изменения.')
    } finally {
      setIsSaving(false)
    }
  }

  function selectAccount(value: string) {
    if (value === 'new') {
      setSelectedAccountId('new')
      setAccountForm(emptyAccount)
      return
    }

    const accountId = Number(value)
    const account = accounts.find((item) => item.id === accountId)

    setSelectedAccountId(accountId)
    setAccountForm(account ? accountToForm(account) : emptyAccount)
  }

  function selectEquipmentSet(value: string) {
    if (value === 'new') {
      setSelectedEquipmentSetId(null)
      setEquipmentForm({ accountId: settings?.activeAccountId ?? null, kind: 'farm' })
      return
    }

    const setId = Number(value)
    const set = equipmentSets.find((item) => item.id === setId)

    setSelectedEquipmentSetId(setId)
    setEquipmentForm(set ? setToForm(set) : { accountId: null, kind: 'farm' })
  }

  function selectTechniquePreset(value: string) {
    if (value === 'new') {
      setSelectedTechniquePresetId(null)
      setTechniqueForm({ accountId: settings?.activeAccountId ?? null, kind: 'farm' })
      setTechniqueSlots({})
      return
    }

    const presetId = Number(value)
    const preset = techniquePresets.find((item) => item.id === presetId)

    setSelectedTechniquePresetId(presetId)
    setTechniqueForm(preset ? presetToForm(preset) : { accountId: null, kind: 'farm' })
  }

  return (
    <section className="settings-panel">
      <div className="settings-panel-header">
        <div>
          <span className="eyebrow">SQLite конфигурация</span>
          <h2>Настройки</h2>
        </div>

        <div className="settings-tabs" role="tablist" aria-label="Разделы настроек">
          <button className={tab === 'accounts' ? 'settings-tab-active' : ''} onClick={() => setTab('accounts')}>
            <Users size={18} />
            Аккаунты
          </button>
          <button className={tab === 'equipment' ? 'settings-tab-active' : ''} onClick={() => setTab('equipment')}>
            <Shield size={18} />
            Сеты
          </button>
          <button className={tab === 'techniques' ? 'settings-tab-active' : ''} onClick={() => setTab('techniques')}>
            <Swords size={18} />
            Приемы
          </button>
          <button className={tab === 'advanced' ? 'settings-tab-active settings-tab-danger' : ''} onClick={() => setTab('advanced')}>
            <Settings size={18} />
            Продвинутые настройки
          </button>
        </div>
      </div>

      {isLoading && <p className="muted-line">Загружаю конфигурацию...</p>}
      {message && <p className="success-line">{message}</p>}
      {error && <p className="panel-error">{error}</p>}

      {tab === 'accounts' && (
        <div className="settings-grid">
          <div className="settings-list">
            <div className="select-row">
              <label>
                Аккаунт
                <select value={selectedAccountId} onChange={(event) => selectAccount(event.target.value)}>
                  <option value="new">Новый аккаунт</option>
                  {accounts.map((account) => (
                    <option key={account.id} value={account.id}>
                      {account.login}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            <div className="account-list">
              {accounts.map((account) => (
                <button
                  key={account.id}
                  className={account.id === activeAccount?.id ? 'account-list-item account-list-item-active' : 'account-list-item'}
                  onClick={() => void chooseActiveAccount(account.id)}>
                  <strong>{account.login}</strong>
                  <span>{account.isEnabled ? 'Включен' : 'Отключен'}</span>
                </button>
              ))}
            </div>

            <Button variant="ghost" disabled={isSaving || !settings?.activeAccountId} onClick={() => void chooseActiveAccount(null)}>
              Сбросить активный
            </Button>
          </div>

          <div className="settings-form">
            <div className="form-grid">
              <label>
                Логин
                <input
                  value={accountForm.login}
                  onChange={(event) => setAccountForm((current) => ({
                    ...current,
                    login: event.target.value,
                  }))}
                />
              </label>

              <label>
                Пароль
                <input
                  type="password"
                  value={accountForm.encryptedPassword ?? ''}
                  onChange={(event) => setAccountForm((current) => ({ ...current, encryptedPassword: event.target.value }))}
                />
              </label>
            </div>

            <ToggleRow
              title="Аккаунт включен"
              description="Отключенные аккаунты остаются в базе, но помечаются неактивными."
              checked={accountForm.isEnabled}
              onChange={(checked) => setAccountForm((current) => ({ ...current, isEnabled: checked }))}
            />

            <div className="actions">
              <Button disabled={isSaving} onClick={() => void saveAccount()}>
                <UserPlus size={18} />
                Сохранить аккаунт
              </Button>
            </div>
          </div>
        </div>
      )}

      {tab === 'advanced' && (
        <div className="settings-form">
          <div className="danger-note">
            <AlertTriangle size={22} />
            <p>
              Продвинутые настройки. Меняй их только если понимаешь последствия:
              слишком агрессивные задержки, таймауты и пороги могут ломать сценарии или выглядеть неестественно для игры.
            </p>
          </div>

          <div className="form-grid advanced-form-grid">
            <label>
              Base URL
              <input
                value={settingsForm.baseUrl}
                onChange={(event) => setSettingsForm((current) => ({ ...current, baseUrl: event.target.value }))}
              />
              <small>
                Адрес игры, к которому будут относиться переходы и прямые HTTP-запросы. Менять стоит только при переезде домена.
              </small>
            </label>

            <label>
              Браузер
              <select
                value={settingsForm.browserKind}
                onChange={(event) => setSettingsForm((current) => ({ ...current, browserKind: event.target.value }))}>
                <option value="chrome">Google Chrome</option>
                <option value="edge">Microsoft Edge</option>
                <option value="chromium">Chromium Playwright</option>
                <option value="firefox">Firefox Playwright</option>
                <option value="webkit">WebKit Playwright</option>
              </select>
              <small>
                Поддерживается ограниченный список движков Playwright. Chrome и Edge используют установленные браузеры,
                остальные варианты запускаются из браузеров Playwright.
              </small>
            </label>

            <label>
              Профиль браузера
              <input
                value={settingsForm.userDataDir ?? ''}
                onChange={(event) => setSettingsForm((current) => ({ ...current, userDataDir: event.target.value }))}
              />
              <small>
                Путь к папке профиля выбранного браузера, где приложение хранит cookies, сессию и локальные данные.
                Обычно можно оставить пустым: приложение использует стандартную папку в AppData.
              </small>
            </label>

            <NumberField
              label="Мин. задержка, мс"
              description="Нижняя граница случайной паузы между действиями. Ноль и очень маленькие значения делают поведение резким."
              value={settingsForm.minDelayMs}
              onChange={(value) => setSettingsForm((current) => ({ ...current, minDelayMs: value }))}
            />
            <NumberField
              label="Макс. задержка, мс"
              description="Верхняя граница случайной паузы между действиями. Должна быть не меньше минимальной задержки."
              value={settingsForm.maxDelayMs}
              onChange={(value) => setSettingsForm((current) => ({ ...current, maxDelayMs: value }))}
            />
            <NumberField
              label="Таймаут, мс"
              description="Сколько ждать элементы страницы и загрузку навигации перед ошибкой Playwright."
              value={settingsForm.defaultTimeoutMs}
              onChange={(value) => setSettingsForm((current) => ({ ...current, defaultTimeoutMs: value }))}
            />
            <NumberField
              label="Множитель восстановления HP"
              description="Коэффициент ожидания восстановления здоровья после боя. Больше значение - дольше пауза."
              value={settingsForm.hpRecoveryDelayMultiplier}
              onChange={(value) => setSettingsForm((current) => ({ ...current, hpRecoveryDelayMultiplier: value }))}
            />
            <NumberField
              label="Мин. HP для атаки"
              description="Нижний порог здоровья для продолжения атак. Значение задается долей: 0.25 означает 25%."
              step={0.01}
              value={settingsForm.minAttackHealthPercent}
              onChange={(value) => setSettingsForm((current) => ({ ...current, minAttackHealthPercent: value }))}
            />
            <NumberField
              label="Макс. HP для атаки"
              description="Верхний порог случайного выбора здоровья для атаки. Значение задается долей: 0.30 означает 30%."
              step={0.01}
              value={settingsForm.maxAttackHealthPercent}
              onChange={(value) => setSettingsForm((current) => ({ ...current, maxAttackHealthPercent: value }))}
            />
            <NumberField
              label="Повторов шага"
              description="Сколько раз повторять шаг сценария при таймауте или временной ошибке перед остановкой."
              value={settingsForm.maxStepRetryCount}
              onChange={(value) => setSettingsForm((current) => ({ ...current, maxStepRetryCount: value }))}
            />
            <NumberField
              label="Retry delay, мс"
              description="Пауза между повторными попытками после временной ошибки."
              value={settingsForm.retryDelayMs}
              onChange={(value) => setSettingsForm((current) => ({ ...current, retryDelayMs: value }))}
            />
            <NumberField
              label="Повтор авторизации, мс"
              description="Пауза перед повторной попыткой авторизации, если сессия потеряна."
              value={settingsForm.authenticationRetryDelayMs}
              onChange={(value) => setSettingsForm((current) => ({ ...current, authenticationRetryDelayMs: value }))}
            />
          </div>

          <ToggleRow
            title="Headless-режим"
            description="Запускать браузер без видимого окна."
            checked={settingsForm.headless}
            onChange={(checked) => setSettingsForm((current) => ({ ...current, headless: checked }))}
          />

          <div className="actions">
            <Button disabled={isSaving} onClick={() => void saveSettings()}>
              <Save size={18} />
              Сохранить настройки
            </Button>
          </div>
        </div>
      )}

      {tab === 'equipment' && (
        <SetEditor
          accounts={accounts}
          disabled={isSaving}
          itemLabel="Сет"
          selectedId={selectedEquipmentSetId}
          items={equipmentSets}
          form={equipmentForm}
          onSelect={selectEquipmentSet}
          onChange={setEquipmentForm}
          onSave={() => void saveEquipmentSet()}
        >
          <div className="empty-state">
            <p>
              Нужно надеть сет в игре и нажать кнопку чтения текущего снаряжения.
            </p>
            <Button
              variant="ghost"
              disabled={isSaving || selectedEquipmentSetId === null}
              onClick={() => void readCurrentEquipmentSet()}
            >
              Считать текущий сет
            </Button>
          </div>
        </SetEditor>
      )}

      {tab === 'techniques' && (
        <SetEditor
          accounts={accounts}
          disabled={isSaving}
          itemLabel="Пресет"
          selectedId={selectedTechniquePresetId}
          items={techniquePresets}
          form={techniqueForm}
          onSelect={selectTechniquePreset}
          onChange={setTechniqueForm}
          onSave={() => void saveTechniquePreset()}
        >
          <div className="technique-grid">
            {techniqueNames.map((techniqueName, index) => {
              const techniqueNumber = index + 1
              const slot = techniqueSlots[techniqueNumber]
                ?? createEmptyTechniqueSlot(selectedTechniquePresetId ?? 0, techniqueNumber, techniqueName)

              return (
                <div className="technique-row" key={techniqueName}>
                  <strong>{techniqueNumber}</strong>
                  <span>{techniqueName}</span>
                  <label>
                    <input
                      type="checkbox"
                      checked={slot.isEnabled}
                      onChange={(event) => setTechniqueSlots((current) => ({
                        ...current,
                        [techniqueNumber]: { ...slot, isEnabled: event.target.checked },
                      }))}
                    />
                    Вкл.
                  </label>
                </div>
              )
            })}
          </div>
        </SetEditor>
      )}
    </section>
  )
}

type NumberFieldProps = {
  label: string
  description: string
  step?: number
  value: number
  onChange: (value: number) => void
}

function NumberField({ label, description, step = 1, value, onChange }: NumberFieldProps) {
  return (
    <label>
      {label}
      <input
        type="number"
        step={step}
        value={value}
        onChange={(event) => onChange(Number(event.target.value))}
      />
      <small>{description}</small>
    </label>
  )
}

type SetEditorProps<TItem extends { id: number; accountId: number | null; kind: ConfigurationKind }> = {
  accounts: Account[]
  disabled: boolean
  itemLabel: string
  selectedId: number | null
  items: TItem[]
  form: Omit<TItem, 'id'>
  children: ReactNode
  onSelect: (value: string) => void
  onChange: (value: Omit<TItem, 'id'>) => void
  onSave: () => void
}

function SetEditor<TItem extends { id: number; accountId: number | null; kind: ConfigurationKind }>({
  accounts,
  disabled,
  itemLabel,
  selectedId,
  items,
  form,
  children,
  onSelect,
  onChange,
  onSave,
}: SetEditorProps<TItem>) {
  return (
    <div className="settings-grid">
      <div className="settings-list">
        <div className="select-row">
          <label>
            {itemLabel}
            <select value={selectedId ?? 'new'} onChange={(event) => onSelect(event.target.value)}>
              <option value="new">Новый</option>
              {items.map((item) => (
                <option key={item.id} value={item.id}>
                  {getModeName(item.kind)}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <div className="settings-form">
        <div className="form-grid">
          <label>
            Режим
            <select value={form.kind} onChange={(event) => onChange({ ...form, kind: event.target.value as ConfigurationKind })}>
              <option value="farm">Фарм</option>
              <option value="combat">Бой</option>
            </select>
          </label>

          <label>
            Аккаунт
            <select
              value={form.accountId ?? ''}
              onChange={(event) => onChange({ ...form, accountId: event.target.value ? Number(event.target.value) : null })}>
              <option value="">Общий</option>
              {accounts.map((account) => (
                <option key={account.id} value={account.id}>
                  {account.login}
                </option>
              ))}
            </select>
          </label>
        </div>

        <div className="actions">
          <Button disabled={disabled} onClick={onSave}>
            <Save size={18} />
            Сохранить
          </Button>
        </div>

        {children}
      </div>
    </div>
  )
}

function settingsToForm(settings: AppSettings): UpdateAppSettingsRequest {
  return {
    baseUrl: settings.baseUrl,
    browserKind: settings.browserKind,
    headless: settings.headless,
    userDataDir: settings.userDataDir ?? '',
    minDelayMs: settings.minDelayMs,
    maxDelayMs: settings.maxDelayMs,
    defaultTimeoutMs: settings.defaultTimeoutMs,
    hpRecoveryDelayMultiplier: settings.hpRecoveryDelayMultiplier,
    minAttackHealthPercent: settings.minAttackHealthPercent,
    maxAttackHealthPercent: settings.maxAttackHealthPercent,
    maxStepRetryCount: settings.maxStepRetryCount,
    retryDelayMs: settings.retryDelayMs,
    authenticationRetryDelayMs: settings.authenticationRetryDelayMs,
  }
}

function accountToForm(account: Account): AccountPayload {
  return {
    login: account.login,
    encryptedPassword: account.encryptedPassword ?? '',
    isEnabled: account.isEnabled,
  }
}

function setToForm(set: EquipmentSet): EquipmentSetPayload {
  return {
    accountId: set.accountId,
    kind: set.kind,
  }
}

function presetToForm(preset: TechniquePreset): TechniquePresetPayload {
  return {
    accountId: preset.accountId,
    kind: preset.kind,
  }
}

function getModeName(kind: ConfigurationKind) {
  return kind === 'farm' ? 'Фарм' : 'Бой'
}

function createEmptyTechniqueSlot(
  techniquePresetId: number,
  techniqueNumber: number,
  techniqueName = ''): TechniquePresetSlot {
  return {
    techniquePresetId,
    techniqueNumber,
    techniqueName,
    isEnabled: false,
  }
}
