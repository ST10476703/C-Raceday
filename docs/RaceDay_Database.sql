/*
    RaceDay Database
    Part 1 - System Planning and Database

    Database: Microsoft SQL Server
*/

IF DB_ID(N'RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

/* =========================================================
   TABLE CREATION
   ========================================================= */

CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    Phone NVARCHAR(20) NULL,
    ProfileImageLink NVARCHAR(500) NULL,
    DateRegistered DATETIME NOT NULL,

    CONSTRAINT DF_Users_DateRegistered
        DEFAULT GETDATE(),

    CONSTRAINT PK_Users
        PRIMARY KEY (UserId),

    CONSTRAINT UQ_Users_Email
        UNIQUE (Email),

    CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO


CREATE TABLE dbo.Events
(
    EventId INT IDENTITY(1,1) NOT NULL,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    EventDate DATE NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Distance DECIMAL(6,2) NOT NULL,
    EventType VARCHAR(10) NOT NULL,
    BannerImageLink NVARCHAR(500) NULL,
    DateCreated DATETIME NOT NULL,

    CONSTRAINT DF_Events_DateCreated
        DEFAULT GETDATE(),

    CONSTRAINT PK_Events
        PRIMARY KEY (EventId),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserId)
        REFERENCES dbo.Users(UserId),

    CONSTRAINT CK_Events_Distance
        CHECK (Distance > 0),

    CONSTRAINT CK_Events_EventType
        CHECK (EventType IN ('Run', 'Walk', 'Cycle'))
);
GO


CREATE TABLE dbo.Categories
(
    CategoryId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    CategoryType VARCHAR(20) NOT NULL,
    MinimumAge INT NULL,
    MaximumAge INT NULL,
    Distance DECIMAL(6,2) NULL,

    CONSTRAINT PK_Categories
        PRIMARY KEY (CategoryId),

    CONSTRAINT FK_Categories_Event
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT UQ_Categories_Event_Name
        UNIQUE (EventId, Name),

    CONSTRAINT UQ_Categories_Category_Event
        UNIQUE (CategoryId, EventId),

    CONSTRAINT CK_Categories_Type
        CHECK (CategoryType IN ('Age', 'Distance')),

    CONSTRAINT CK_Categories_AgeRange
        CHECK
        (
            (MinimumAge IS NULL AND MaximumAge IS NULL)
            OR
            (
                MinimumAge IS NOT NULL
                AND MaximumAge IS NOT NULL
                AND MinimumAge >= 0
                AND MaximumAge >= MinimumAge
            )
        ),

    CONSTRAINT CK_Categories_Distance
        CHECK (Distance IS NULL OR Distance > 0)
);
GO


CREATE TABLE dbo.Enrolments
(
    EnrolmentId INT IDENTITY(1,1) NOT NULL,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATETIME NOT NULL,
    Status VARCHAR(20) NOT NULL,

    CONSTRAINT DF_Enrolments_EnrolmentDate
        DEFAULT GETDATE(),

    CONSTRAINT DF_Enrolments_Status
        DEFAULT 'Pending',

    CONSTRAINT PK_Enrolments
        PRIMARY KEY (EnrolmentId),

    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantId)
        REFERENCES dbo.Users(UserId),

    CONSTRAINT FK_Enrolments_Event
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT FK_Enrolments_Category_Event
        FOREIGN KEY (CategoryId, EventId)
        REFERENCES dbo.Categories(CategoryId, EventId),

    CONSTRAINT UQ_Enrolments_Participant_Event
        UNIQUE (ParticipantId, EventId),

    CONSTRAINT CK_Enrolments_Status
        CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled'))
);
GO


CREATE TABLE dbo.Results
(
    ResultId INT IDENTITY(1,1) NOT NULL,
    EnrolmentId INT NOT NULL,
    FinishTime TIME NULL,
    FinishPosition INT NULL,
    RecordedAt DATETIME NOT NULL,

    CONSTRAINT DF_Results_RecordedAt
        DEFAULT GETDATE(),

    CONSTRAINT PK_Results
        PRIMARY KEY (ResultId),

    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentId)
        REFERENCES dbo.Enrolments(EnrolmentId),

    CONSTRAINT UQ_Results_Enrolment
        UNIQUE (EnrolmentId),

    CONSTRAINT CK_Results_FinishPosition
        CHECK (FinishPosition IS NULL OR FinishPosition > 0)
);
GO


