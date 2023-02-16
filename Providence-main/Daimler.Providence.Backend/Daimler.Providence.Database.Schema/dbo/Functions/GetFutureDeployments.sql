CREATE FUNCTION [dbo].[GetFutureDeployments]
( 
	@EnvironmentId INT
)

RETURNS TABLE 

AS
	RETURN 
( 	
	SELECT DISTINCT dp.*, env.[Name] AS EnvironmentName, env.ElementId AS EnvironmentSubscriptionId
	FROM Deployment dp
	INNER JOIN Environment env ON env.Id = dp.EnvironmentId
	WHERE dp.EnvironmentId  = @EnvironmentId 
	AND (GETDATE() <= dp.StartDate OR (dp.StartDate <= GETDATE() AND (GETDATE() <= dp.EndDate OR dp.EndDate IS NULL)))
)