CREATE TABLE [Staging].[Transaction]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[AccountId] INT NOT NULL FOREIGN KEY REFERENCES [Staging].[Account](Id),
	[TransactionDate] DATETIMEOFFSET NOT NULL,
	[UniqueId] NVARCHAR(100) NOT NULL,
	[TransactionType] NVARCHAR(100) NOT NULL,
	[Payee] NVARCHAR(255) NULL,
	[CardNumber] NVARCHAR(100) NULL,
	[Description] NVARCHAR(500) NOT NULL,
	[Amount] DECIMAL(10, 3) NOT NULL,
    CONSTRAINT [UQ_UniqueId_AccountId] UNIQUE NONCLUSTERED ([UniqueId], [AccountId])
)
