-- Migration: Add CreatedAt column to OtpRequests table
-- Date: 2026-04-18

USE [POS36DB];
GO

-- Check if column exists before adding
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[OtpRequests]') 
    AND name = 'CreatedAt'
)
BEGIN
    ALTER TABLE [dbo].[OtpRequests]
    ADD [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE();
    
    PRINT 'Column CreatedAt added successfully to OtpRequests table';
END
ELSE
BEGIN
    PRINT 'Column CreatedAt already exists in OtpRequests table';
END
GO
