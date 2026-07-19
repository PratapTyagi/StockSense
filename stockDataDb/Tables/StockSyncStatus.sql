CREATE TABLE [dbo].[StockSyncStatus]
(
    [Id] BIGINT IDENTITY(1,1) NOT NULL,
    [Stock_Id] BIGINT NOT NULL,
    [Symbol] NVARCHAR(50) NOT NULL DEFAULT '',
    [LastCandleTimestamp] DATETIME2(6) NULL,
    [LastFundamentalSync] DATETIME2(6) NULL,

    CONSTRAINT [PK_StockSyncStatus] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_StockSyncStatus_Stocks] FOREIGN KEY ([Stock_Id]) REFERENCES [dbo].[Stocks]([Id])
);