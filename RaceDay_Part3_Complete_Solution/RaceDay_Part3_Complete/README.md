# RaceDay — Programming 2B Part 3 PoE

**Module:** PROG6212 · **Solution:** `RaceDayAuthAPI.sln` · **Framework:** .NET 8

RaceDay is a road-running, walking and cycling platform. The Part 3 website is an ASP.NET Core MVC frontend consuming the Part 2 RESTful API over HTTP. **MVC has no Entity Framework or direct SQL Server access.** Organisers and participants have separate session-protected experiences.

## What's included

| Project | Purpose |
|---|---|
| `RaceDayAuthAPI` | Existing Part 2 API, SQL Server/EF Core, JWT, extended with Azure Blob image endpoints and organiser results listing |
| `RaceDayAuthAPI.Tests` | Existing xUnit authentication, events, enrolments, results and HTTP integration tests |
| `RaceDayMVC` | New Part 3 MVC website, session-based role-aware UI, API client, image proxy, sporting theme |
| `.github/workflows/part3-ci.yml` | Builds the API + MVC + tests, and Docker image |

### MVC functionality

- **Unauthenticated:** register, login, browse/filter upcoming events, view details/categories.
- **Participant:** session-protected event entry, My Events, My Results, profile updates and picture upload.
- **Organiser:** dashboard with event counts and enrolment totals, event create/edit/delete, banners, categories, enrolled participants, result publication and editing.
- **Azure Blob Storage:** MVC uploads to **API only**; the API validates and stores the image in Azure, saves the Azure URL to the SQL Server `Events` / `Users` row, and can proxy an image from a private container.
- **Docker:** the MVC app can run in a standalone Linux container while API runs on the Windows host.

## Requirements

- Visual Studio Community with .NET 8 SDK and ASP.NET and Web Development workload.
- SQL Server LocalDB (or your existing SQL Server instance) with the original **RaceDayDB** Part 1 tables already created.
- The Part 2 API's correct `Jwt:Key` configured using **User Secrets**.
- Azure Storage account / connection string to test real banner and profile-picture uploads.
- Docker Desktop for the container assessment.

## 1. Open and launch the solution

1. Extract the ZIP. Open `RaceDayAuthAPI.sln` in Visual Studio.
2. Restore NuGet packages and **Build → Rebuild Solution**. The uploaded archive was flattened from its extra `stuff/` and `testes/` folder layers so paths now match the `.sln`.
3. Verify `RaceDayAuthAPI/appsettings.json` points to your actual SQL Server and existing `RaceDayDB`. No schema migrations are needed.
4. Right-click the **RaceDayAuthAPI** project → **Manage User Secrets** and set a *unique randomly generated* development signing key:
   ```json
   { "Jwt": { "Key": "REPLACE-WITH-A-UNIQUE-RANDOM-32-PLUS-BYTE-SECRET" } }
   ```
   Do not use the placeholder verbatim or commit a real key. `Jwt:Issuer` is `RaceDayApi` and `Jwt:Audience` is `RaceDayClient` in `appsettings.json`.
5. Verify the API is running on **https://localhost:7230** (its extracted launch profile). Swagger should be at `https://localhost:7230/swagger`. If you change the API port, update `RaceDayMVC/appsettings.json` → `Api:BaseUrl`.
6. Solution → **Configure Startup Projects** → Multiple Startup Projects: **Start** both `RaceDayAuthAPI` and `RaceDayMVC`. The MVC launch profile is **https://localhost:7241**.
7. Browse the website at `https://localhost:7241` and create an Organiser and a Participant account using the registration screen.

**Important:** Since Part 2 was built against an existing SQL schema, do not run `Update-Database` or recreate the tables. MVC calls the API; only the API accesses SQL Server.

## 2. Azure Blob Storage setup

The API uses `Azure.Storage.Blobs`. Configure the following secrets **in the API project**, not in MVC:

```json
{
  "AzureStorage": {
    "ConnectionString": "<YOUR-AZURE-STORAGE-CONNECTION-STRING>",
    "Container": "raceday-images"
  }
}
```

Use Visual Studio → right-click `RaceDayAuthAPI` → **Manage User Secrets**. The storage account must permit the connection string to create/use the container. Do **not** include storage credentials in Git or screenshots.

