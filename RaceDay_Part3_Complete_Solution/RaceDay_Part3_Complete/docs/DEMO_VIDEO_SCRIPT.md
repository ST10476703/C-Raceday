# RaceDay Part 3 — Video demonstration outline

The rubric allocates **15 marks** to an engaging, clear presentation. Record an actual screen capture, not a static slide presentation.

## Suggested 5-minute walkthrough

**0:00–0:30 — Introduction.** Show the RaceDay homepage and identify it as the Programming 2B Part 3 ASP.NET Core MVC website consuming the Part 2 Web API. Explain the MVC → HTTP API → SQL Server architecture briefly.

**0:30–1:15 — Authentication.** Show register and login, distinct Organiser and Participant menus, session-based user ID/role tracking, and protected-page redirect / logout.

**1:15–2:15 — Organiser experience.** Show dashboard summary, create/edit event, categories, enrolled participants, and record a finishing time/position.

**2:15–3:00 — Participant experience.** Browse/filter events, view event detail, select category, enter, check My Events status and My Results.

**3:00–3:45 — Azure integration.** Show a real banner/profile image being sent from the MVC form to the API, image uploaded to Azure Blob, URL saved in the SQL Server row and image displayed. Show the API upload controller briefly. If Azure wasn't configured, do not claim this worked.

**3:45–4:30 — Docker.** Show the Dockerfile, `docker build` and `docker run` commands from README, and the website running at `http://localhost:8080`. If it hasn't run successfully, do not fake the demonstration.

**4:30–5:00 — Source, test, CI and conclusion.** Show GitHub meaningful commits, passing tests, Part 3 green workflow, README, and project structure. Add the unlisted YouTube URL to the README.

## Evidence checklist

- [ ] Video recorded with narration and code explanation
- [ ] Screens show genuine login and role navigation
- [ ] Organiser and Participant workflows visibly function
- [ ] Azure image upload genuinely succeeds
- [ ] Docker container actually runs the MVC site
- [ ] Part 3 GitHub Actions run is green
- [ ] Unlisted YouTube link inserted in README and ARC submission
