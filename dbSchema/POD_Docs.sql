CREATE TABLE [dbo].[POD_Docs]
(
	[SessionDate] DATE NOT NULL , 
    [AttachmentID] INT NOT NULL, 
    [DocID] NVARCHAR(10) NULL, 
    [FileContents] VARBINARY(MAX) NULL, 
    [FileExt] NVARCHAR(3) NULL, 
    [Failed] BIT NULL DEFAULT 0, 
    [FailedError] NVARCHAR(100) NULL DEFAULT '', 
    [Uploaded] BIT NULL DEFAULT 0, 
    [UploadedCounter] INT NULL DEFAULT 0, 
    [SAP_FileName] NVARCHAR(50) NULL, 
    [ModDate] DATETIME NULL DEFAULT GETDATE(), 
    [ModUser] NVARCHAR(20) NULL, 
    [Status] SMALLINT NULL DEFAULT 0, 
    [ReviewedBy] NVARCHAR(20) NULL DEFAULT '', 
    PRIMARY KEY ([SessionDate], [AttachmentID])
)
