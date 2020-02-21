﻿CREATE TABLE [Staging].[StatementRun]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1, 1),
	[FromDate] DATETIMEOFFSET NOT NULL,
	[ToDate] DATETIMEOFFSET NOT NULL,
	[TransactionCount] INT NOT NULL DEFAULT(0),
	[SourceFileName] NVARCHAR(255) NULL,
	[AccountId] INT NOT NULL FOREIGN KEY REFERENCES [Staging].[Account](Id),
	[Status] NVARCHAR(255) NOT NULL DEFAULT('Unknown')
)
