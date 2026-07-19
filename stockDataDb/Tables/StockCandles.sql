CREATE TABLE [dbo].[StockCandles]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [StockId] BIGINT NOT NULL,
    [Timestamp] DATETIME2(6) NOT NULL,
    [Open] DECIMAL(18,4) NOT NULL,
    [High] DECIMAL(18,4) NOT NULL,
    [Low] DECIMAL(18,4) NOT NULL,
    [Close] DECIMAL(18,4) NOT NULL,
    [Volume] BIGINT NOT NULL,
    [CreatedAt] DATETIME2(6) NOT NULL CONSTRAINT [DF_StockCandles_CreatedAt] DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_StockCandles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StockCandles_Stocks] FOREIGN KEY ([StockId]) REFERENCES [dbo].[Stocks]([Id])
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_StockCandles_StockId_Timestamp]
    ON [dbo].[StockCandles] ([StockId] ASC, [Timestamp] ASC);
GO