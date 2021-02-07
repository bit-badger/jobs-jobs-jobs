ALTER TABLE jjj.citizen ALTER COLUMN display_name DROP NOT NULL;
ALTER TABLE jjj.citizen ADD COLUMN real_name VARCHAR(255);

COMMENT ON COLUMN jjj.citizen.real_name
  IS 'The real name of the user';

-- This can be run as often as needed
UPDATE jjj.citizen SET display_name = NULL WHERE display_name = na_user;
