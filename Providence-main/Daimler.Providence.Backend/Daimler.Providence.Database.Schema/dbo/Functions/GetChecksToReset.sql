
CREATE FUNCTION [dbo].[GetChecksToReset]
( 
	@Environment int
)
 
RETURNS TABLE 

AS

RETURN 
( 
	SELECT DISTINCT st1.*, env.[Name] AS EnvironmentName, s.[Name] AS StateName, c.Frequency
	FROM StateTransition st1
	INNER JOIN Environment env ON env.Id = st1.Environment
	INNER JOIN [State] s ON s.Id = st1.[State]
	INNER JOIN [Check] c ON c.ElementId = st1.[CheckId] AND c.EnvironmentId = @Environment
	INNER JOIN (SELECT MAX(SourceTimeStamp) AS SourceTimeStamp, ISNULL(ElementId, '') + ISNULL(CheckId, '') + ISNULL(Alertname, '') AS 'Key' FROM StateTransition WHERE Environment = @Environment GROUP BY ElementId, CheckId, AlertName) st2 
		ON st2.[Key] = ISNULL(st1.ElementId, '') + ISNULL(st1.CheckId,'') + ISNULL(st1.AlertName, '')
	AND st1.SourceTimestamp = st2.SourceTimeStamp
	AND st1.Environment = @Environment	
	AND (st1.[State] = 3 OR st1.[State] = 2)
)