CREATE TABLE dbo.Routes
(
    RouteId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    RouteDescription NVARCHAR(MAX) NOT NULL,
    Distance DECIMAL(6,2) NULL,
    ElevationGain DECIMAL(6,2) NULL,
    RouteMapUrl NVARCHAR(500) NULL,

    CONSTRAINT PK_Routes
        PRIMARY KEY (RouteId),

    CONSTRAINT FK_Routes_Event
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT UQ_Routes_Event
        UNIQUE (EventId),

    CONSTRAINT CK_Routes_Distance
        CHECK (Distance IS NULL OR Distance > 0),

    CONSTRAINT CK_Routes_ElevationGain
        CHECK (ElevationGain IS NULL OR ElevationGain >= 0)
);
GO


CREATE TABLE dbo.WeatherInformation
(
    WeatherId INT IDENTITY(1,1) NOT NULL,
    EventId INT NOT NULL,
    Temperature DECIMAL(5,2) NULL,
    WeatherCondition NVARCHAR(100) NULL,
    WindSpeed DECIMAL(5,2) NULL,
    Humidity INT NULL,
    RetrievedAt DATETIME NOT NULL,

    CONSTRAINT DF_Weather_RetrievedAt
        DEFAULT GETDATE(),

    CONSTRAINT PK_WeatherInformation
        PRIMARY KEY (WeatherId),

    CONSTRAINT FK_WeatherInformation_Event
        FOREIGN KEY (EventId)
        REFERENCES dbo.Events(EventId),

    CONSTRAINT UQ_WeatherInformation_Event
        UNIQUE (EventId),

    CONSTRAINT CK_WeatherInformation_WindSpeed
        CHECK (WindSpeed IS NULL OR WindSpeed >= 0),

    CONSTRAINT CK_WeatherInformation_Humidity
        CHECK (Humidity IS NULL OR Humidity BETWEEN 0 AND 100)
);
GO


/* =========================================================
   SEED DATA
   =========================================================

   The PasswordHash values below are sample hash strings
   for Part 1 database seeding.

   Authentication and production password hashing
   will be implemented in Part 2.
   ========================================================= */

INSERT INTO dbo.Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Role,
    Phone
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo.mokoena@raceday.co.za',
    '$2b$12$RaceDayDemoOrganiserHash001',
    'Organiser',
    '0825550101'
),
(
    'Naledi',
    'Dlamini',
    'naledi.dlamini@raceday.co.za',
    '$2b$12$RaceDayDemoOrganiserHash002',
    'Organiser',
    '0825550102'
),
(
    'Ofentse',
    'Molefe',
    'ofentse.molefe@example.com',
    '$2b$12$RaceDayDemoParticipantHash001',
    'Participant',
    '0835550101'
),
(
    'Lerato',
    'Ndlovu',
    'lerato.ndlovu@example.com',
    '$2b$12$RaceDayDemoParticipantHash002',
    'Participant',
    '0835550102'
);
GO


INSERT INTO dbo.Events
(
    OrganiserId,
    Name,
    Description,
    EventDate,
    Location,
    Distance,
    EventType,
    BannerImageLink
)
VALUES
(
    1,
    'Pretoria City Run',
    'A community road running event through central Pretoria and surrounding suburbs.',
    '2027-02-14',
    'Pretoria, Gauteng',
    21.10,
    'Run',
    'https://raceday.blob.core.windows.net/events/pretoria-city-run.jpg'
),
(
    1,
    'Tshwane Family Walk',
    'A family-friendly community walking event designed for participants of different ages.',
    '2027-03-06',
    'Pretoria National Botanical Garden, Gauteng',
    10.00,
    'Walk',
    'https://raceday.blob.core.windows.net/events/tshwane-family-walk.jpg'
),
(
    2,
    'Hartbeespoort Cycle Challenge',
    'A road cycling challenge around Hartbeespoort with multiple participation categories.',
    '2027-04-18',
    'Hartbeespoort, North West',
    42.00,
    'Cycle',
    'https://raceday.blob.core.windows.net/events/hartbeespoort-cycle.jpg'
);
GO


