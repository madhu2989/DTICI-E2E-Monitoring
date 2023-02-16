CREATE PROCEDURE [dbo].[DeleteUnusedComponents]
AS
BEGIN

	DELETE FROM [Component] WHERE Id NOT IN (SELECT ComponentId FROM Mapping_Action_Component)

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[DeleteUnusedComponents] TO [monitoringService]
    AS [dbo];

