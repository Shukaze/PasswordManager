CREATE PROCEDURE [dbo].[sp_InsertCard]
    @card nvarchar(50),
    @pin int,
    @ivc int,
	@username nvarchar(50),
    @Id int out
AS
    INSERT INTO Cards ([card], pin, ivc, Username)
    VALUES (@card, @pin, @ivc, @username)
   
    SET @Id=SCOPE_IDENTITY()
