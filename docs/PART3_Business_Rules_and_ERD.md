# PART 3 — Submission sheet (copy these into the exam paper)

**Repository link:** `https://github.com/<your-username>/portfolio`
**Supabase schema name:** `portfolio`

## Business rules (read each relationship twice, once from each end)

| # | ENTITY 1 | may / must | verb | number | ENTITY 2 |
|---|----------|-----------|------|--------|----------|
| 1 | COURSE | may | have | many | PROJECT |
| 2 | PROJECT | must | belong to | one | COURSE |
| 3 | PROJECT | may | have | many | PROJECT_IMAGE |
| 4 | PROJECT_IMAGE | must | belong to | one | PROJECT |

Why these words match the tables exactly:
* Rule 1 "may … many": a course can have zero projects (nothing forces a project row to exist) and many → `project.course_id` is the foreign key on the MANY side.
* Rule 2 "must … one": `project.course_id` is `NOT NULL` (bar = minimum one) and a single column holds one course (bar = maximum one).
* Rule 3 "may … many": a project can have zero images; many images → `project_image.project_id` is the foreign key.
* Rule 4 "must … one": `project_image.project_id` is `NOT NULL`.
* The video is NOT a relationship: it is one nullable column `project.video_url` (a project MAY have one video link).

## ERD (Chen notation with crow's-foot marks)

Draw it as below. Rectangles = entities, ovals = attributes (underline the key), diamonds = relationships.
Marks sit BESIDE the entity they describe: crow's foot = many, bar = only one / at least one, circle = may be none.

```
 (course_id)  (course_code)  (course_title) (description) (units) (academic_term) (image_url)
   ____          \             |              /            /          /            /
      \___________\____________|_____________/____________/__________/____________/
                              [ COURSE ]
                                  |
                      ||          |            (bar|bar  = exactly ONE course, beside COURSE)
                                  |
                              < HAS >              (diamond: course "has" projects)
                                  |
                      -o<         |            (circle + crow's foot = ZERO OR MANY, beside PROJECT)
                                  |
 (project_id) (title) (description) (tech_stack) (completed_year) (project_url) (video_url)
        \        |         |            |             |               |            /
         \_______|_________|____________|_____________|_______________|___________/
                              [ PROJECT ]
                                  |
                      ||          |            (bar|bar  = exactly ONE project, beside PROJECT)
                                  |
                              < HAS >              (diamond: project "has" images)
                                  |
                      -o<         |            (circle + crow's foot = ZERO OR MANY, beside PROJECT_IMAGE)
                                  |
              (image_id) (image_url) (sort_order)
                   \          |          /
                          [ PROJECT_IMAGE ]
```

Attributes to write in the ovals (underlined = primary key):
* **COURSE**: __course_id__, course_code (unique), course_title, description, units, academic_term, image_url
* **PROJECT**: __project_id__, title, description, tech_stack, completed_year, project_url, video_url  (FK course_id)
* **PROJECT_IMAGE**: __image_id__, image_url, sort_order  (FK project_id)

Note on the crow's-foot marks (check the lecture slides and copy their exact drawing of "bar / circle / crow's foot"):
* Beside **PROJECT** (end of the COURSE–PROJECT line): circle + crow's foot → "a course MAY have MANY projects".
* Beside **COURSE** (other end): two bars → "a project MUST belong to ONE course".
* Beside **PROJECT_IMAGE**: circle + crow's foot → "a project MAY have MANY images".
* Beside **PROJECT** (end of the PROJECT–PROJECT_IMAGE line): two bars → "an image MUST belong to ONE project".

## Constraints list (for the ERD → tables discussion)

| Table | PK | FK (delete rule) | UNIQUE | CHECK |
|---|---|---|---|---|
| course | pk_course | – | uq_course_code | chk_course_code_format, chk_course_units, chk_course_image_url |
| project | pk_project | fk_project_course → course (**ON DELETE RESTRICT**) | uq_project_course_title | chk_project_year, chk_project_project_url, chk_project_video_url |
| project_image | pk_project_image | fk_project_image_project → project (**ON DELETE CASCADE**) | uq_project_image_url | chk_image_url_format, chk_image_sort_order |
