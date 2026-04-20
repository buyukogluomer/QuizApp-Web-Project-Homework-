IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Username'
)
BEGIN
    ALTER TABLE [dbo].[Users] ADD [Username] NVARCHAR(MAX) DEFAULT '' NOT NULL;
END


-- Role sütunu yoksa ekle
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'Role')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [Role] NVARCHAR(MAX) DEFAULT 'User' NOT NULL;
END

-- Eğer veritabanında sütun adı 'UserId' ise ama kod 'Id' bekliyorsa bu hata çıkar.
-- Users tablosundaki PK sütun adını kontrol etmemiz lazım.
-- Aşağıdaki komut UserId sütununu Id olarak yeniden adlandırır (Eğer ismi UserId ise).
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'UserId')
BEGIN
    EXEC sp_rename 'dbo.Users.UserId', 'Id', 'COLUMN';
END