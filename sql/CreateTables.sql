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



--Таблица Notification
CREATE TABLE notification (
    id UUID PRIMARY KEY,
    userid UUID NOT NULL,
    type TEXT NOT NULL,
    text TEXT NOT NULL,
    scheduledat TIMESTAMP NOT NULL,
    isnotified BOOLEAN NOT NULL DEFAULT false,
    notifiedat TIMESTAMP NULL,
    
    -- Внешний ключ для связи с таблицей todouser
    CONSTRAINT fk_notification_userid 
        FOREIGN KEY (userid) 
        REFERENCES users(id)
        ON DELETE CASCADE
);

-- Опционально: создание индекса для улучшения производительности запросов по userid
CREATE INDEX idx_notification_userid ON notification(userid);

-- Опционально: создание индекса для запросов по scheduledat
CREATE INDEX idx_notification_scheduledat ON notification(scheduledat);

-- Опционально: создание индекса для запросов по isnotified
CREATE INDEX idx_notification_isnotified ON notification(isnotified);