CREATE TABLE [dbo].[WatchListItems]
(
    [Id]     INT            IDENTITY(1,1) NOT NULL,
    [Symbol] NVARCHAR(50)   NOT NULL,

    CONSTRAINT [PK_WatchListItems] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

CREATE NONCLUSTERED INDEX [IX_WatchListItems_Symbol]
    ON [dbo].[WatchListItems] ([Symbol] ASC);
GO