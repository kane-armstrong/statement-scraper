CREATE TABLE [Staging].[Account]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
	[Identifier] NVARCHAR(100) NOT NULL,
	[AccountType] TINYINT NOT NULL,
	[CardOrAccountNumber] NVARCHAR(100) NULL
)
