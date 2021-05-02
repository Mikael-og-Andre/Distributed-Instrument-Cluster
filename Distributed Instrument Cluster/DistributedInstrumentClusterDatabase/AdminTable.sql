CREATE TABLE [dbo].[AdminTable]
(
	[AdminID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [UserId] INT NOT NULL,
	[isAdmin] BIT NOT NULL DEFAULT 0, 
    [ElevatedById] INT NULL, 
    CONSTRAINT FK_AccountID FOREIGN KEY (UserID) REFERENCES AccountTable(AccountID)
)
