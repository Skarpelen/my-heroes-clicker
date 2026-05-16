ALTER TABLE app_settings
ADD COLUMN alert_sound_enabled INTEGER NOT NULL DEFAULT 1;

ALTER TABLE app_settings
ADD COLUMN alert_sound_volume REAL NOT NULL DEFAULT 0.55;
