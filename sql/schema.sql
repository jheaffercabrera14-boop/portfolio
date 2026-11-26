-- =====================================================================
--  portfolio / sql/schema.sql   (PostgreSQL on Supabase)
--  Run the WHOLE file once in Supabase > SQL Editor.
--  Tables: course (1) ---< project (1) ---< project_image
--  Every constraint is named: pk_ / fk_ / uq_ / chk_
-- =====================================================================

CREATE SCHEMA IF NOT EXISTS portfolio;
SET search_path TO portfolio, public;

-- To rebuild from scratch (DESTROYS DATA), uncomment these three lines first:
-- DROP TABLE IF EXISTS portfolio.project_image;
-- DROP TABLE IF EXISTS portfolio.project;
-- DROP TABLE IF EXISTS portfolio.course;

-- ---------------------------------------------------------------------
-- COURSE: one row per subject you have taken.
-- ---------------------------------------------------------------------
CREATE TABLE portfolio.course (
    course_id      INTEGER GENERATED ALWAYS AS IDENTITY,
    course_code    VARCHAR(12)  NOT NULL,
    course_title   VARCHAR(120) NOT NULL,
    description    VARCHAR(500),
    units          INTEGER      NOT NULL,
    academic_term  VARCHAR(30),
    image_url      VARCHAR(500),                       -- optional cover image (public URL)

    CONSTRAINT pk_course PRIMARY KEY (course_id),
    CONSTRAINT uq_course_code UNIQUE (course_code),
    CONSTRAINT chk_course_code_format
        CHECK (course_code ~ '^[A-Z]{2,8}[0-9]{1,4}$'),
    CONSTRAINT chk_course_units
        CHECK (units BETWEEN 1 AND 6),
    CONSTRAINT chk_course_image_url
        CHECK (image_url IS NULL
               OR image_url ~ '^https?://[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+(:[0-9]+)?(/[^[:space:]]*)?$')
);

-- ---------------------------------------------------------------------
-- PROJECT: many projects belong to ONE course.
-- Deleting a course that still has projects is REFUSED (23503).
-- ---------------------------------------------------------------------
CREATE TABLE portfolio.project (
    project_id      INTEGER GENERATED ALWAYS AS IDENTITY,
    course_id       INTEGER      NOT NULL,
    title           VARCHAR(120) NOT NULL,
    description     VARCHAR(1000),
    tech_stack      VARCHAR(200),
    completed_year  INTEGER      NOT NULL,
    project_url     VARCHAR(300),                      -- optional live demo / repo link
    video_url       VARCHAR(300),                      -- optional YouTube link

    CONSTRAINT pk_project PRIMARY KEY (project_id),
    CONSTRAINT fk_project_course FOREIGN KEY (course_id)
        REFERENCES portfolio.course (course_id)
        ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT uq_project_course_title UNIQUE (course_id, title),
    CONSTRAINT chk_project_year
        CHECK (completed_year BETWEEN 2000 AND 2100),
    CONSTRAINT chk_project_project_url
        CHECK (project_url IS NULL
               OR project_url ~ '^https?://[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+(:[0-9]+)?(/[^[:space:]]*)?$'),
    CONSTRAINT chk_project_video_url
        CHECK (video_url IS NULL
               OR video_url ~ '^https?://(www\.|m\.)?(youtube\.com/watch\?([^[:space:]]*&)?v=|youtube\.com/shorts/|youtu\.be/)[A-Za-z0-9_-]{11}([^[:space:]]*)?$')
);

CREATE INDEX ix_project_course_id ON portfolio.project (course_id);

-- ---------------------------------------------------------------------
-- PROJECT_IMAGE: many images belong to ONE project (stored as URLs only).
-- Deleting a project deletes its images (CASCADE).
-- The same URL twice in one project is REFUSED (23505) -> transaction rolls back.
-- ---------------------------------------------------------------------
CREATE TABLE portfolio.project_image (
    image_id    INTEGER GENERATED ALWAYS AS IDENTITY,
    project_id  INTEGER      NOT NULL,
    image_url   VARCHAR(500) NOT NULL,
    sort_order  INTEGER      NOT NULL DEFAULT 1,

    CONSTRAINT pk_project_image PRIMARY KEY (image_id),
    CONSTRAINT fk_project_image_project FOREIGN KEY (project_id)
        REFERENCES portfolio.project (project_id)
        ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT uq_project_image_url UNIQUE (project_id, image_url),
    CONSTRAINT chk_image_url_format
        CHECK (image_url ~ '^https?://[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+(:[0-9]+)?(/[^[:space:]]*)?$'),
    CONSTRAINT chk_image_sort_order
        CHECK (sort_order >= 1)
);

CREATE INDEX ix_project_image_project_id ON portfolio.project_image (project_id);
