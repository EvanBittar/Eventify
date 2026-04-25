-- USE EventManagement
-- GO
-- CREATE TABLE Event.Users(
--     UserId INT PRIMARY KEY IDENTITY(1,1),
--     FirstName NVARCHAR(50) NOT NULL,
--     LastName NVARCHAR(50) NOT NULL,
--     Email NVARCHAR(100) NOT NULL UNIQUE,
--     PasswordHash NVARCHAR(255) NOT NULL,
--     IsAdmin BIT NOT NULL DEFAULT 0,
--     CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
-- )
-- GO
-- CREATE TABLE Event.Categories(
--     CategoryId INT PRIMARY KEY IDENTITY(1,1),
--     NameCategory NVARCHAR(100) NOT NULL UNIQUE
-- )
-- GO
-- CREATE TABLE Event.Events(
--     EventId INT PRIMARY KEY IDENTITY(1,1),
--     CategoryId INT NOT NULL,
--     MaxAttendees INT NOT NULL CHECK (MaxAttendees > 0),
--     Title NVARCHAR(200) NOT NULL,
--     Description NVARCHAR(MAX) NOT NULL,
--     Location NVARCHAR(200) NOT NULL,
--     StartDate DATETIME NOT NULL,
--     EndDate DATETIME NOT NULL,
--     CreatedBy INT NOT NULL,
--     CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
--     FOREIGN KEY (CategoryId) REFERENCES Event.Categories(CategoryId),
--     FOREIGN KEY (CreatedBy) REFERENCES Event.Users(UserId)
-- )
-- GO
-- CREATE TABLE Event.Bookings(
--     BookingId INT PRIMARY KEY IDENTITY(1,1),
--     EventId INT NOT NULL,
--     UserId INT NOT NULL,
--     BookingDate DATETIME NOT NULL DEFAULT GETDATE(),
--     Status TINYINT NOT NULL DEFAULT 1, -- 1: Confirmed, 2: Cancelled, 3: Attended
--     FOREIGN KEY (EventId) REFERENCES Event.Events(EventId),
--     FOREIGN KEY (UserId) REFERENCES Event.Users(UserId)
-- )
GO
INSERT INTO Event.Users
    (
    FirstName ,
    LastName ,
    Email ,
    PasswordHash ,
    IsAdmin ,
    CreatedAt )
VALUES(
        'Admin', 'User', 'admin@example.com', 'hashed_password', 1, GETDATE()
    )
GO
SELECT * FROM Event.Users
GO
SELECT [EventId],
    [CategoryId],
    [MaxAttendees],
    [Title],
    [Description],
    [Location],
    [StartDate],
    [EndDate],
    [CreatedBy],
    [CreatedAt]
FROM Event.Events
GO
SELECT [CategoryId],
       [NameCategory]
FROM Event.Categories
GO
SELECT e.*,c.NameCategory FROM Event.Events AS e JOIN Event.Categories AS c ON e.CategoryId = c.CategoryId
GO
INSERT INTO Event.Categories (NameCategory) 
VALUES ('Technology'), ('Music'), ('Sports'), ('Education');
GO
ALTER TABLE Event.Bookings 
ADD CONSTRAINT UQ_User_Event UNIQUE (UserId, EventId);
GO
-- INSERT INTO Event.Bookings (EventId, UserId, BookingDate, Status)
-- VALUES (1, 1, GETDATE(), 1);
-- GO
-- SELECT * FROM Event.Users
-- SELECT * FROM Event.Bookings
-- SELECT * FROM Event.Events
-- SELECT TOP (@TopN) 
--             e.Title, 
--             COUNT(b.BookingId) AS BookingCount
--         FROM Event.Events AS e
--         LEFT JOIN Event.Bookings AS b ON e.EventId = b.EventId
--         GROUP BY e.EventId, e.Title
--         ORDER BY BookingCount DESC
SELECT 
    EventId, 
    Title, 
    MaxAttendees, 
    (SELECT COUNT(*) FROM Event.Bookings WHERE EventId = e.EventId) AS CurrentBookings,
    CASE WHEN StartDate > GETDATE() THEN 'Future' ELSE 'Past' END AS TimeStatus
FROM Event.Events e;
UPDATE Event.Events 
SET StartDate = DATEADD(month, 1, GETDATE()),
    EndDate = DATEADD(month, 1, DATEADD(day, 1, GETDATE())); 
GO
CREATE TABLE Event.Reviews (
    ReviewId INT PRIMARY KEY IDENTITY(1,1),
    EventId INT NOT NULL,
    UserId INT NOT NULL,
    Rating TINYINT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Comment NVARCHAR(255),
    ReviewDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EventId) REFERENCES Event.Events(EventId),
    FOREIGN KEY (UserId) REFERENCES Event.Users(UserId)
);
GO

-- IF EXISTS (SELECT 1 FROM Event.bookings WHERE EventId = @EventId AND UserId = @UserId)
-- BEGIN
--     INSERT INTO Event.Reviews (EventId, UserId, Rating, Comment)
--     VALUES (@EventId, @UserId, @Rating, @Comment);
--     SELECT 'Success';
-- END
-- ELSE 
-- BEGIN
--     SELECT 'You must book the event before reviewing it';
-- END
-- GO
ALTER TABLE Event.Events
ADD Price DECIMAL(10, 2) DEFAULT 0.00;
GO
-- SELECT 
--     e.EventId, 
--     e.Title, 
--     e.StartDate, 
--     e.Location,
--     e.Price,
--     ISNULL(AVG(CAST(r.Rating AS DECIMAL(10,2))), 0) AS AverageRating,
--     COUNT(r.ReviewId) AS ReviewsCount
-- FROM Event.Events AS e
-- LEFT JOIN Event.Reviews AS r ON e.EventId = r.EventId
-- GROUP BY 
--     e.EventId, e.Title, e.StartDate, e.Location, e.Price;
ALTER TABLE Event.Users
ADD VerificationCode NVARCHAR(6) NULL ,
    CodeExpiration DATETIME NULL,
    IsVerified BIT NOT NULL DEFAULT 0,
    ResetCode NVARCHAR(6) NULL;
GO
ALTER TABLE Event.Users
ADD Roles NVARCHAR(50) NOT NULL DEFAULT 'User';
GO
ALTER TABLE Event.Events
ADD IsDeleted BIT NOT NULL DEFAULT 0;