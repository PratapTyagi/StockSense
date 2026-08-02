CREATE TABLE [dbo].[Stocks]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Symbol] NVARCHAR(50) NOT NULL,
    [InstrumentToken] NVARCHAR(255) NOT NULL,
    [Exchange] NVARCHAR(50) NOT NULL,
    [CompanyName] NVARCHAR(255) NOT NULL,
    [IsActive] BIT NOT NULL,

    CONSTRAINT [PK_Stocks] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Stocks_Symbol]
    ON [dbo].[Stocks] ([Symbol] ASC);
GO