- Organiser uploads a banner via event create/edit → MVC POST multipart to `api/uploads/event/{eventId}` → API uploads to Azure and writes URL into `Events.BannerImageLink`.
- Participant uploads picture via profile edit → MVC POST multipart to `api/uploads/profile` → API uploads to Azure and writes URL into `Users.ProfileImageLink`.
- MVC displays private-container images through `Images/Show`, which calls the API's `GET api/uploads/{blobName}`. MVC **never connects to Azure directly**.
- Files must be genuine JPG/PNG/WebP under 5 MB; the API checks image signatures.
- Upload returns `503` with an explanation when Azure credentials have not been configured. The remaining app still works without Azure.

## 3. Docker — required run command

Docker packages **only the MVC app**, as required. Keep the API running separately in Visual Studio using its **http** profile at `http://localhost:5013`. In Docker Desktop on Windows, `host.docker.internal` resolves the Windows host:

From the **solution root** (`RaceDayAuthAPI.sln` folder):

```powershell
docker build -f RaceDayMVC/Dockerfile -t raceday-mvc:part3 .
docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:5013/ raceday-mvc:part3
```

Open **http://localhost:8080**. If the API is on another port, replace `5013` accordingly. Use the API's HTTP profile for Docker to avoid untrusted Windows development HTTPS certificates in the Linux container. Never store secrets in the MVC image. The API itself is not containerised because it uses your existing local SQL Server instance; it can run on Windows as above.

## 4. Run tests and CI/CD

```powershell
dotnet restore RaceDayAuthAPI.sln
dotnet build RaceDayAuthAPI.sln -c Release
dotnet test RaceDayAuthAPI.Tests/RaceDayAuthAPI.Tests.csproj -c Release
```

GitHub Actions → **RaceDay Part 3 - Build Test and Docker**. It restores dependencies, builds the full solution, runs the existing Part 2 automated tests using EF Core InMemory and test JWT settings, and builds the MVC Docker image. Save a screenshot of a **real green workflow** for submission. This is a workflow definition; its status cannot be confirmed until pushed and run on GitHub.

## 5. Endpoint map

| MVC operation | Part 2 API endpoint |
|---|---|
| Register / Login | `POST api/auth/register`, `POST api/auth/login` |
| Profile read/edit | `GET/PUT api/users/me` |
| Browse/detail/events CRUD | `GET/POST/PUT/DELETE api/events` |
| Categories | `GET api/events/{eventId}/categories`, `POST`, `PUT/DELETE api/categories/{id}` |
| Enter event / my entries / organiser entries | `POST api/events/{id}/enrolments`, `GET api/enrolments/me`, `GET api/events/{id}/enrolments` |
| Results | `GET api/results/me`, `POST api/enrolments/{id}/result`, `PUT api/results/{id}` |
| NEW: organiser leaderboard | `GET api/events/{id}/results` |
| NEW: event banner | `POST api/uploads/event/{id}` |
| NEW: profile photo | `POST api/uploads/profile` |
| NEW: image display | `GET api/uploads/{blobName}` |

## 6. Part 3 evidence / submission

See `docs/PART3_CHECKLIST.md` and `docs/PART3_COMMIT_PLAN.md`.

- GitHub repo: https://github.com/ST10476703/C-Raceday
- **20 meaningful commits for Part 3 are required in addition to Part 2**. This ZIP is source code, not a manufactured Git history. Commit genuine logical features and improvements as you integrate and verify them.
- CI screenshot: [insert link to your green Part 3 Actions run]
- Docker screenshot / command verified: [insert evidence after local Docker test]
- Video presentation (YouTube link): [insert your unlisted YouTube link]
- Submit your GitHub repository URL through ARC, as specified in the Part 3 brief.

## Known verification limitations

The provided archive can be inspected and edited here, but this package has **not** been built on the user's Windows machine or tested against the user's SQL Server/Azure account. Verify its NuGet restore, MVC/API flows, Blob uploads and Docker run before submitting. The existing Part 2 test source has been carried forward and should be re-run after integration. Do not claim CI, Azure or Docker success without real evidence.
