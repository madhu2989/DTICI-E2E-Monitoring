

CREATE View [dbo].[ElementNames] AS 
	SELECT [Name], ElementId, Id AS EnvironmentId FROM Environment
	UNION
	SELECT [Name], ElementId, EnvironmentId FROM [Service] 
	UNION
	SELECT [Name], ElementId, EnvironmentId FROM [Action] 
	UNION
	SELECT [Name], ElementId, EnvironmentId FROM [Component] 
	UNION
	SELECT [Name], ElementId, EnvironmentId FROM [Check]