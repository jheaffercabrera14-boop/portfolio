-- =====================================================================
--  TEMPORARY TEST ROWS  --  NOT YOUR PORTFOLIO DATA.  DELETE THEM BEFORE PRESENTING.
--  Purpose: prove the schema, the cascade, and the three error codes before you load real data.
--  Everything here uses the course code TEST1 so cleanup is one block.
-- =====================================================================
SET search_path TO portfolio, public;

-- A) Create the temporary rows ------------------------------------------------------
INSERT INTO portfolio.course (course_code, course_title, units)
VALUES ('TEST1', 'TEMPORARY test course - delete me', 3);

INSERT INTO portfolio.project (course_id, title, completed_year)
VALUES ((SELECT course_id FROM portfolio.course WHERE course_code = 'TEST1'), 'TEMPORARY test project', 2026);

INSERT INTO portfolio.project_image (project_id, image_url, sort_order)
VALUES ((SELECT project_id FROM portfolio.project WHERE title = 'TEMPORARY test project'),
        'https://example.com/test-image-1.png', 1);

-- B) Run ONE of these at a time; each must FAIL with the code shown ------------------
-- 23503 foreign key  (course still has a project):
--   DELETE FROM portfolio.course WHERE course_code = 'TEST1';
-- 23505 unique       (same image URL twice in one project):
--   INSERT INTO portfolio.project_image (project_id, image_url, sort_order)
--   VALUES ((SELECT project_id FROM portfolio.project WHERE title = 'TEMPORARY test project'),
--           'https://example.com/test-image-1.png', 2);
-- 23514 check        (units outside 1..6):
--   UPDATE portfolio.course SET units = 9 WHERE course_code = 'TEST1';

-- C) CLEANUP -- run this block to remove every temporary row (children first) ------------
-- DELETE FROM portfolio.project WHERE course_id = (SELECT course_id FROM portfolio.course WHERE course_code = 'TEST1');
-- DELETE FROM portfolio.course  WHERE course_code = 'TEST1';
