-- =====================================================================
--  portfolio / sql/seed.sql   --  YOUR REAL DATA GOES HERE
--  Nothing below is pre-filled with fake data: every INSERT is commented out.
--  Run schema.sql FIRST, then fill in and run this file in Supabase > SQL Editor.
--
--  HOW TO USE
--   1. Copy a commented block, remove the leading "-- ", replace every <ANGLE BRACKET>.
--   2. Insert in this order: COURSES, then PROJECTS, then IMAGES (parents before children).
--   3. Never type the *_id numbers: they are generated. Look parents up by course_code / title.
--   4. Text values use single quotes. If your text contains an apostrophe, double it:  'Bianca''s app'
-- =====================================================================

SET search_path TO portfolio, public;
BEGIN;                                   -- all-or-nothing: if any line fails, nothing is saved

-- ---------------------------------------------------------------------
-- 1) COURSES  (one INSERT per subject)
--    course_code   : UNIQUE, CAPITAL letters then digits, 2-8 letters + 1-4 digits (DBMWEB1, ITP101, CS2)
--                    no spaces or dashes. Two courses can never share a code (uq_course_code).
--    course_title  : up to 120 characters
--    description   : up to 500 characters, or NULL
--    units         : whole number 1..6   (chk_course_units)
--    academic_term : up to 30 characters, or NULL  e.g. '1st Sem 2026-2027'
--    image_url     : PUBLIC https URL of a cover image, or NULL
-- ---------------------------------------------------------------------
-- INSERT INTO portfolio.course (course_code, course_title, description, units, academic_term, image_url)
-- VALUES ('<COURSECODE1>', '<Course title>', '<What the course was about, or NULL>', 3, '<1st Sem 2026-2027>',
--         'https://<your-project>.supabase.co/storage/v1/object/public/<bucket>/<course-cover>.png');
--
-- INSERT INTO portfolio.course (course_code, course_title, description, units, academic_term, image_url)
-- VALUES ('<COURSECODE2>', '<Course title>', NULL, 3, NULL, NULL);

-- ---------------------------------------------------------------------
-- 2) PROJECTS  (course_id is looked up from the course code, never typed by hand)
--    title          : up to 120 chars; unique inside one course (uq_project_course_title)
--    description    : up to 1000 chars, or NULL
--    tech_stack     : up to 200 chars, or NULL   e.g. 'ASP.NET Core MVC, PostgreSQL'
--    completed_year : 2000..2100
--    project_url    : PUBLIC http(s) link to the live demo or GitHub repo, or NULL
--    video_url      : YouTube link, or NULL. Accepted shapes (exactly 11-character video id):
--                       https://www.youtube.com/watch?v=XXXXXXXXXXX
--                       https://youtu.be/XXXXXXXXXXX
--                       https://www.youtube.com/shorts/XXXXXXXXXXX
-- ---------------------------------------------------------------------
-- INSERT INTO portfolio.project (course_id, title, description, tech_stack, completed_year, project_url, video_url)
-- VALUES ((SELECT course_id FROM portfolio.course WHERE course_code = '<COURSECODE1>'),
--         '<Project title>', '<What it does>', '<Tech used>', 2026,
--         'https://github.com/<you>/<repo>',
--         'https://www.youtube.com/watch?v=<11-char-id>');

-- ---------------------------------------------------------------------
-- 3) PROJECT IMAGES  (one row per image; URL must be PUBLIC and start with http:// or https://)
--    * Do NOT repeat the same URL inside one project: uq_project_image_url refuses it (23505).
--    * sort_order 1 is the cover image shown on the project card.
--    * The sub-SELECT finds the project by course code + project title.
-- ---------------------------------------------------------------------
-- INSERT INTO portfolio.project_image (project_id, image_url, sort_order)
-- VALUES
--   ((SELECT p.project_id FROM portfolio.project p JOIN portfolio.course c ON c.course_id = p.course_id
--      WHERE c.course_code = '<COURSECODE1>' AND p.title = '<Project title>'),
--    'https://<your-project>.supabase.co/storage/v1/object/public/<bucket>/shot-1.png', 1),
--   ((SELECT p.project_id FROM portfolio.project p JOIN portfolio.course c ON c.course_id = p.course_id
--      WHERE c.course_code = '<COURSECODE1>' AND p.title = '<Project title>'),
--    'https://<your-project>.supabase.co/storage/v1/object/public/<bucket>/shot-2.png', 2);

COMMIT;                                  -- if you see an error instead, run ROLLBACK; and fix the line it names
