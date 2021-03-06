SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_password_get_by_username]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 02/07/2008
-- Description:	Retrieve a user''s password based
--				on username
-- =============================================
CREATE PROCEDURE [dbo].[r_user_password_get_by_username]
	@username varchar(50)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT r_user.r_password FROM r_user WHERE r_user.r_username = @username
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_rule]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[r_rule](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[r_name] [nvarchar](50) NOT NULL,
	[r_description] [nvarchar](255) NOT NULL,
	[r_configuration] [xml] NOT NULL,
	[r_precedence] [int] NOT NULL,
	[r_enable] [bit] NOT NULL CONSTRAINT [DF_r_rule_r_enable]  DEFAULT ((0)),
 CONSTRAINT [PK_r_rule] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_remove_by_id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Remove a user
-- =============================================
CREATE PROCEDURE [dbo].[r_user_remove_by_id]
	@id int
AS
BEGIN
	DELETE FROM r_user WHERE id = @id
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_password_update_by_id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Update uesr password
-- =============================================
CREATE PROCEDURE [dbo].[r_user_password_update_by_id]
	-- Add the parameters for the stored procedure here
	@id int,
	@password varchar(74)
AS
BEGIN
	UPDATE r_user SET r_password = @password WHERE id = @id
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_password_get_by_id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Retrive user''s password based on id
-- =============================================
CREATE PROCEDURE [dbo].[r_user_password_get_by_id]
	@id int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT r_password FROM r_user WHERE id = @id
END
' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_device_remove_by_guid]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Remove a device
-- =============================================
CREATE PROCEDURE [dbo].[r_device_remove_by_guid]
	@guid varchar(36)
AS
BEGIN
	DELETE FROM r_device WHERE r_guid = @guid
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_device]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[r_device](
	[r_guid] [varchar](36) NOT NULL CONSTRAINT [DF_r_obj_device_r_guid]  DEFAULT ('00000000-0000-0000-0000-000000000000'),
	[r_name] [nvarchar](50) NOT NULL,
	[r_description] [nvarchar](255) NOT NULL,
	[r_type] [varchar](36) NOT NULL,
	[r_profile] [xml] NOT NULL,
	[r_configuration] [xml] NOT NULL,
 CONSTRAINT [PK_r_device] PRIMARY KEY CLUSTERED 
(
	[r_guid] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[r_user](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[r_username] [varchar](50) NOT NULL,
	[r_password] [varchar](74) NOT NULL,
	[r_name] [nvarchar](50) NOT NULL,
	[r_description] [nvarchar](255) NOT NULL,
	[r_permission] [int] NOT NULL,
 CONSTRAINT [PK_r_user] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_statistics]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[r_statistics](
	[r_device] [varchar](36) NOT NULL,
	[r_service] [smallint] NOT NULL,
	[r_date] [datetime] NOT NULL,
	[r_type] [tinyint] NOT NULL,
	[r_count] [bigint] NOT NULL,
	[r_value] [real] NOT NULL,
 CONSTRAINT [PK_r_statistics] PRIMARY KEY CLUSTERED 
(
	[r_device] ASC,
	[r_service] ASC,
	[r_date] ASC,
	[r_type] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_rule_create]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 03/31/2008
-- Description:	Add a new rule
-- =============================================
CREATE PROCEDURE [dbo].[r_rule_create]
	@name nvarchar(50),
	@description nvarchar(255),
	@configuration XML,
	@precedence int,
	@enable bit
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO r_rule (r_name, r_description, r_configuration, r_precedence, r_enable)
	VALUES (@name, @description, @configuration, @precedence, @enable)
    
	SELECT @@IDENTITY
	RETURN @@IDENTITY
END


' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_rule_remove_by_id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 03/31/2008
-- Description:	Remove a rule according to its ID
-- =============================================
CREATE PROCEDURE [dbo].[r_rule_remove_by_id]
	@id int
AS
BEGIN
	DELETE FROM r_rule where id = @id
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_rule_update_by_id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Update a rule
-- =============================================
CREATE PROCEDURE [dbo].[r_rule_update_by_id]
	@id int,
	@name varchar(50),
	@description varchar(255),
	@configuration XML,
	@precedence int,
	@enable bit
AS
BEGIN
	UPDATE r_rule SET
		r_name = @name,
		r_description = @description,
		r_configuration = @configuration,
		r_precedence = @precedence,
		r_enable = @enable
	WHERE id = @id		
END


' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_rule_get_by_id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Retrieve a rule
-- =============================================
CREATE PROCEDURE [dbo].[r_rule_get_by_id]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

    SELECT id, r_name, r_description, r_configuration, r_precedence, r_enable FROM r_rule
	WHERE id = @id
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_rule_list_all]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 03/31/2008
-- Description:	Query all rules in the database
-- =============================================
CREATE PROCEDURE [dbo].[r_rule_list_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT id, r_name, r_description, r_configuration, r_precedence, r_enable FROM r_rule
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_device_update_by_guid]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Update a device
-- =============================================
CREATE PROCEDURE [dbo].[r_device_update_by_guid]
	@guid varchar(36),
	@name nvarchar(50),
	@description nvarchar(255),
	@type varchar(36),
	@profile XML,
	@configuration XML
AS
BEGIN
	UPDATE r_device SET r_name = @name, r_description = @description, r_type = @type, r_profile = @profile, r_configuration = @configuration
	WHERE r_guid = @guid
END






' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_device_get_by_guid]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 03/30/2008
-- Description:	Query detail on a Device based on GUID
-- =============================================
CREATE PROCEDURE [dbo].[r_device_get_by_guid]
	-- Add the parameters for the stored procedure here
	@guid varchar(36) 
AS
BEGIN
	SET NOCOUNT ON;

	SELECT r_guid, r_name, r_description, r_profile, r_type, r_configuration FROM r_device
	WHERE r_guid = @guid
END



' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_device_create]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Creates a new device
-- =============================================
CREATE PROCEDURE [dbo].[r_device_create]
	@guid varchar(36),
	@name nvarchar(50),
	@description nvarchar(255),
	@type varchar(36),
	@profile XML,
	@configuration XML
AS
BEGIN
    INSERT INTO r_device (r_guid, r_name, r_description, r_type, r_profile, r_configuration)
	VALUES (@guid, @name, @description, @type, @profile, @configuration)
END





' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_device_list_all]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	List all devices
-- =============================================
CREATE PROCEDURE [dbo].[r_device_list_all]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT r_guid, r_name, r_description, r_profile, r_type, r_configuration FROM r_device
END



' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_update_by_id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Update user information
-- =============================================
CREATE PROCEDURE [dbo].[r_user_update_by_id]
	@id int,
	@name nvarchar(50),
	@description nvarchar(255),
	@permission int
AS
BEGIN
	UPDATE r_user SET r_name = @name, r_description = @description, r_permission = @permission WHERE id = @id
END


' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_get_by_id]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	Retrive a user
-- =============================================
CREATE PROCEDURE [dbo].[r_user_get_by_id]
	@id int
AS
BEGIN
	SET NOCOUNT ON;

    SELECT id,r_username,r_name,r_description,r_permission FROM r_user WHERE id = @id
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_list_all]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 04/01/2008
-- Description:	List all users
-- =============================================
CREATE PROCEDURE [dbo].[r_user_list_all]
AS
BEGIN
	SET NOCOUNT ON;

    SELECT id,r_username,r_name,r_description,r_permission FROM r_user
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_get_by_username]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 02/07/2008
-- Description:	Used when user log in
-- =============================================
CREATE PROCEDURE [dbo].[r_user_get_by_username]
	@username varchar(50)
