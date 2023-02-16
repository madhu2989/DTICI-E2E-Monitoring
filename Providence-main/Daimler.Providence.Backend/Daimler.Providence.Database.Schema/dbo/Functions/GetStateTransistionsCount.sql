
CREATE FUNCTION [dbo].[GetStateTransistionsCount]
( 
	
)
RETURNS TABLE 

AS

RETURN 
(
	SELECT COUNT(*) AS StatetransitionsCount FROM StateTransition
)