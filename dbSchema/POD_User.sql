﻿CREATE TABLE [dbo].[POD_User]
(
	[UserName] NVARCHAR(20) NOT NULL PRIMARY KEY, 
    [Password] NVARCHAR(MAX) NULL, 
    [Name] NVARCHAR(50) NULL, 
    [Admin] BIT NULL DEFAULT 0, 
    [Report] BIT NULL DEFAULT 0, 
    [Active] BIT NULL DEFAULT 0, 
    [ModDate] DATETIME NULL DEFAULT GETDATE(), 
    [ModUser] NVARCHAR(20) NULL 
)