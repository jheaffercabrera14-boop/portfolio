# portfolio — ASP.NET Core MVC + PostgreSQL (Supabase)

Courses → Projects → Images + YouTube video. Viewer (public) and Editor (signed-in) sides.
Raw **Npgsql** repositories, explicit **transactions**, **SQLSTATE 23503 / 23505 / 23514** handling.

## Folder structure
```
portfolio/
├─ portfolio.sln
├─ portfolio.csproj              (Npgsql only; UserSecretsId for secrets)
├─ Program.cs                    DI (repositories), cookie auth, error pages
├─ appsettings.json              NO secrets (empty connection string / editor password)
├─ sql/
│  ├─ schema.sql                 tables + named constraints
│  ├─ seed.sql                   YOUR data template (all INSERTs commented out)
│  └─ test_rows_TEMPORARY.sql    optional test rows + cleanup — DELETE before presenting
├─ Models/                       Course, Project, ProjectImage
├─ ViewModels/                   form + page view models (data annotations)
├─ Validation/                   Patterns.cs (regexes), FormRules.cs (Regex.IsMatch)
├─ Data/                         DbExtensions, PgErrors (SQLSTATE → message), ImageSaveException
├─ Repositories/                 CourseRepository, ProjectRepository, ProjectImageRepository
├─ Security/                     EditorCredentialChecker
├─ Controllers/                  Home, Courses, Projects (viewer) · Editor* (editor, [Authorize]) · Account · Error
├─ Views/                        Shared (_Layout, _ProjectCard, _Alerts) + one folder per controller
├─ wwwroot/css/site.css          design
├─ wwwroot/js/site.js            client-side regex check + live filter
└─ docs/                         PART3_Business_Rules_and_ERD.md · ORAL_DEFENSE_NOTES.md
```

## Setup (in order)
1. **GitHub**: create a *public* repo named `portfolio`, put this folder's contents in it.
2. **Supabase**: SQL Editor → paste and run `sql/schema.sql`. (The schema `portfolio` is created for you.)
3. **Secrets (never commit them).** In a terminal inside the project folder:
   ```
   dotnet user-secrets set "ConnectionStrings:Default" "Host=<host>;Port=5432;Database=postgres;Username=<user>;Password=<db-password>;SSL Mode=Require;Trust Server Certificate=true;Search Path=portfolio,public"
   dotnet user-secrets set "Editor:Username" "<choose-a-username>"
   dotnet user-secrets set "Editor:Password" "<choose-a-long-password>"
   ```
   Use the same host/user/password that worked in your Library System, in this key=value format
   (Npgsql does not accept the `postgresql://` URI). Use the direct or *session* pooler (port 5432), not the transaction pooler.
   When hosting online use environment variables instead: `ConnectionStrings__Default`, `Editor__Username`, `Editor__Password`.
4. Optionally edit `Portfolio:OwnerName` and `Portfolio:Tagline` in `appsettings.json` (not secret).
5. Open `portfolio.sln` in Visual Studio → press **F5**. Click **✎ Editor mode**, sign in, add data.

Target framework is `net8.0`; change it in `portfolio.csproj` if your class uses net9.0/net10.0.

---
## YOUR OWN DATA — where it goes

### A) Hosting image URLs (what works, what does not)
* **Supabase Storage (recommended):** Storage → *New bucket* → name it (e.g. `portfolio`) → tick **Public bucket** → upload files → click a file → **Get URL**.
  The URL looks like `https://<project-ref>.supabase.co/storage/v1/object/public/portfolio/shot-1.png`. Paste that.
* **GitHub:** commit images to the repo (e.g. `wwwroot/img/` or an `images/` folder), open the file on GitHub → right-click the image → *Copy image address*, or use  
  `https://raw.githubusercontent.com/<user>/portfolio/main/images/shot-1.png`. The repo must be public.
* **Do NOT use** Google Drive, Facebook, or OneDrive share links: they point to a web page, not an image, so nothing displays.
* Test a URL: paste it in a new browser tab while signed out of everything. If you see only the image, it works.
* Rules the app enforces: must start with `http://` or `https://`, have a host with a dot, no spaces, ≤ 500 characters, and the **same URL cannot appear twice in one project**.

### B) Through `sql/seed.sql`
Open `sql/seed.sql`. It has three commented blocks. Uncomment and fill them **in this order**:
1. `INSERT INTO portfolio.course …` — `course_code` unique, capitals + digits (`DBMWEB1`); `units` 1–6.
2. `INSERT INTO portfolio.project …` — `course_id` is found with `(SELECT course_id … WHERE course_code = '…')`; put your demo/repo link in `project_url` and your YouTube link in `video_url` (`https://www.youtube.com/watch?v=<11 chars>`, `https://youtu.be/<11 chars>` or `…/shorts/<11 chars>`).
3. `INSERT INTO portfolio.project_image …` — one row per image URL; `sort_order` 1 is the card cover.

Then run the file in Supabase SQL Editor. It runs inside `BEGIN … COMMIT`, so a typo saves nothing. Common errors:
`23505` duplicate course code / duplicate image URL · `23514` bad code format, units, year, or URL shape · `23503` course code you referenced does not exist yet.
Apostrophes in text must be doubled: `'Bianca''s app'`.

### C) Through the Editor (live)
* **Add course:** Editor → *+ Course* (code, title, units, optional cover image URL).
* **Add project + images:** Editor → *+ Project* → choose the course, type title/year, paste the project URL and YouTube link, and paste image URLs **one per line** in the big box. Save: project and images are stored together in one transaction.
* **Edit images:** open the project's *Edit*: change the lines in the textarea (all images are replaced in one transaction), or use the list underneath to edit/delete/add a single image.
* **Delete:** a course with projects is refused (23503) — delete its projects first. Deleting a project deletes its images.

### D) Temporary test rows
`sql/test_rows_TEMPORARY.sql` creates a course `TEST1` with one project and image so you can verify the cascade and the three error codes. They are **not your data**. To swap in your own: run the CLEANUP block at the bottom of that file (or delete them in the Editor), then load your seed/Editor data. Skip the file entirely if you prefer.

## Security notes
* Editor pages: `EditorBaseController` carries `[Authorize]`; all editor controllers inherit it, so signed-out visitors are redirected to `/Account/Login`.
* Password and connection string live in user-secrets / environment variables, never in Git (`.gitignore` also excludes `appsettings.Development.json`).
* All SQL uses parameters; all POSTs require an anti-forgery token; only http(s) URLs are accepted (blocks `javascript:` links).
