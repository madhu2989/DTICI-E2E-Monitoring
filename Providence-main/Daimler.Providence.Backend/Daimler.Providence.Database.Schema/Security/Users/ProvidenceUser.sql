CREATE USER [monitoringService] 
FOR LOGIN [monitoringService]
WITH DEFAULT_SCHEMA = [dbo] 
GO

GRANT CONNECT TO [monitoringService]
GO

ALTER ROLE [db_ddladmin] ADD MEMBER [monitoringService];

GO
ALTER ROLE [db_datareader] ADD MEMBER [monitoringService];

GO
ALTER ROLE [db_datawriter] ADD MEMBER [monitoringService];

