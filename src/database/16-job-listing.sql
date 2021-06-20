
-- Add job listing table

CREATE TABLE jjj.job_listing (
  id           VARCHAR(12)  NOT NULL,
  citizen_id   VARCHAR(12)  NOT NULL,
  created_on   TIMESTAMP    NOT NULL,
  title        VARCHAR(100) NOT NULL,
  continent_id VARCHAR(12)  NOT NULL,
  region       VARCHAR(255) NOT NULL,
  remote_work  BOOLEAN      NOT NULL,
  expired      BOOLEAN      NOT NULL,
  updated_on   TIMESTAMP    NOT NULL,
  listing      TEXT         NOT NULL,
  needed_by    DATE,
  filled_here  BOOLEAN,
  CONSTRAINT pk_job_listing       PRIMARY KEY (id),
  CONSTRAINT fk_listing_citizen   FOREIGN KEY (citizen_id)   REFERENCES jjj.citizen   (id),
  CONSTRAINT fk_listing_continent FOREIGN KEY (continent_id) REFERENCES jjj.continent (id)
);

COMMENT ON TABLE jjj.job_listing IS 'Job Listings';
COMMENT ON COLUMN jjj.job_listing.id
  IS 'A unique identifier for a job listing';
COMMENT ON COLUMN jjj.job_listing.created_on
  IS 'The date/time a job listing was created';
COMMENT ON COLUMN jjj.job_listing.title
  IS 'The title of the job listing';
COMMENT ON COLUMN jjj.job_listing.continent_id
  IS 'The ID of the continent on which this job is based';
COMMENT ON COLUMN jjj.job_listing.region
  IS 'The region in which this job is based';
COMMENT ON COLUMN jjj.job_listing.remote_work
  IS 'Whether this job is a remote job';
COMMENT ON COLUMN jjj.job_listing.expired
  IS 'Whether this listing is expired';
COMMENT ON COLUMN jjj.job_listing.updated_on
  IS 'The date/time this job listing was last updated';
COMMENT ON COLUMN jjj.job_listing.listing
  IS 'The text of the job listing';
COMMENT ON COLUMN jjj.job_listing.needed_by
  IS 'The date by which this job needs to be filled';
COMMENT ON COLUMN jjj.job_listing.filled_here
  IS 'Whether this job listing was filled because it appeared here';

CREATE INDEX idx_listing_citizen   ON jjj.job_listing (citizen_id);
CREATE INDEX idx_listing_continent ON jjj.job_listing (continent_id);
COMMENT ON INDEX jjj.idx_listing_citizen   IS 'FK index';
COMMENT ON INDEX jjj.idx_listing_continent IS 'FK index';

-- Add source column to success story

ALTER TABLE jjj.success ADD COLUMN source VARCHAR(7) NOT NULL DEFAULT 'profile';
ALTER TABLE jjj.success ADD CONSTRAINT ck_source CHECK (source IN ('profile', 'listing'));
COMMENT ON COLUMN jjj.success.source
  IS 'The source of the success story (profile or listing)';
