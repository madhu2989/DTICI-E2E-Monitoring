
CREATE FUNCTION [dbo].[GetChangelogsCount]
( 
	
)
RETURNS TABLE 

AS

RETURN 
(
	SELECT COUNT(*) AS ChangelogsCount FROM Changelog
)