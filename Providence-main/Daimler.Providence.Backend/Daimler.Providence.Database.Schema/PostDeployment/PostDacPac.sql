ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = OFF; 
GO

/*Database masterdata*/

/*Table State*/
SET IDENTITY_INSERT [State] ON
GO

INSERT INTO [State] (Id, [Name])  
SELECT *  FROM (VALUES (1, 'OK'), (2, 'WARNING'), (3, 'ERROR')) AS S1 (Id, [Name])
EXCEPT
SELECT Id, [Name] FROM [State]
GO

SET IDENTITY_INSERT [State] OFF
GO

/*Table ComponentType*/
SET IDENTITY_INSERT ComponentType ON
GO

INSERT INTO ComponentType (Id, [Name])  
SELECT *  FROM (VALUES (1, 'AZURE'), (2, 'EXTERNAL'), (3, 'Check'), (4, 'Action'), (5, 'Environment'), (6, 'Service'), (7, 'Component')) AS S1 (Id, [Name])
EXCEPT
SELECT Id, [Name] FROM ComponentType
GO

SET IDENTITY_INSERT ComponentType OFF
GO
