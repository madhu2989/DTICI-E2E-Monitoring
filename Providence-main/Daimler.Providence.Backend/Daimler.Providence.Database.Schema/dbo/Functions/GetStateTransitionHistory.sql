CREATE FUNCTION [dbo].[GetStateTransitionHistory]
( 
	@Environment int,
	@StartDate datetime,
	@EndDate datetime
)
 
RETURNS TABLE 

AS
RETURN 
( 
	SELECT DISTINCT st1.Id, st1.Environment, st1.SourceTimestamp, st1.[State], st1.ComponentType, st1.GeneratedTimestamp, st1.[Description], st1.[ProgressState],
	st1.Customfield1 AS Customfield1, st1.Customfield2 AS Customfield2, st1.Customfield3 AS Customfield3, st1.Customfield4 AS Customfield4, st1.Customfield5 Customfield5,
	st1.CheckId, st1.[Guid], st1.ElementId, st1.AlertName, st1.TriggeredByCheckId, st1.TriggeredByElementId, st1.TriggeredByAlertName , env.[Name] AS EnvironmentName, s.[Name] AS StateName, e.[Name] as TriggerName
	FROM StateTransition st1 LEFT JOIN StateTransition st2
	ON (st1.ElementId = st2.ElementId AND st1.CheckId = st2.CheckId AND st1.AlertName = st2.AlertName AND st1.SourceTimestamp < st2.SourceTimestamp AND st2.Environment = @Environment)
	INNER JOIN Environment env ON env.Id = st1.Environment
	INNER JOIN [State] s ON s.Id = st1.[State]
	INNER JOIN [ElementNames] e ON e.elementId = ISNULL( st1.TriggeredByElementId , st1.TriggeredByCheckId) AND e.EnvironmentId = @Environment
	WHERE st1.Environment = @Environment AND (st1.SourceTimestamp >= @StartDate AND st1.SourceTimestamp <= @EndDate)
)
