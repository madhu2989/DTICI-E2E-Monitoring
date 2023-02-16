CREATE PROCEDURE [dbo].[DeleteExpiredStatetransitions]
@CutOffDate datetime
AS
BEGIN

	DELETE FROM StateTransition WHERE SourceTimestamp < @CutOffDate

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[DeleteExpiredStatetransitions] TO [monitoringService]
    AS [dbo];

