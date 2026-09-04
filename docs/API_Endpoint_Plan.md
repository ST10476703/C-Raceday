# RaceDay API Endpoint Plan

**Project:** RaceDay Event Management System  
**Part:** Portfolio of Evidence – Part 1  
**Technology:** ASP.NET Core Web API  
**Database:** Microsoft SQL Server  
**Authentication:** Session-based authentication  

---

## 1. Purpose

This document defines the RESTful API endpoints planned for the RaceDay event management system.

The API will provide the backend services required by the RaceDay MVC application. It will handle authentication, user profiles, events, categories, event enrolments, results, route information, weather information and image uploads.

Role-based access will be enforced at API level.

---

## 2. User Roles

### Public

Users who have not authenticated. Public users can browse available events and view public event information.

### Participant

An authenticated user who can browse events, enrol in events, view their own enrolments and track their personal results.

### Organiser

An authenticated user responsible for creating and managing events, categories, enrolments and participant results.

---

# 3. Authentication Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Creates a new RaceDay account. | Public | FirstName, LastName, Email, Password, Phone, Role | 201 Created |
| POST | `/api/auth/login` | Authenticates a user and creates an authenticated session. | Public | Email, Password | 200 OK |
| POST | `/api/auth/logout` | Ends the current authenticated session. | Authenticated | None | 200 OK |

Passwords must be securely hashed before being stored and must never be stored as plain text.

---

# 4. User Profile Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/users/me` | Retrieves the currently authenticated user's profile. | Authenticated | None | 200 OK |
| PUT | `/api/users/me` | Updates the currently authenticated user's profile. | Authenticated | FirstName, LastName, Email, Phone | 200 OK |
| POST | `/api/users/me/profile-image` | Uploads or replaces the user's profile image. | Authenticated | Image file | 200 OK |

Users may only view or update their own profile.

---

# 5. Event Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events` | Retrieves available/upcoming events. | Public | None | 200 OK |
| GET | `/api/events/{id}` | Retrieves details for a specific event. | Public | None | 200 OK |
| POST | `/api/events` | Creates a new event. | Organiser | Name, Description, EventDate, Location, Distance, EventType | 201 Created |
| PUT | `/api/events/{id}` | Updates an event owned by the Organiser. | Organiser | Event fields | 200 OK |
| DELETE | `/api/events/{id}` | Deletes an event owned by the Organiser. | Organiser | None | 204 No Content |
| POST | `/api/events/{id}/banner` | Uploads or replaces an event banner image. | Organiser | Image file | 200 OK |

Each event must contain:

- Name
- Description
- Date
- Location
- Distance
- Event Type

Supported event types are:

- Run
- Walk
- Cycle

---

# 6. Category Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{eventId}/categories` | Retrieves categories available for an event. | Public | None | 200 OK |
| POST | `/api/events/{eventId}/categories` | Creates a category for an Organiser's event. | Organiser | Name, CategoryType, MinimumAge, MaximumAge, Distance | 201 Created |
| PUT | `/api/categories/{id}` | Updates an existing category. | Organiser | Category fields | 200 OK |
| DELETE | `/api/categories/{id}` | Deletes a category. | Organiser | None | 204 No Content |

Categories may represent age groups or distance categories.

Examples include:

- Under 20
- Senior
- Veteran
- 10km
- 21km
- 42km

---

# 7. Event Enrolment Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/enrolments` | Enrols the authenticated Participant into an event and selected category. | Participant | EventId, CategoryId | 201 Created |
| GET | `/api/enrolments/my` | Retrieves all events the Participant has entered. | Participant | None | 200 OK |
| GET | `/api/events/{eventId}/enrolments` | Retrieves all enrolments for an Organiser's event. | Organiser | None | 200 OK |

The API must verify that the selected category belongs to the selected event.

A Participant must not be able to create duplicate enrolments for the same event.

---

# 8. Results Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/results` | Records a Participant's result after an event. | Organiser | EnrolmentId, FinishTime, FinishPosition | 201 Created |
| PUT | `/api/results/{id}` | Updates an existing result. | Organiser | FinishTime, FinishPosition | 200 OK |
| GET | `/api/results/my` | Retrieves the authenticated Participant's personal race history. | Participant | None | 200 OK |
| GET | `/api/events/{eventId}/results` | Retrieves results for an Organiser's event. | Organiser | None | 200 OK |

Results must be associated with a valid event enrolment.

Results contain:

- Finish time
- Finishing position
- Recorded date

---

# 9. Route Information Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{eventId}/route` | Retrieves route information for an event. | Public | None | 200 OK |
| PUT | `/api/events/{eventId}/route` | Creates or updates route information for an Organiser's event. | Organiser | RouteDescription, Distance, ElevationGain, RouteMapUrl | 200 OK |

Route information can include:

- Route description
- Distance
- Elevation gain
- Route map URL

---

# 10. Weather Information Endpoints

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{eventId}/weather` | Retrieves weather information associated with an event. | Public | None | 200 OK |
| PUT | `/api/events/{eventId}/weather` | Creates or updates weather information for an Organiser's event. | Organiser | Temperature, WeatherCondition, WindSpeed, Humidity | 200 OK |

Weather information can include:

- Temperature
- Weather condition
- Wind speed
- Humidity
- Retrieval time

---

# 11. HTTP Status Codes

| Status Code | Meaning |
|---|---|
| 200 | Request completed successfully |
| 201 | Resource created successfully |
| 204 | Resource deleted successfully |
| 400 | Invalid request or validation failure |
| 401 | Authentication required or credentials invalid |
| 403 | User authenticated but does not have permission |
| 404 | Requested resource was not found |
| 409 | Request conflicts with existing data |
| 500 | Unexpected server error |

---

# 12. Role-Based Access Summary

| Function | Public | Participant | Organiser |
|---|:---:|:---:|:---:|
| Register | Yes | — | — |
| Login | Yes | — | — |
| Logout | — | Yes | Yes |
| View own profile | — | Yes | Yes |
| Update own profile | — | Yes | Yes |
| Upload profile image | — | Yes | Yes |
| Browse events | Yes | Yes | Yes |
| View event details | Yes | Yes | Yes |
| Create events | — | — | Yes |
| Update events | — | — | Yes |
| Delete events | — | — | Yes |
| Upload event banner | — | — | Yes |
| View categories | Yes | Yes | Yes |
| Manage categories | — | — | Yes |
| Enrol in events | — | Yes | — |
| View own enrolments | — | Yes | — |
| View event enrolments | — | — | Yes |
| Capture results | — | — | Yes |
| Update results | — | — | Yes |
| View own results | — | Yes | — |
| View event results | — | — | Yes |
| View route information | Yes | Yes | Yes |
| Manage route information | — | — | Yes |
| View weather information | Yes | Yes | Yes |
| Manage weather information | — | — | Yes |

---

# 13. Security Requirements

The API must enforce authorisation independently of the MVC application.

The API must verify:

1. Whether the user is authenticated.
2. Whether the user's role permits the requested operation.
3. Whether an Organiser owns the event or resource they are attempting to manage.
4. Whether a Participant is accessing their own enrolments and results.
5. Whether submitted data is valid.

Passwords must never be stored in their original form.

---

# 14. API and Database Relationship

The API will communicate with the SQL Server database through Entity Framework Core.

The main relationships are:


Users
  |
  +---- Organiser ----> Events
  |
  +---- Participant --> Enrolments
                           |
                           +--> Categories
                           |
                           +--> Results

Events
  |
  +--> Categories
  |
  +--> Enrolments
  |
  +--> Routes
  |
  +--> Weather Information
