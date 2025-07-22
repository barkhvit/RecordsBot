CREATE TABLE Users (
    Id UUID PRIMARY KEY,
    TelegramId BIGINT NOT NULL UNIQUE,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100),
    RegistrationDate TIMESTAMP NOT NULL
);

CREATE TABLE Procedures (
    Id UUID PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    Price DECIMAL(10, 2) NOT NULL,
    DurationMinutes INT NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE FreePeriods (
    FreePeriodId UUID PRIMARY KEY,
    Date DATE NOT NULL,
    StartTime TIME NOT NULL,
    FinishTime TIME NOT NULL,
    Duration INT NOT NULL
);

CREATE TABLE Appointments (
    Id UUID PRIMARY KEY,
    DateTime TIMESTAMP NOT NULL,
    IsConfirmed BOOLEAN NOT NULL DEFAULT FALSE,
    TelegramUserId BIGINT NOT NULL,
    ProcedureId UUID NOT NULL,
    FOREIGN KEY (ProcedureId) REFERENCES Procedures(Id)
);

CREATE INDEX idx_users_telegramid ON Users(TelegramId);
CREATE INDEX idx_appointments_telegramuserid ON Appointments(TelegramUserId);
CREATE INDEX idx_freeperiods_date ON FreePeriods(Date);
