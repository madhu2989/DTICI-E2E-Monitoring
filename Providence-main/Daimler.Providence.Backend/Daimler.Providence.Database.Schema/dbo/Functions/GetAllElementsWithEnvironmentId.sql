

CREATE FUNCTION [dbo].[GetAllElementsWithEnvironmentId]
( 
	@EnvironmentId int
)

RETURNS TABLE 

AS

RETURN 
(
	SELECT ElementId AS ElementId, ElementId AS EnvironmentSubscriptionId, CreateDate AS CreationDate, 'Environment' AS 'Type' FROM Environment WHERE Id = @EnvironmentId
	UNION
	SELECT s.ElementId AS ElementId, e.ElementId  AS EnvironmentSubscriptionId, s.CreateDate AS CreationDate, 'Service' AS 'Type' FROM [Service] s INNER JOIN Environment e ON e.Id = s.EnvironmentId WHERE s.EnvironmentId = @EnvironmentId
	UNION
	SELECT a.ElementId AS ElementId, e.ElementId AS EnvironmentSubscriptionId, a.CreateDate AS CreationDate, 'Action' AS 'Type'  FROM [Action] a INNER JOIN Environment e ON e.Id = a.EnvironmentId WHERE a.EnvironmentId = @EnvironmentId
	UNION
	SELECT c.ElementId AS ElementId, e.ElementId AS EnvironmentSubscriptionId, c.CreateDate AS CreationDate, 'Component' AS 'Type'  FROM [Component] c INNER JOIN Environment e ON e.Id = c.EnvironmentId WHERE c.EnvironmentId = @EnvironmentId
	UNION
	SELECT ch.ElementId AS ElementId, e.ElementId AS EnvironmentSubscriptionId, null AS CreationDate, 'Check' AS 'Type'  FROM [Check] ch INNER JOIN Environment e ON e.Id = ch.EnvironmentId WHERE ch.EnvironmentId = @EnvironmentId

	
)