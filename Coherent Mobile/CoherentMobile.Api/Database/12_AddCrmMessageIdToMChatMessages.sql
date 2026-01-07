-- Add CrmMessageId column to MChatMessages table for deduplication of messages from CRM
-- This ensures messages synced from web backend are not duplicated

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('MChatMessages') AND name = 'CrmMessageId')
BEGIN
    ALTER TABLE MChatMessages ADD CrmMessageId NVARCHAR(50) NULL;
    
    -- Create index for fast lookup by CrmMessageId
    CREATE NONCLUSTERED INDEX IX_MChatMessages_CrmMessageId 
    ON MChatMessages(CrmMessageId) 
    WHERE CrmMessageId IS NOT NULL;
    
    PRINT 'Added CrmMessageId column and index to MChatMessages table';
END
ELSE
BEGIN
    PRINT 'CrmMessageId column already exists in MChatMessages table';
END
GO
