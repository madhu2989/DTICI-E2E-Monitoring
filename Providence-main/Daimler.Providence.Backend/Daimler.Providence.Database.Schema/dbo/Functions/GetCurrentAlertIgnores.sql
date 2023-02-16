
CREATE FUNCTION [dbo].[GetCurrentAlertIgnores]
( 
	@EnvironmentSubscriptionId NVARCHAR(500)
)

RETURNS TABLE 

AS
	RETURN 
( 	
	SELECT *
	FROM AlertIgnore
	WHERE GETDATE() BETWEEN CreationDate AND ExpirationDate
	AND EnvironmentSubscriptionId = @EnvironmentSubscriptionId

)