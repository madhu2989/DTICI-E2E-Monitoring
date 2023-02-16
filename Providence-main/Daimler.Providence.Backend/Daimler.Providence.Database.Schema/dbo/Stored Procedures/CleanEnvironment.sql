CREATE PROCEDURE [dbo].[CleanEnvironment]
 @EnvironmentId int
AS
BEGIN
	Delete From Service Where EnvironmentId = @EnvironmentId
	Delete From Action Where EnvironmentId = @EnvironmentId
	Delete From Component Where EnvironmentId = @EnvironmentId
	Delete From [Check] Where EnvironmentId = @EnvironmentId
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[CleanEnvironment] TO [monitoringService]
    AS [dbo];

