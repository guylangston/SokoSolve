USE master
IF EXISTS(select * from sys.databases where name='SokoDB')
BEGIN
	EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'SokoDB'

	USE [master]

	ALTER DATABASE [SokoDB] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE

	USE [master]

	/****** Object:  Database [SokoDB]    Script Date: 03/05/2014 19:01:08 ******/
	DROP DATABASE [SokoDB]
END


CREATE DATABASE SokoDB
GO

USE SokoDB
GO


CREATE TABLE Puzzle(
	PuzzleId int PRIMARY KEY IDENTITY(1,1),
	Name nvarchar(200),
	[Hash] int NOT NULL,
	Rating int NOT NULL,
	[CharMap] varchar(1500) NOT NULL,

	[SourceIdent] nvarchar(500),

	Author nvarchar(200),
	URL nvarchar(200),
	Email nvarchar(200),

	Created datetime,
	Modified datetime,
)

CREATE TABLE Solution(
	SolutionId int PRIMARY KEY IDENTITY(1,1),
	PuzzleREF int FOREIGN KEY REFERENCES Puzzle (PuzzleId) NOT NULL,
	[CharPath] varchar(1500) NOT NULL,

	SolverType nvarchar(200),
	SolverVersionMajor int,
	SolverVersionMinor int,
	SolverDescription nvarchar(500),

	HostMachine nvarchar(32),

	
	TotalNodes int NOT NULL DEFAULT 0,
	TotalSecs decimal(10,4) NOT NULL DEFAULT 0,

	Description nvarchar(500),
	Report nvarchar(500),


	Author nvarchar(200),
	URL nvarchar(200),
	Email nvarchar(200),

	Created datetime,
	Modified datetime,

)