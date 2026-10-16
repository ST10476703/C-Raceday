# RaceDay — Part 2 Implementation Report

## 1. Project Overview
RaceDay is an ASP.NET Core Web API for managing running, walking and cycling events. The system supports Organisers and Participants with role-based access.

## 2. Technologies
- C# and ASP.NET Core Web API
- Microsoft SQL Server
- Entity Framework Core
- JWT authentication
- Swagger/OpenAPI
- xUnit automated testing
- GitHub Actions CI/CD

## 3. Implemented API Functionality

| Feature | Functionality |
|---|---|
| Authentication | User registration, login and JWT authentication |
| Profiles | Retrieve and update authenticated user profiles |
| Events | Create, view, update and delete events |
| Categories | Manage event categories |
| Enrolments | Register participants and prevent duplicate enrolments |
| Results | Record and update race results and retrieve participant results |

## 4. Database Integration
The application uses SQL Server with Entity Framework Core. The database includes Users, Events, Categories, Enrolments, Results, Routes and WeatherInformation tables.

Entity classes were generated from the existing database schema using EF Core scaffolding.

## 5. Security
Passwords are hashed before storage. JWT authentication protects restricted API endpoints. Role-based authorization distinguishes Organiser and Participant access.

Organiser ownership checks restrict modification of events, categories and results.

## 6. Testing
The automated test suite contains 21 tests covering authentication, event management, enrolments, results and HTTP integration.

Testing uses xUnit, EF Core InMemory and ASP.NET Core integration testing infrastructure.

**Final verified test result:** [Insert Test Explorer result]

## 7. Continuous Integration
GitHub Actions is configured to restore dependencies, build the solution and execute automated tests.

**Final CI/CD status:** [Insert latest workflow result]

## 8. Version Control
The Part 2 implementation has been recorded in 20 meaningful Git commits covering project setup, database integration, authentication, business functionality, automated testing and configuration.

## 9. Conclusion
RaceDay Part 2 provides the backend foundation for event management through RESTful API endpoints, persistent SQL Server storage, authenticated user access and automated testing. The API provides the foundation for the MVC frontend in Part 3.
