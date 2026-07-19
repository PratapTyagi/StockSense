/*
 Post-Deployment Script
 --------------------------------------------------------------------------------------
 This script is executed after the schema changes are applied.
 Use this for seed data, reference data, or any post-migration logic.
 --------------------------------------------------------------------------------------
*/

PRINT N'Running Post-Deployment Script...';
GO

-- Seed WatchListItems with default symbols if table is empty
-- Note: Add seeding logic in future if we've it in future.
-- IF NOT EXISTS (SELECT 1
-- FROM [dbo].[WatchListItems])
-- BEGIN
--     PRINT N'Seeding logic...';
-- INSERT INTO [dbo].[WatchListItems] ([Symbol])
-- VALUES 
--     ('AAPL'), -- Apple Inc.
--     ('MSFT'), -- Microsoft Corporation
--     ('GOOGL'), -- Alphabet Inc.
--     ('AMZN'), -- Amazon.com, Inc.
--     ('TSLA'); -- Tesla, Inc.
-- END
-- GO

PRINT N'Post-Deployment Script completed.';
GO