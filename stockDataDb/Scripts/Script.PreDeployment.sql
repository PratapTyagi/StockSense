/*
 Pre-Deployment Script
 --------------------------------------------------------------------------------------
 This script is executed before the schema changes are applied.
 Use this for any preparatory steps such as:
   - Backing up data before destructive changes
   - Disabling constraints temporarily
   - Any pre-migration logic
 --------------------------------------------------------------------------------------
*/

PRINT N'Running Pre-Deployment Script...';
GO