AS
BEGIN
	SET NOCOUNT ON;
	SELECT id,r_username,r_name,r_description,r_permission FROM r_user WHERE r_username = @username
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_user_create]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 02/07/2008
-- Description:	Creates a new user
-- =============================================
CREATE PROCEDURE [dbo].[r_user_create]
	@username varchar(50),
	@name nvarchar(50),
	@description nvarchar(255),
	@permission int
AS
BEGIN
	SET NOCOUNT ON;
	IF (SELECT COUNT(*) FROM r_user WHERE r_username = @username) >= 1
	BEGIN
		SELECT -1
		RETURN -1
	END	

	INSERT INTO r_user (r_username,r_password,r_name,r_description,r_permission)
		VALUES (@username, '''', @name, @description, @permission)
	SELECT @@IDENTITY
	RETURN @@IDENTITY
END


' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_statistics_add]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 06/28/2008
-- Description:	Add statistics to database
-- =============================================
CREATE PROCEDURE [dbo].[r_statistics_add]
	@device varchar(36),
	@service smallint,
	@date_minute datetime,
	@date_hour datetime,
	@date_day datetime,
	@date_month datetime,
	@count bigint,
	@value real
AS
BEGIN
	-- For minutes
	IF (SELECT COUNT(*) FROM r_statistics WHERE r_device = @device AND r_service = @service AND r_date = @date_minute AND r_type = 1) >= 1
	BEGIN
		UPDATE r_statistics
		SET r_value = ((r_value * r_count + @value) / (r_count + @count)), r_count = r_count + @count
		WHERE r_device = @device AND r_service = @service AND r_date = @date_minute AND r_type = 1
	END
	ELSE
	BEGIN
		INSERT INTO r_statistics (r_device, r_service, r_date, r_type, r_count, r_value)
		VALUES (@device, @service, @date_minute, 1, @count, @value / @count)
	END
	-- For hours
	IF (SELECT COUNT(*) FROM r_statistics WHERE r_device = @device AND r_service = @service AND r_date = @date_hour AND r_type = 2) >= 1
	BEGIN
		UPDATE r_statistics
		SET r_value = ((r_value * r_count + @value) / (r_count + @count)), r_count = r_count + @count
		WHERE r_device = @device AND r_service = @service AND r_date = @date_hour AND r_type = 2
	END
	ELSE
	BEGIN
		INSERT INTO r_statistics (r_device, r_service, r_date, r_type, r_count, r_value)
		VALUES (@device, @service, @date_hour, 2, @count, @value / @count)
	END
	-- For days
	IF (SELECT COUNT(*) FROM r_statistics WHERE r_device = @device AND r_service = @service AND r_date = @date_day AND r_type = 3) >= 1
	BEGIN
		UPDATE r_statistics
		SET r_value = ((r_value * r_count + @value) / (r_count + @count)), r_count = r_count + @count
		WHERE r_device = @device AND r_service = @service AND r_date = @date_day AND r_type = 3
	END
	ELSE
	BEGIN
		INSERT INTO r_statistics (r_device, r_service, r_date, r_type, r_count, r_value)
		VALUES (@device, @service, @date_day, 3, @count, @value / @count)
	END
	-- For months
	IF (SELECT COUNT(*) FROM r_statistics WHERE r_device = @device AND r_service = @service AND r_date = @date_month AND r_type = 4) >= 1
	BEGIN
		UPDATE r_statistics
		SET r_value = ((r_value * r_count + @value) / (r_count + @count)), r_count = r_count + @count
		WHERE r_device = @device AND r_service = @service AND r_date = @date_month AND r_type = 4
	END
	ELSE
	BEGIN
		INSERT INTO r_statistics (r_device, r_service, r_date, r_type, r_count, r_value)
		VALUES (@device, @service, @date_month, 4, @count, @value / @count)
	END
END


' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_statistics_clean]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan
-- Create date: 06/28/2008
-- Description:	Clean up old statistics
-- =============================================
CREATE PROCEDURE [dbo].[r_statistics_clean]
	@minute int,
	@hour int,
	@day int,
	@month int
AS
BEGIN
	SET NOCOUNT ON;
    DELETE FROM r_statistics WHERE r_type = 1 AND datediff(mi,r_date,getdate()) > @minute
    DELETE FROM r_statistics WHERE r_type = 2 AND datediff(hh,r_date,getdate()) > @hour
    DELETE FROM r_statistics WHERE r_type = 3 AND datediff(dd,r_date,getdate()) > @day
    DELETE FROM r_statistics WHERE r_type = 4 AND datediff(mm,r_date,getdate()) > @month
END

' 
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[r_statistics_query]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'-- =============================================
-- Author:		Ziyan Zhou
-- Create date: 06/28/2008
-- Description:	Query statistics data
-- =============================================
CREATE PROCEDURE [dbo].[r_statistics_query]
	@device varchar(36),
	@service smallint,
	@start_date datetime,
	@end_date datetime,
	@type tinyint
AS
BEGIN
	SET NOCOUNT ON;

	SELECT r_device,r_service,r_date,r_type,r_count,r_value
	FROM r_statistics
	WHERE r_device = @device AND (r_service & @service > 0) AND r_type = @type
		AND datediff(ss,r_date,@end_date) >= 0 AND datediff(ss,@start_date,r_date) >= 0
END


' 
END
