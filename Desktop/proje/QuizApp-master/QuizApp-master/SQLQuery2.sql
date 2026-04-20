-- Points sütunu yoksa ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Questions]') AND name = 'Points')
BEGIN
    ALTER TABLE [dbo].[Questions] ADD [Points] INT DEFAULT 10 NOT NULL;
END

-- TimeLimitSecond sütunu yoksa ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Questions]') AND name = 'TimeLimitSecond')
BEGIN
    ALTER TABLE [dbo].[Questions] ADD [TimeLimitSecond] INT DEFAULT 10 NOT NULL;
END