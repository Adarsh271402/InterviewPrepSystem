-- ================================================================
--  InterviewPrepDB - Complete Database Setup Script
--  Run this in SQL Server Management Studio (SSMS)
-- ================================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'InterviewPrepDB')
    CREATE DATABASE InterviewPrepDB;
GO

USE InterviewPrepDB;
GO

-- ----------------------------------------------------------------
--  TABLE: Users
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
CREATE TABLE Users (
    Id           INT PRIMARY KEY IDENTITY(1,1),
    Name         NVARCHAR(100)  NOT NULL,
    Email        NVARCHAR(100)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX)  NOT NULL,
    Role         NVARCHAR(50)   NOT NULL DEFAULT 'User',
    CreatedDate  DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

-- ----------------------------------------------------------------
--  TABLE: Categories
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' AND xtype='U')
CREATE TABLE Categories (
    Id   INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL
);
GO

-- ----------------------------------------------------------------
--  TABLE: Questions
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Questions' AND xtype='U')
CREATE TABLE Questions (
    Id              INT PRIMARY KEY IDENTITY(1,1),
    CategoryId      INT           NOT NULL,
    QuestionText    NVARCHAR(MAX) NOT NULL,
    DifficultyLevel NVARCHAR(50)  NOT NULL DEFAULT 'Medium',
    CorrectAnswer   NVARCHAR(MAX) NOT NULL DEFAULT '',
    IsAIGenerated   BIT           NOT NULL DEFAULT 0,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE
);
GO

-- ----------------------------------------------------------------
--  TABLE: MockTests
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MockTests' AND xtype='U')
CREATE TABLE MockTests (
    Id             INT PRIMARY KEY IDENTITY(1,1),
    UserId         INT          NOT NULL,
    CategoryId     INT          NOT NULL,
    Score          INT          NOT NULL DEFAULT 0,
    TotalQuestions INT          NOT NULL DEFAULT 0,
    Status         NVARCHAR(50) NOT NULL DEFAULT 'InProgress',
    CreatedDate    DATETIME     NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (UserId)     REFERENCES Users(Id)      ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE NO ACTION
);
GO

-- ----------------------------------------------------------------
--  TABLE: UserAnswers
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserAnswers' AND xtype='U')
CREATE TABLE UserAnswers (
    Id             INT PRIMARY KEY IDENTITY(1,1),
    MockTestId     INT           NOT NULL,
    QuestionId     INT           NOT NULL,
    UserAnswerText NVARCHAR(MAX) NOT NULL DEFAULT '',
    AIScore        INT           NOT NULL DEFAULT 0,
    AIFeedback     NVARCHAR(MAX) NOT NULL DEFAULT '',
    AISuggestions  NVARCHAR(MAX) NOT NULL DEFAULT '',
    AnsweredAt     DATETIME      NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (MockTestId)  REFERENCES MockTests(Id)  ON DELETE CASCADE,
    FOREIGN KEY (QuestionId)  REFERENCES Questions(Id)  ON DELETE NO ACTION
);
GO

-- ----------------------------------------------------------------
--  SEED: Categories
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    INSERT INTO Categories (Name) VALUES
        ('Java'),
        ('SQL'),
        ('ASP.NET Core'),
        ('Python'),
        ('HR Interview'),
        ('C#'),
        ('React.js');
END
GO

-- ----------------------------------------------------------------
--  SEED: Sample Questions - ASP.NET Core
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Questions WHERE CategoryId = 3)
BEGIN
    DECLARE @CatId INT = 3; -- ASP.NET Core
    INSERT INTO Questions (CategoryId, QuestionText, DifficultyLevel, CorrectAnswer) VALUES
    (@CatId, 'What is Dependency Injection in ASP.NET Core and why is it important?',
     'Easy',
     'Dependency Injection (DI) is a design pattern where objects receive their dependencies from an external source rather than creating them internally. ASP.NET Core has DI built-in. It promotes loose coupling, testability, and maintainability.'),

    (@CatId, 'Explain the difference between AddSingleton, AddScoped, and AddTransient service lifetimes.',
     'Medium',
     'AddSingleton: one instance for the entire application lifetime. AddScoped: one instance per HTTP request. AddTransient: a new instance every time it is requested. Use Singleton for stateless services, Scoped for per-request data, and Transient for lightweight services.'),

    (@CatId, 'What is Middleware in ASP.NET Core? How does it work?',
     'Medium',
     'Middleware is software assembled into an application pipeline to handle requests and responses. Each component chooses whether to pass the request to the next component. Middleware is configured in Program.cs using app.Use(), app.Run(), or app.Map().'),

    (@CatId, 'What is JWT Authentication? How do you implement it in ASP.NET Core?',
     'Hard',
     'JWT (JSON Web Token) is a compact, self-contained token for securely transmitting information between parties. In ASP.NET Core: install JwtBearer package, configure in Program.cs with AddAuthentication().AddJwtBearer(), generate tokens using JwtSecurityToken with claims, and use [Authorize] attribute on controllers.'),

    (@CatId, 'What is Entity Framework Core and what is the difference between Code First and Database First?',
     'Easy',
     'EF Core is an ORM (Object-Relational Mapper) for .NET. Code First: you define C# model classes and EF generates the database via migrations. Database First: you start with an existing database and scaffold model classes from it. Code First is preferred in modern development.');
END
GO

-- ----------------------------------------------------------------
--  SEED: Sample Questions - SQL
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Questions WHERE CategoryId = 2)
BEGIN
    DECLARE @SqlCatId INT = 2;
    INSERT INTO Questions (CategoryId, QuestionText, DifficultyLevel, CorrectAnswer) VALUES
    (@SqlCatId, 'What is the difference between INNER JOIN and LEFT JOIN?',
     'Easy',
     'INNER JOIN returns only rows where there is a match in both tables. LEFT JOIN returns all rows from the left table and matched rows from the right table; unmatched right-side columns will be NULL.'),

    (@SqlCatId, 'What is a Primary Key vs a Foreign Key?',
     'Easy',
     'A Primary Key uniquely identifies each row in a table and cannot be NULL. A Foreign Key is a column that references the Primary Key of another table, establishing a relationship between the two tables.'),

    (@SqlCatId, 'Explain the difference between WHERE and HAVING clauses.',
     'Medium',
     'WHERE filters rows before grouping (works on individual rows). HAVING filters groups after a GROUP BY operation. You cannot use aggregate functions in WHERE but you can in HAVING.');
END
GO

-- ----------------------------------------------------------------
--  SEED: Admin User (Password: Admin@123)
--  BCrypt hash of "Admin@123"
-- ----------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM Users WHERE Role = 'Admin')
BEGIN
    INSERT INTO Users (Name, Email, PasswordHash, Role)
    VALUES ('Admin', 'admin@interviewprep.com',
            '$2a$11$rSV/WCKC2QLEFKzSGcS7qOEdFkVVa6XShABHbMKNiYVDI4.sUVfHO',
            'Admin');
END
GO

PRINT 'Database setup complete! InterviewPrepDB is ready.';
GO
