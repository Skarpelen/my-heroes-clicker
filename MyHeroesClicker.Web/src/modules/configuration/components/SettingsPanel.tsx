import { useCallback, useEffect, useMemo, useState } from 'react'
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
  updateTechniquePresetSlot,
} from '../api/configurationApi'
import type {
  Account,
  AccountPayload,
  AppSettings,
  ConfigurationKind,
  EquipmentSet,
  TechniquePreset,
  TechniquePresetSlot,
  UpdateAppSettingsRequest,
} from '../model/types'

type SettingsTab = 'accounts' | 'equipment' | 'techniques' | 'advanced'

type SettingsPanelProps = {
  onConfigurationChanged: () => Promise<void>
}

const emptyAccount: AccountPayload = {
  login: '',
  password: '',
  isEnabled: true,
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

const configurationKinds: ConfigurationKind[] = ['farm', 'combat']

export function SettingsPanel({ onConfigurationChanged }: SettingsPanelProps) {
  const [tab, setTab] = useState<SettingsTab>('accounts')
  const [accounts, setAccounts] = useState<Account[]>([])
  const [settings, setSettings] = useState<AppSettings | null>(null)
  const [settingsForm, setSettingsForm] = useState<UpdateAppSettingsRequest | null>(null)
  const [equipmentSets, setEquipmentSets] = useState<EquipmentSet[]>([])
  const [techniquePresets, setTechniquePresets] = useState<TechniquePreset[]>([])
  const [selectedAccountId, setSelectedAccountId] = useState<number | null>(null)
  const [accountForm, setAccountForm] = useState<AccountPayload>(emptyAccount)
  const [newAccountForm, setNewAccountForm] = useState<AccountPayload>(emptyAccount)
  const [equipmentKind, setEquipmentKind] = useState<ConfigurationKind>('farm')
  const [techniqueKind, setTechniqueKind] = useState<ConfigurationKind>('farm')
  const [techniqueSlots, setTechniqueSlots] = useState<Record<number, TechniquePresetSlot>>({})
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const activeAccount = useMemo(
    () => accounts.find((account) => account.id === settings?.activeAccountId) ?? null,
    [accounts, settings?.activeAccountId])
  const selectedAccount = useMemo(
    () => accounts.find((account) => account.id === selectedAccountId) ?? null,
    [accounts, selectedAccountId])
  const selectedEquipmentSet = useMemo(
    () => findConfiguration(equipmentSets, selectedAccountId, equipmentKind),
    [equipmentKind, equipmentSets, selectedAccountId])
  const selectedTechniquePreset = useMemo(
    () => findConfiguration(techniquePresets, selectedAccountId, techniqueKind),
    [selectedAccountId, techniqueKind, techniquePresets])

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
      syncSelectedAccount(selectedAccountId, nextAccounts, nextSettings)
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Не удалось загрузить конфигурацию.')
    } finally {
      setIsLoading(false)
    }
  }, [selectedAccountId])

  useEffect(() => {
    const refreshId = window.setTimeout(() => {
      void refreshConfiguration()
    }, 0)

    return () => {
      window.clearTimeout(refreshId)
    }
  }, [refreshConfiguration])

  useEffect(() => {
    const refreshId = window.setTimeout(() => {
      if (selectedTechniquePreset === null) {
        setTechniqueSlots({})
        return
      }

      void refreshTechniqueSlots(selectedTechniquePreset.id)
    }, 0)

    return () => {
      window.clearTimeout(refreshId)
    }
  }, [selectedTechniquePreset])

  function syncSelectedAccount(id: number | null, nextAccounts: Account[], nextSettings: AppSettings) {
    const fallbackAccount = nextAccounts.find((item) => item.id === nextSettings.activeAccountId) ?? nextAccounts[0]
    const account = id === null ? fallbackAccount : nextAccounts.find((item) => item.id === id) ?? fallbackAccount

    if (account) {
      setSelectedAccountId(account.id)
      setAccountForm(accountToForm(account))
    } else {
      setSelectedAccountId(null)
      setAccountForm(emptyAccount)
    }
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
    if (selectedAccountId === null) {
      setError('Выберите аккаунт для редактирования.')
      return
    }

    await saveAsync(async () => {
      await updateAccount(selectedAccountId, accountToUpdatePayload(accountForm))

      await refreshConfiguration()
      await onConfigurationChanged()
    }, 'Аккаунт сохранен.')
  }

  async function createNewAccount() {
    await saveAsync(async () => {
      const nextAccountForm = accountToCreatePayload(newAccountForm)
      const accountId = await createAccount(nextAccountForm)

      setSelectedAccountId(accountId)
      setAccountForm(emptyAccount)
      setNewAccountForm(emptyAccount)

      await refreshConfiguration()
      setSelectedAccountId(accountId)
      await onConfigurationChanged()
    }, 'Аккаунт создан.')
  }

  async function chooseActiveAccount(accountId: number | null) {
    await saveAsync(async () => {
      await setActiveAccount(accountId)
      await refreshConfiguration()
      await onConfigurationChanged()
    }, accountId === null ? 'Активный аккаунт сброшен.' : 'Активный аккаунт выбран.')
  }

  async function saveSettings() {
    if (settingsForm === null) {
      setError('Настройки еще не загружены.')
      return
    }

    await saveAsync(async () => {
      await updateAppSettings(settingsForm)
      await refreshConfiguration()
      await onConfigurationChanged()
    }, 'Настройки сохранены.')
  }

  async function ensureEquipmentSetId(kind: ConfigurationKind) {
    if (selectedAccountId === null) {
      throw new Error('Сначала выберите аккаунт.')
    }

    const existingSet = findConfiguration(equipmentSets, selectedAccountId, kind)

    if (existingSet) {
      return existingSet.id
    }

    return await createEquipmentSet({ accountId: selectedAccountId, kind })
  }

  async function readCurrentEquipmentSet() {
    await saveAsync(async () => {
      const setId = await ensureEquipmentSetId(equipmentKind)

      await readCurrentEquipmentSetSlots(setId)
      await refreshConfiguration()
    }, 'Текущий сет считан и сохранен.')
  }

  async function ensureTechniquePresetId(kind: ConfigurationKind) {
    if (selectedAccountId === null) {
      throw new Error('Сначала выберите аккаунт.')
    }

    const existingPreset = findConfiguration(techniquePresets, selectedAccountId, kind)

    if (existingPreset) {
      return existingPreset.id
    }

    return await createTechniquePreset({ accountId: selectedAccountId, kind })
  }

  async function saveTechniquePreset() {
    await saveAsync(async () => {
      const presetId = await ensureTechniquePresetId(techniqueKind)

      await Promise.all(techniqueNames.map((techniqueName, index) => {
        const techniqueNumber = index + 1
        const slot = techniqueSlots[techniqueNumber] ?? createEmptyTechniqueSlot(presetId, techniqueNumber, techniqueName)

        return updateTechniquePresetSlot(presetId, techniqueNumber, {
          techniqueName,
          isEnabled: slot.isEnabled,
        })
      }))

      await refreshTechniqueSlots(presetId)

      await refreshConfiguration()
    }, 'Пресет и приемы сохранены.')
  }

  function setAllTechniqueSlots(isEnabled: boolean) {
    setTechniqueSlots(Object.fromEntries(techniqueNames.map((techniqueName, index) => {
      const techniqueNumber = index + 1
      const slot = techniqueSlots[techniqueNumber]
        ?? createEmptyTechniqueSlot(selectedTechniquePreset?.id ?? 0, techniqueNumber, techniqueName)

      return [techniqueNumber, { ...slot, isEnabled }]
    })))
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

  function selectAccount(accountId: number) {
    const account = accounts.find((item) => item.id === accountId)

    setSelectedAccountId(accountId)
    setAccountForm(account ? accountToForm(account) : emptyAccount)
  }

  function updateSettingsForm(update: Partial<UpdateAppSettingsRequest>) {
    setSettingsForm((current) => current === null ? current : { ...current, ...update })
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
            <div className="account-list">
              {accounts.map((account) => (
                <button
                  key={account.id}
                  className={account.id === selectedAccountId ? 'account-list-item account-list-item-active' : 'account-list-item'}
                  onClick={() => selectAccount(account.id)}>
                  <strong>{account.login}</strong>
                  <span>
                    {getAccountStateLabel(account, activeAccount)}
                  </span>
                </button>
              ))}
            </div>

            <div className="actions">
              <Button
                variant="ghost"
                disabled={isSaving || selectedAccountId === null || selectedAccountId === settings?.activeAccountId}
                onClick={() => {
                  if (selectedAccountId !== null) {
                    void chooseActiveAccount(selectedAccountId)
                  }
                }}
              >
                Сделать активным
              </Button>
              <Button variant="ghost" disabled={isSaving || !settings?.activeAccountId} onClick={() => void chooseActiveAccount(null)}>
                Сбросить активный
              </Button>
            </div>
          </div>

          <div className="settings-form">
            <h3>Новый аккаунт</h3>
            <div className="form-grid">
              <label>
                Логин
                <input
                  value={newAccountForm.login}
                  onChange={(event) => setNewAccountForm((current) => ({
                    ...current,
                    login: event.target.value,
                  }))}
                />
              </label>

              <label>
                Пароль
                <input
                  type="password"
                  value={newAccountForm.password ?? ''}
                  onChange={(event) => setNewAccountForm((current) => ({
                    ...current,
                    password: event.target.value,
                  }))}
                />
              </label>
            </div>

            <div className="actions">
              <Button disabled={isSaving} onClick={() => void createNewAccount()}>
                <UserPlus size={18} />
                Добавить аккаунт
              </Button>
            </div>

            <h3>{selectedAccount ? `Редактирование: ${selectedAccount.login}` : 'Выберите аккаунт'}</h3>
            <div className="form-grid">
              <label>
                Логин
                <input
                  disabled={selectedAccountId === null}
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
                  disabled={selectedAccountId === null}
                  placeholder={selectedAccount?.hasPassword ? 'Пароль сохранен' : ''}
                  value={accountForm.password ?? ''}
                  onChange={(event) => setAccountForm((current) => ({ ...current, password: event.target.value }))}
                />
                <small>
                  {selectedAccount?.hasPassword
                    ? 'Оставьте поле пустым, чтобы сохранить текущий пароль.'
                    : 'Введите пароль для первичной авторизации.'}
                </small>
              </label>
            </div>

            <ToggleRow
              title="Аккаунт включен"
              description="Отключенные аккаунты остаются в базе, но помечаются неактивными."
              checked={accountForm.isEnabled}
              disabled={selectedAccountId === null}
              onChange={(checked) => setAccountForm((current) => ({ ...current, isEnabled: checked }))}
            />

            <div className="actions">
              <Button disabled={isSaving || selectedAccountId === null} onClick={() => void saveAccount()}>
                <Save size={18} />
                Сохранить выбранный аккаунт
              </Button>
            </div>
          </div>
        </div>
      )}

      {tab === 'advanced' && settingsForm !== null && (
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
                onChange={(event) => updateSettingsForm({ baseUrl: event.target.value })}
              />
              <small>
                Адрес игры, к которому будут относиться переходы и прямые HTTP-запросы. Менять стоит только при переезде домена.
              </small>
            </label>

            <label>
              Браузер
              <select
                value={settingsForm.browserKind}
                onChange={(event) => updateSettingsForm({ browserKind: event.target.value })}>
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
                onChange={(event) => updateSettingsForm({ userDataDir: event.target.value })}
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
              onChange={(value) => updateSettingsForm({ minDelayMs: value })}
            />
            <NumberField
              label="Макс. задержка, мс"
              description="Верхняя граница случайной паузы между действиями. Должна быть не меньше минимальной задержки."
              value={settingsForm.maxDelayMs}
              onChange={(value) => updateSettingsForm({ maxDelayMs: value })}
            />
            <NumberField
              label="Таймаут, мс"
              description="Сколько ждать элементы страницы и загрузку навигации перед ошибкой Playwright."
              value={settingsForm.defaultTimeoutMs}
              onChange={(value) => updateSettingsForm({ defaultTimeoutMs: value })}
            />
            <NumberField
              label="Множитель восстановления HP"
              description="Коэффициент ожидания восстановления здоровья после боя. Больше значение - дольше пауза."
              value={settingsForm.hpRecoveryDelayMultiplier}
              onChange={(value) => updateSettingsForm({ hpRecoveryDelayMultiplier: value })}
            />
            <NumberField
              label="Мин. HP для атаки"
              description="Нижний порог здоровья для продолжения атак. Значение задается долей: 0.25 означает 25%."
              step={0.01}
              value={settingsForm.minAttackHealthPercent}
              onChange={(value) => updateSettingsForm({ minAttackHealthPercent: value })}
            />
            <NumberField
              label="Макс. HP для атаки"
              description="Верхний порог случайного выбора здоровья для атаки. Значение задается долей: 0.30 означает 30%."
              step={0.01}
              value={settingsForm.maxAttackHealthPercent}
              onChange={(value) => updateSettingsForm({ maxAttackHealthPercent: value })}
            />
            <NumberField
              label="Повторов шага"
              description="Сколько раз повторять шаг сценария при таймауте или временной ошибке перед остановкой."
              value={settingsForm.maxStepRetryCount}
              onChange={(value) => updateSettingsForm({ maxStepRetryCount: value })}
            />
            <NumberField
              label="Retry delay, мс"
              description="Пауза между повторными попытками после временной ошибки."
              value={settingsForm.retryDelayMs}
              onChange={(value) => updateSettingsForm({ retryDelayMs: value })}
            />
            <NumberField
              label="Повтор авторизации, мс"
              description="Пауза перед повторной попыткой авторизации, если сессия потеряна."
              value={settingsForm.authenticationRetryDelayMs}
              onChange={(value) => updateSettingsForm({ authenticationRetryDelayMs: value })}
            />
            <NumberField
              label="Проверка войны, мин"
              description="Как часто проверять страницу войны после окончания войны, при доступной атаке или неизвестном состоянии. Во время кулдауна используется известный таймер до следующей битвы."
              value={settingsForm.warCheckIntervalMinutes}
              onChange={(value) => updateSettingsForm({ warCheckIntervalMinutes: value })}
            />
            <NumberField
              label="Подготовка к войне, сек"
              description="За сколько секунд до начала боя остановить активный фарм и надеть боевой сет."
              value={settingsForm.warCombatPreparationSecondsBeforeRegistrationEnd}
              onChange={(value) => updateSettingsForm({ warCombatPreparationSecondsBeforeRegistrationEnd: value })}
            />
          </div>

          <ToggleRow
            title="Headless-режим"
            description="Запускать браузер без видимого окна."
            checked={settingsForm.headless}
            onChange={(checked) => updateSettingsForm({ headless: checked })}
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
        <div className="settings-form">
          <ConfigurationHeader
            selectedAccount={selectedAccount}
            selectedKind={equipmentKind}
            onSelectKind={setEquipmentKind}
          />

          <div className="empty-state">
            <p>{selectedEquipmentSet ? 'Сет готов к обновлению.' : 'Сет будет создан автоматически при чтении текущего снаряжения.'}</p>
            <Button disabled={isSaving || selectedAccountId === null} onClick={() => void readCurrentEquipmentSet()}>
              Считать текущий сет
            </Button>
          </div>
        </div>
      )}

      {tab === 'techniques' && (
        <div className="settings-form">
          <ConfigurationHeader
            selectedAccount={selectedAccount}
            selectedKind={techniqueKind}
            onSelectKind={setTechniqueKind}
          />

          <div className="technique-grid">
            {techniqueNames.map((techniqueName, index) => {
              const techniqueNumber = index + 1
              const slot = techniqueSlots[techniqueNumber]
                ?? createEmptyTechniqueSlot(selectedTechniquePreset?.id ?? 0, techniqueNumber, techniqueName)

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

          <div className="actions">
            <Button
              variant="ghost"
              disabled={isSaving || selectedAccountId === null}
              onClick={() => setAllTechniqueSlots(true)}
            >
              Включить все
            </Button>
            <Button
              variant="ghost"
              disabled={isSaving || selectedAccountId === null}
              onClick={() => setAllTechniqueSlots(false)}
            >
              Выключить все
            </Button>
            <Button disabled={isSaving || selectedAccountId === null} onClick={() => void saveTechniquePreset()}>
              <Save size={18} />
              Сохранить приемы
            </Button>
          </div>
        </div>
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

type ConfigurationHeaderProps = {
  selectedAccount: Account | null
  selectedKind: ConfigurationKind
  onSelectKind: (kind: ConfigurationKind) => void
}

function ConfigurationHeader({ selectedAccount, selectedKind, onSelectKind }: ConfigurationHeaderProps) {
  return (
    <div className="configuration-header">
      <div>
        <span>Аккаунт</span>
        <strong>{selectedAccount?.login ?? 'Не выбран'}</strong>
      </div>

      <div className="mode-switch" role="tablist" aria-label="Режим конфигурации">
        {configurationKinds.map((kind) => (
          <button
            key={kind}
            className={selectedKind === kind ? 'mode-switch-active' : ''}
            onClick={() => onSelectKind(kind)}
          >
            {getModeName(kind)}
          </button>
        ))}
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
    warCheckIntervalMinutes: settings.warCheckIntervalMinutes,
    warCombatPreparationSecondsBeforeRegistrationEnd: settings.warCombatPreparationSecondsBeforeRegistrationEnd,
  }
}

function accountToForm(account: Account): AccountPayload {
  return {
    login: account.login,
    password: '',
    isEnabled: account.isEnabled,
  }
}

function accountToCreatePayload(account: AccountPayload): AccountPayload {
  return {
    ...account,
    password: account.password?.trim() ? account.password : null,
  }
}

function accountToUpdatePayload(account: AccountPayload): AccountPayload {
  return {
    ...account,
    password: account.password?.trim() ? account.password : null,
  }
}

function getAccountStateLabel(account: Account, activeAccount: Account | null) {
  const state = account.id === activeAccount?.id ? 'Активный' : account.isEnabled ? 'Включен' : 'Отключен'
  const passwordState = account.hasPassword ? 'пароль есть' : 'пароль не задан'

  return `${state}, ${passwordState}`
}

function getModeName(kind: ConfigurationKind) {
  return kind === 'farm' ? 'Фарм' : 'Бой'
}

function findConfiguration<TItem extends { accountId: number | null; kind: ConfigurationKind }>(
  items: TItem[],
  accountId: number | null,
  kind: ConfigurationKind): TItem | null {
  if (accountId === null) {
    return null
  }

  return items.find((item) => item.accountId === accountId && item.kind === kind) ?? null
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
