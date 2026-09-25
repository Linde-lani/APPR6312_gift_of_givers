-- CREATING A DATABASE CALLED Gifters_db
CREATE DATABASE gifters_db;

--USE THE DB BEFORE CREATING
use gifters_db;

-- =========================
-- USERS TABLE
-- =========================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,  -- hashed with BCrypt
    Role NVARCHAR(50) NOT NULL,       -- Donor, Volunteer, Supporter
    JoinDate DATETIME DEFAULT GETDATE()
);

-- =========================
-- DONATIONS TABLE
-- =========================
CREATE TABLE Donations (
    DonationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Purpose NVARCHAR(150) NOT NULL,
    Date DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- =========================
-- 3️⃣ EVENTS TABLE
-- =========================
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(150) NOT NULL,
    EventDate DATETIME NOT NULL,
    Location NVARCHAR(150),
    Description NVARCHAR(255)
);

-- =========================
-- 4️⃣ EVENT REGISTRATIONS TABLE
-- =========================
CREATE TABLE EventRegistrations (
    RegistrationId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    EventId INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Registered',
    FOREIGN KEY (UserId) REFERENCES Users(UserId),
    FOREIGN KEY (EventId) REFERENCES Events(EventId)
);

-- =========================
-- 5️⃣ VOLUNTEER ACTIVITIES TABLE
-- =========================
CREATE TABLE VolunteerActivities (
    ActivityId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ActivityName NVARCHAR(150) NOT NULL,
    Date DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Signed Up',
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);


-- =========================
-- 6️⃣ SYSTEM LOGS TABLE (optional)
-- =========================
CREATE TABLE SystemLogs (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(150),
    Timestamp DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);