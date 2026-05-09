PRAGMA foreign_keys = OFF;

CREATE TABLE accounts_new (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  login TEXT NOT NULL,
  encrypted_password TEXT NULL,
  is_enabled INTEGER NOT NULL DEFAULT 1,
  created_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_utc TEXT NULL
);

INSERT INTO accounts_new (
  id,
  login,
  encrypted_password,
  is_enabled,
  created_utc,
  updated_utc
)
SELECT
  id,
  login,
  encrypted_password,
  is_enabled,
  created_utc,
  updated_utc
FROM accounts;

DROP TABLE accounts;
ALTER TABLE accounts_new RENAME TO accounts;

ALTER TABLE app_settings
ADD COLUMN browser_kind TEXT NOT NULL DEFAULT 'chrome'
CHECK (browser_kind IN ('chromium', 'chrome', 'edge', 'firefox', 'webkit'));

CREATE TABLE equipment_sets_new (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  account_id INTEGER NULL REFERENCES accounts(id) ON DELETE CASCADE,
  kind TEXT NOT NULL CHECK (kind IN ('farm', 'combat')),
  created_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_utc TEXT NULL,
  UNIQUE(account_id, kind)
);

CREATE TEMP TABLE equipment_set_id_map AS
SELECT source.id AS old_id,
       target.id AS new_id
FROM equipment_sets source
JOIN (
  SELECT MIN(id) AS id,
         account_id,
         kind
  FROM equipment_sets
  GROUP BY account_id, kind
) target ON source.kind = target.kind
  AND (
    source.account_id = target.account_id
    OR (source.account_id IS NULL AND target.account_id IS NULL)
  );

INSERT INTO equipment_sets_new (
  id,
  account_id,
  kind,
  created_utc,
  updated_utc
)
SELECT id,
       account_id,
       kind,
       created_utc,
       updated_utc
FROM equipment_sets
WHERE id IN (SELECT new_id FROM equipment_set_id_map);

CREATE TABLE equipment_set_slots_new (
  equipment_set_id INTEGER NOT NULL REFERENCES equipment_sets_new(id) ON DELETE CASCADE,
  slot_number INTEGER NOT NULL,
  expected_image_src TEXT NOT NULL DEFAULT '',
  should_be_empty INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY (equipment_set_id, slot_number)
);

INSERT OR IGNORE INTO equipment_set_slots_new (
  equipment_set_id,
  slot_number,
  expected_image_src,
  should_be_empty
)
SELECT map.new_id,
       slots.slot_number,
       slots.expected_image_src,
       slots.should_be_empty
FROM equipment_set_slots slots
JOIN equipment_set_id_map map ON map.old_id = slots.equipment_set_id
ORDER BY slots.equipment_set_id;

DROP TABLE equipment_set_slots;
DROP TABLE equipment_sets;
ALTER TABLE equipment_sets_new RENAME TO equipment_sets;
ALTER TABLE equipment_set_slots_new RENAME TO equipment_set_slots;
DROP TABLE equipment_set_id_map;

CREATE TABLE technique_presets_new (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  account_id INTEGER NULL REFERENCES accounts(id) ON DELETE CASCADE,
  kind TEXT NOT NULL CHECK (kind IN ('farm', 'combat')),
  created_utc TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_utc TEXT NULL,
  UNIQUE(account_id, kind)
);

CREATE TEMP TABLE technique_preset_id_map AS
SELECT source.id AS old_id,
       target.id AS new_id
FROM technique_presets source
JOIN (
  SELECT MIN(id) AS id,
         account_id,
         kind
  FROM technique_presets
  GROUP BY account_id, kind
) target ON source.kind = target.kind
  AND (
    source.account_id = target.account_id
    OR (source.account_id IS NULL AND target.account_id IS NULL)
  );

INSERT INTO technique_presets_new (
  id,
  account_id,
  kind,
  created_utc,
  updated_utc
)
SELECT id,
       account_id,
       kind,
       created_utc,
       updated_utc
FROM technique_presets
WHERE id IN (SELECT new_id FROM technique_preset_id_map);

CREATE TABLE technique_preset_slots_new (
  technique_preset_id INTEGER NOT NULL REFERENCES technique_presets_new(id) ON DELETE CASCADE,
  technique_number INTEGER NOT NULL CHECK (technique_number BETWEEN 1 AND 15),
  technique_name TEXT NOT NULL DEFAULT '',
  is_enabled INTEGER NOT NULL,
  PRIMARY KEY (technique_preset_id, technique_number)
);

INSERT OR IGNORE INTO technique_preset_slots_new (
  technique_preset_id,
  technique_number,
  technique_name,
  is_enabled
)
SELECT map.new_id,
       slots.technique_number,
       slots.technique_name,
       slots.is_enabled
FROM technique_preset_slots slots
JOIN technique_preset_id_map map ON map.old_id = slots.technique_preset_id
ORDER BY slots.technique_preset_id;

DROP TABLE technique_preset_slots;
DROP TABLE technique_presets;
ALTER TABLE technique_presets_new RENAME TO technique_presets;
ALTER TABLE technique_preset_slots_new RENAME TO technique_preset_slots;
DROP TABLE technique_preset_id_map;

CREATE INDEX idx_equipment_sets_account_kind
ON equipment_sets(account_id, kind);

CREATE INDEX idx_technique_presets_account_kind
ON technique_presets(account_id, kind);

PRAGMA foreign_keys = ON;
