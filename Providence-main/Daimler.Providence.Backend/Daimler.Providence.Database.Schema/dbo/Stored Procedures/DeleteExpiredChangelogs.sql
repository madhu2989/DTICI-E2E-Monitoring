CREATE PROCEDURE [dbo].[DeleteExpiredChangelogs]
@CutOffDate datetime
AS
BEGIN

DELETE FROM Changelog WHERE ChangeDate < @CutOffDate

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[DeleteExpiredChangelogs] TO [monitoringService]
    AS [dbo];