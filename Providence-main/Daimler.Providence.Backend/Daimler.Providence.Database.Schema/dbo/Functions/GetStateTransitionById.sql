CREATE FUNCTION [dbo].[GetStateTransitionById]
( 
	@Id int
)
 
RETURNS TABLE 

AS
RETURN 
( 
	SELECT DISTINCT st1.*, env.[Name] AS EnvironmentName, s.[Name] AS StateName, e.[Name] as TriggerName
	FROM StateTransition st1 
	INNER JOIN Environment env ON env.Id = st1.Environment
	INNER JOIN [State] s ON s.Id = st1.[State]
	INNER JOIN [ElementNames] e ON e.elementId = ISNULL( st1.TriggeredByElementId , st1.TriggeredByCheckId) AND e.EnvironmentId = st1.Environment
	WHERE st1.Id = @Id
)
