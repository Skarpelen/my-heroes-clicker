CREATE TABLE accounts (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  login TEXT NOT NULL,
  encrypted_password TEXT NULL,
  is_enabled INTEGER NOT NULL DEFAULT 1,
  created_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_utc TEXT NULL
);

CREATE TABLE app_settings (
  id INTEGER PRIMARY KEY CHECK (id = 1),
  active_account_id INTEGER NULL REFERENCES accounts(id) ON DELETE SET NULL,
  base_url TEXT NOT NULL,
  browser_kind TEXT NOT NULL DEFAULT 'chrome' CHECK (browser_kind IN ('chromium', 'chrome', 'edge', 'firefox', 'webkit')),
  headless INTEGER NOT NULL,
  user_data_dir TEXT NULL,
  min_delay_ms INTEGER NOT NULL,
  max_delay_ms INTEGER NOT NULL,
  default_timeout_ms INTEGER NOT NULL,
  hp_recovery_delay_multiplier INTEGER NOT NULL,
  min_attack_health_percent REAL NOT NULL,
  max_attack_health_percent REAL NOT NULL,
  max_step_retry_count INTEGER NOT NULL,
  retry_delay_ms INTEGER NOT NULL,
  authentication_retry_delay_ms INTEGER NOT NULL
);

CREATE TABLE equipment_sets (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  account_id INTEGER NULL REFERENCES accounts(id) ON DELETE CASCADE,
  kind TEXT NOT NULL CHECK (kind IN ('farm', 'combat')),
  created_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_utc TEXT NULL,
  UNIQUE(account_id, kind)
);

CREATE TABLE equipment_set_slots (
  equipment_set_id INTEGER NOT NULL REFERENCES equipment_sets(id) ON DELETE CASCADE,
  slot_number INTEGER NOT NULL,
  item_id INTEGER NULL,
  expected_image_src TEXT NOT NULL DEFAULT '',
  should_be_empty INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY (equipment_set_id, slot_number)
);

CREATE TABLE technique_presets (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  account_id INTEGER NULL REFERENCES accounts(id) ON DELETE CASCADE,
  kind TEXT NOT NULL CHECK (kind IN ('farm', 'combat')),
  created_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_utc TEXT NULL,
  UNIQUE(account_id, kind)
);

CREATE TABLE technique_preset_slots (
  technique_preset_id INTEGER NOT NULL REFERENCES technique_presets(id) ON DELETE CASCADE,
  technique_number INTEGER NOT NULL CHECK (technique_number BETWEEN 1 AND 15),
  technique_name TEXT NOT NULL DEFAULT '',
  is_enabled INTEGER NOT NULL,
  PRIMARY KEY (technique_preset_id, technique_number)
);

CREATE INDEX idx_equipment_sets_account_kind
ON equipment_sets(account_id, kind);

CREATE INDEX idx_technique_presets_account_kind
ON technique_presets(account_id, kind);

INSERT INTO app_settings (
  id,
  active_account_id,
  base_url,
  browser_kind,
  headless,
  user_data_dir,
  min_delay_ms,
  max_delay_ms,
  default_timeout_ms,
  hp_recovery_delay_multiplier,
  min_attack_health_percent,
  max_attack_health_percent,
  max_step_retry_count,
  retry_delay_ms,
  authentication_retry_delay_ms
)
VALUES (
  1,
  NULL,
  'https://myheroes.ru/',
  'chrome',
  0,
  NULL,
  100,
  250,
  10000,
  20,
  0.25,
  0.30,
  10,
  1000,
  60000
);
