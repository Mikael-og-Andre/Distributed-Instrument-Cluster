CREATE TABLE [dbo].[RemoteDeviceTable]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY, 
    [AccessToken] VARCHAR(128) NOT NULL UNIQUE, 
    [Name] VARCHAR(50) NULL, 
    [Location] VARCHAR(50) NULL, 
    [Type] VARCHAR(50) NULL,
	Constraint RemoteDeviceInfo Unique(Name,Location,Type)
)
