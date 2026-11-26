# Oral defense cheat-sheet (Part 2 — 20 pts)

## 1. Where each exam criterion lives
| Criterion | File(s) |
|---|---|
| Database design | `sql/schema.sql`, `sql/seed.sql` |
| One repository per table, registered in Program.cs | `Repositories/CourseRepository.cs`, `ProjectRepository.cs`, `ProjectImageRepository.cs`; `Program.cs` (`AddScoped`) |
| No SQL in controllers/views | grep for `SELECT`/`INSERT` in `Controllers/` and `Views/` → zero hits |
| Viewer screens | `HomeController`, `CoursesController`, `ProjectsController` + `Views/Home|Courses|Projects` |
| Editor screens | `EditorController`, `EditorCoursesController`, `EditorProjectsController`, `EditorImagesController` |
| Viewer ⇄ Editor link | `Views/Shared/_Layout.cshtml` (`mode-switch`) |
| Transaction | `ProjectRepository.CreateWithImagesAsync` / `UpdateWithImagesAsync` |
| 3 regexes | `Validation/Patterns.cs` (ImageUrl, VideoUrl, CourseCode), used by `FormRules.Matches` (`Regex.IsMatch`) and `wwwroot/js/site.js` + `pattern=""` attributes |
| 23503 / 23505 / 23514 | `Data/PgErrors.cs`; caught as `PostgresException` in controllers |
| 404 / friendly error page | `ErrorController`, `Views/Error/`, `UseStatusCodePagesWithReExecute` + `UseExceptionHandler` in `Program.cs` |
| Extra credit sign-in | `AccountController`, `EditorBaseController` (`[Authorize]`), `Security/EditorCredentialChecker.cs` |

## 2. The transaction in your own words
"`CreateWithImagesAsync` opens one connection and calls `BeginTransactionAsync` (BEGIN). It inserts the project and gets the new `project_id` back with `RETURNING`. Then it loops over the image lines and inserts each one using the SAME connection and transaction. If the second line repeats the first URL, PostgreSQL raises 23505 on `uq_project_image_url`; my `catch` calls `RollbackAsync` (ROLLBACK), so the project row and the first image are undone, and the controller shows 'Line 2: … Nothing was saved'. Only when every insert succeeds do I call `CommitAsync`."

Live rollback demo: Editor → Add project → paste the SAME image URL on two lines → Save → message appears, and the dashboard shows the project was NOT created. For Edit: repeat a URL → the previously saved images are still there.

## 3. Design decisions (ERD questions)
* **Why a separate image table?** A project has many images: storing them in one column would break first normal form (multivalued attribute → its own table keyed by the owner's key + the value → `UNIQUE (project_id, image_url)`).
* **Why RESTRICT on course→project but CASCADE on project→image?** A course deleted by accident would wipe real work, so the database refuses (23503). Images have no meaning without their project, so they go with it.
* **Why do I check in the browser, the server AND the database?** Browser = speed, server = real security (JS can be off), CHECK constraints = last line of defence for any other client.
* **Why Npgsql and not EF Core?** Every SQL statement, transaction and SQLSTATE is visible and explainable.
* **Why `GENERATED ALWAYS AS IDENTITY`?** Keys are produced by the database; nobody can insert their own id.
* **Why never store files?** Images/videos are public URLs; the DB only stores text.

## 4. Showing 23514 (check) live
The browser and server validate first, so a normal form submit is stopped before the database. To show the database itself refusing, run in Supabase SQL editor (from `sql/test_rows_TEMPORARY.sql`): `UPDATE portfolio.course SET units = 9 WHERE course_code = '<a real code>';` → error 23514 `chk_course_units`. The app's `PgErrors` maps that code to a friendly message if it ever reaches the page.
Showing 23503 live: Editor → Delete a course that still has projects → red message on the page.
Showing 23505 live: add a second course with an existing code → message under the form.

## 5. Before presenting
* Open the Supabase project (free projects pause after 7 days) — "Restore" if paused.
* Run the app, sign in, check Viewer + Editor on your REAL data.
* Make sure you deleted the TEMPORARY test rows (`TEST1`).
