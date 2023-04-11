CREATE TABLE [dbo].[tbl_Details]

(
	[Id] INT NOT NULL PRIMARY KEY, 
    [UID] NVARCHAR(50) NULL, 
    [Student Name] NVARCHAR(MAX) NULL, 
    [Course] NVARCHAR(50) NULL, 
    [img] IMAGE NOT NULL
)
