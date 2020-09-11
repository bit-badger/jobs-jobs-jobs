CREATE SCHEMA jjj;
COMMENT ON SCHEMA jjj IS 'Jobs, Jobs, Jobs';

CREATE TABLE jjj.citizen (
  id           VARCHAR(12)   NOT NULL,
  na_user      VARCHAR(50)   NOT NULL,
  display_name VARCHAR(255)  NOT NULL,
  profile_url  VARCHAR(1024) NOT NULL,
  joined_on    BIGINT        NOT NULL,
  last_seen_on BIGINT        NOT NULL,
  CONSTRAINT pk_citizen PRIMARY KEY (id),
  CONSTRAINT uk_na_user UNIQUE (na_user)
);
COMMENT ON TABLE jjj.citizen IS 'Users';
COMMENT ON COLUMN jjj.citizen.id
  IS 'A unique identifier for a user';
COMMENT ON COLUMN jjj.citizen.na_user
  IS 'The ID of this user from No Agenda Social';
COMMENT ON COLUMN jjj.citizen.display_name
  IS 'The display name of the user as it appeared on their profile the last time they logged on';
COMMENT ON COLUMN jjj.citizen.profile_url
  IS 'The URL for the No Agenda Social profile for this user';
COMMENT ON COLUMN jjj.citizen.joined_on
  IS 'When this user joined Jobs, Jobs, Jobs';
COMMENT ON COLUMN jjj.citizen.last_seen_on
  IS 'When this user last logged on to Jobs, Jobs, Jobs';

CREATE TABLE jjj.continent (
  id   VARCHAR(12)  NOT NULL,
  name VARCHAR(255) NOT NULL,
  CONSTRAINT pk_continent PRIMARY KEY (id)
);
COMMENT ON TABLE jjj.continent IS 'Continents';
COMMENT ON COLUMN jjj.continent.id
  IS 'A unique identifier for the continent';
COMMENT ON COLUMN jjj.continent.name
  IS 'The name of the continent';

CREATE TABLE jjj.profile (
  citizen_id         VARCHAR(12)  NOT NULL,
  seeking_employment BOOLEAN      NOT NULL,
  is_public          BOOLEAN      NOT NULL,
  continent_id       VARCHAR(12)  NOT NULL,
  region             VARCHAR(255) NOT NULL,
  remote_work        BOOLEAN      NOT NULL,
  full_time          BOOLEAN      NOT NULL,
  biography          TEXT         NOT NULL,
  last_updated_on    BIGINT       NOT NULL,
  experience         TEXT,
  CONSTRAINT pk_profile           PRIMARY KEY (citizen_id),
  CONSTRAINT fk_profile_citizen   FOREIGN KEY (citizen_id)   REFERENCES jjj.citizen   (id),
  CONSTRAINT fk_profile_continent FOREIGN KEY (continent_id) REFERENCES jjj.continent (id)
);
COMMENT ON TABLE jjj.profile IS 'Employment Profiles';
COMMENT ON COLUMN jjj.profile.citizen_id
  IS 'The ID of the user to whom this profile belongs';
COMMENT ON COLUMN jjj.profile.seeking_employment
  IS 'Whether this user is actively seeking employment';
COMMENT ON COLUMN jjj.profile.is_public
  IS 'Whether this profile should appear on the anonymized public job seeker list';
COMMENT ON COLUMN jjj.profile.continent_id
  IS 'The ID of the continent on which this user is located';
COMMENT ON COLUMN jjj.profile.region
  IS 'The region within the continent where this user is located';
COMMENT ON COLUMN jjj.profile.remote_work
  IS 'Whether this user is open to remote work opportunities';
COMMENT ON COLUMN jjj.profile.full_time
  IS 'Whether this user is looking for full time work';
COMMENT ON COLUMN jjj.profile.biography
  IS 'The professional biography for this user (Markdown)';
COMMENT ON COLUMN jjj.profile.last_updated_on
  IS 'When this profile was last updated';
COMMENT ON COLUMN jjj.profile.experience
  IS 'The prior employment experience for this user (Markdown)';

CREATE INDEX idx_profile_continent ON jjj.profile (continent_id);
COMMENT ON INDEX jjj.idx_profile_continent IS 'FK Index';

CREATE TABLE jjj.skill (
  id         VARCHAR(12)  NOT NULL,
  citizen_id VARCHAR(12)  NOT NULL,
  skill      VARCHAR(100) NOT NULL,
  notes      VARCHAR(100),
  CONSTRAINT pk_skill         PRIMARY KEY (id),
  CONSTRAINT fk_skill_citizen FOREIGN KEY (citizen_id) REFERENCES jjj.citizen (id)
);
COMMENT ON TABLE jjj.skill IS 'Skills';
COMMENT ON COLUMN jjj.skill.id
  IS 'A unique identifier for each skill entry';
COMMENT ON COLUMN jjj.skill.citizen_id
  IS 'The ID of the user to whom this skill applies';
COMMENT ON COLUMN jjj.skill.skill
  IS 'The skill itself';
COMMENT ON COLUMN jjj.skill.notes
  IS 'Proficiency level, length of experience, etc. in this skill';

CREATE INDEX idx_skill_citizen ON jjj.skill (citizen_id);
COMMENT ON INDEX jjj.idx_skill_citizen IS 'FK index';

CREATE TABLE jjj.success (
  id          VARCHAR(12) NOT NULL,
  citizen_id  VARCHAR(12) NOT NULL,
  recorded_on BIGINT      NOT NULL,
  from_here   BOOLEAN     NOT NULL,
  story       TEXT,
  CONSTRAINT pk_success         PRIMARY KEY (id),
  CONSTRAINT fk_success_citizen FOREIGN KEY (citizen_id) REFERENCES jjj.citizen (id)
);
COMMENT ON TABLE jjj.success IS 'Success stories';
COMMENT ON COLUMN jjj.success.id
  IS 'A unique identifier for each success story';
COMMENT ON COLUMN jjj.success.citizen_id
  IS 'The ID of the user to whom this story belongs';
COMMENT ON COLUMN jjj.success.recorded_on
  IS 'When the user recorded this success story';
COMMENT ON COLUMN jjj.success.from_here
  IS 'Whether the user attributes their employment to their need appearing in Jobs, Jobs, Jobs';
COMMENT ON COLUMN jjj.success.story
  IS 'The story of how employment came about (Markdown)';

CREATE INDEX idx_success_citizen ON jjj.success (citizen_id);
COMMENT ON INDEX jjj.idx_success_citizen IS 'FK index';
