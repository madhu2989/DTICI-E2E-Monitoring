
CREATE FUNCTION [dbo].[GetInitialStateByElementId]
( 
	@ElementId varchar(max),
	@Environment int,
	@referenceDate datetime
)

RETURNS TABLE 

AS
	RETURN 
( 	
	SELECT Top 1 st1.*, ct.[Name] AS ComponentTypeName, env.[Name] AS EnvironmentName, s.[Name] AS StateName, e.[Name] as TriggerName 
	FROM  statetransition st1 
	INNER JOIN Environment env ON env.Id = st1.Environment
	INNER JOIN ComponentType ct ON st1.ComponentType = ct.Id
	INNER JOIN [State] s ON s.Id = st1.[State]
	INNER JOIN [ElementNames] e ON e.elementId = ISNULL( st1.TriggeredByElementId , st1.TriggeredByCheckId) AND e.EnvironmentId = @Environment
	INNER JOIN (SELECT MAX(SourceTimeStamp) AS SourceTimeStamp, ISNULL(ElementId, '') + ISNULL(CheckId, '') + ISNULL(Alertname, '') AS 'Key' FROM StateTransition WHERE SourceTimestamp < @referenceDate AND Environment = @Environment GROUP BY ElementId, CheckId, AlertName) st2 
		ON st2.[Key] = ISNULL(st1.ElementId, '') + ISNULL(st1.CheckId,'') + ISNULL(st1.AlertName, '')
	AND st1.SourceTimestamp = st2.SourceTimeStamp
	AND st1.Environment = @Environment	
	AND st1.SourceTimestamp < @referenceDate
	AND st1.ElementId = @ElementId 
	ORDER BY SourceTimestamp DESC
)

