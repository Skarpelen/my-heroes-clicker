ALTER TABLE app_settings
ADD COLUMN war_check_interval_minutes INTEGER NOT NULL DEFAULT 15;

ALTER TABLE app_settings
ADD COLUMN war_combat_preparation_seconds_before_registration_end INTEGER NOT NULL DEFAULT 60;
