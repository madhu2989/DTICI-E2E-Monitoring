CREATE FUNCTION [dbo].[GetDeploymentHistory]
( 
	@EnvironmentId INT,
	@StartDate datetime,
	@EndDate datetime
)
 
RETURNS TABLE 

AS
RETURN 
( 
	SELECT DISTINCT dp.*, env.[Name] AS EnvironmentName, env.ElementId AS EnvironmentSubscriptionId
	FROM Deployment dp
	INNER JOIN Environment env ON env.Id = dp.EnvironmentId
	WHERE dp.EnvironmentId  = @EnvironmentId 
	AND ((dp.StartDate <= @StartDate AND dp.EndDate >= @StartDate) OR
		 (dp.StartDate >= @StartDate AND dp.EndDate <= @EndDate) OR
		 (dp.StartDate <= @EndDate AND dp.EndDate >= @EndDate) OR
		 (dp.StartDate <= @EndDate AND dp.EndDate IS NULL))
)
