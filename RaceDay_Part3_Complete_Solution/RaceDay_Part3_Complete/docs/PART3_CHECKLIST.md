# Part 3 PoE marking checklist — PROG6212

This checklist is based on the rubric photographs supplied by the student. Tick only after real verification.

- [ ] **Correct submission (5):** structured repository, README, Docker instructions, screenshot, YouTube link, GitHub URL submitted to ARC.
- [ ] **GitHub and CI/CD (10):** 20 meaningful *Part 3* commits, source present, Part 3 workflow green including MVC.
- [ ] **Authentication and role-based navigation (10):** registration/login via API, server session with user ID/role, Organiser and Participant menus, protected pages, logout.
- [ ] **Organiser interface (15):** dashboard event/enrolment/upcoming summaries; event CRUD; event banner upload; categories; enrolment lists; result capture and update.
- [ ] **Participant interface (15):** upcoming event listing and filter, full details and categories, enrolment, My Events statuses, My Results times/positions, profile edit and picture upload.
- [ ] **Azure Blob Storage (10):** image sent from MVC to API, uploaded to Azure, Azure URL saved in SQL Server, image displayed from MVC.
- [ ] **Docker (10):** `docker build` and `docker run` work for MVC, README command documented, container has same functionality as local site.
- [ ] **UI design (10):** branded sporty theme, responsive layout, role navigation, statuses clearly coded, graceful errors.
- [ ] **Video presentation (15):** actual demo of MVC pages, role flows, Blob Storage, Docker and code explanation. Upload video and add URL to README.

## Manual smoke test

1. Launch existing API and MVC together. Visit the event discovery homepage as anonymous visitor.
2. Register Organiser; login; verify session and distinct Organiser navigation.
3. Create/edit event; add/edit category; check public details.
4. Register Participant; login; select event category; verify My Events and enrolment status.
5. Log back in as Organiser; see enrolments and record a result.
6. Log in as Participant; verify My Results and profile edit.
7. Upload event banner and profile photo with Azure configured. Confirm image URL in DB and image displays.
8. Logout and attempt to navigate directly to `/Participant/MyEvents` and `/Organiser/Dashboard` — redirected or denied.
9. Run original xUnit suite and verify green GitHub Actions workflow.
10. Build/run Docker image using README command; capture screenshots, video and commit evidence.

**Do not mark an item complete just because source exists. Confirm the running behaviour.**