INSERT INTO dbo.Categories
(
    EventId,
    Name,
    CategoryType,
    MinimumAge,
    MaximumAge,
    Distance
)
VALUES
(
    1,
    'Senior',
    'Age',
    20,
    39,
    NULL
),
(
    1,
    'Veteran',
    'Age',
    40,
    49,
    NULL
),
(
    1,
    '21.1km Open',
    'Distance',
    NULL,
    NULL,
    21.10
),
(
    2,
    'Junior',
    'Age',
    13,
    19,
    NULL
),
(
    2,
    'Senior',
    'Age',
    20,
    59,
    NULL
),
(
    2,
    '10km Open',
    'Distance',
    NULL,
    NULL,
    10.00
),
(
    3,
    'Under 30',
    'Age',
    18,
    29,
    NULL
),
(
    3,
    'Senior Cyclist',
    'Age',
    30,
    49,
    NULL
),
(
    3,
    '42km Open',
    'Distance',
    NULL,
    NULL,
    42.00
);
GO


INSERT INTO dbo.Enrolments
(
    ParticipantId,
    EventId,
    CategoryId,
    Status
)
VALUES
(
    3,
    1,
    1,
    'Confirmed'
),
(
    4,
    1,
    3,
    'Confirmed'
),
(
    3,
    2,
    6,
    'Pending'
),
(
    4,
    3,
    9,
    'Confirmed'
);
GO


INSERT INTO dbo.Results
(
    EnrolmentId,
    FinishTime,
    FinishPosition
)
VALUES
(
    1,
    '01:42:35',
    47
),
(
    2,
    '01:58:12',
    112
);
GO


INSERT INTO dbo.Routes
(
    EventId,
    RouteDescription,
    Distance,
    ElevationGain,
    RouteMapUrl
)
VALUES
(
    1,
    '21.1km road route beginning in central Pretoria and passing through selected city landmarks.',
    21.10,
    185.00,
    'https://raceday.blob.core.windows.net/routes/pretoria-city-run-route.png'
),
(
    2,
    '10km walking route through the Pretoria National Botanical Garden area and surrounding roads.',
    10.00,
    75.00,
    'https://raceday.blob.core.windows.net/routes/tshwane-family-walk-route.png'
),
(
    3,
    '42km cycling route around Hartbeespoort with rolling climbs and open-road sections.',
    42.00,
    620.00,
    'https://raceday.blob.core.windows.net/routes/hartbeespoort-cycle-route.png'
);
GO


INSERT INTO dbo.WeatherInformation
(
    EventId,
    Temperature,
    WeatherCondition,
    WindSpeed,
    Humidity
)
VALUES
(
    1,
    24.50,
    'Partly Cloudy',
    14.20,
    58
),
(
    2,
    26.00,
    'Sunny',
    10.50,
    51
),
(
    3,
    22.00,
    'Clear',
    18.00,
    46
);
GO


/* =========================================================
   VERIFICATION QUERIES
   ========================================================= */

SELECT
    'Users' AS Entity,
    COUNT(*) AS RecordCount
FROM dbo.Users

UNION ALL

SELECT
    'Events',
    COUNT(*)
FROM dbo.Events

UNION ALL

SELECT
    'Categories',
    COUNT(*)
FROM dbo.Categories

UNION ALL

SELECT
    'Enrolments',
    COUNT(*)
FROM dbo.Enrolments

UNION ALL

SELECT
    'Results',
    COUNT(*)
FROM dbo.Results

UNION ALL

SELECT
    'Routes',
    COUNT(*)
FROM dbo.Routes

UNION ALL

SELECT
    'WeatherInformation',
    COUNT(*)
FROM dbo.WeatherInformation;
GO


/* =========================================================
   VERIFY MAIN EVENT / ENROLMENT RELATIONSHIP
   ========================================================= */

SELECT
    e.Name AS EventName,
    u.FirstName + ' ' + u.LastName AS Participant,
    c.Name AS Category,
    en.Status,
    r.FinishTime,
    r.FinishPosition
FROM dbo.Enrolments en

INNER JOIN dbo.Users u
    ON en.ParticipantId = u.UserId

INNER JOIN dbo.Events e
    ON en.EventId = e.EventId

INNER JOIN dbo.Categories c
    ON en.CategoryId = c.CategoryId

LEFT JOIN dbo.Results r
    ON en.EnrolmentId = r.EnrolmentId

ORDER BY
    e.EventDate,
    r.FinishPosition;
GO
