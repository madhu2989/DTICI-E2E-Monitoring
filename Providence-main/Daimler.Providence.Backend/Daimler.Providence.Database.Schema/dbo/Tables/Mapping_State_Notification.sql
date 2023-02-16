CREATE TABLE [dbo].[Mapping_State_Notification] (
    [StateId]        INT NOT NULL,
    [NotificationId] INT NOT NULL,
    CONSTRAINT [PK_Mapping_State_Notification] PRIMARY KEY CLUSTERED ([StateId] ASC, [NotificationId] ASC),
    CONSTRAINT [FK_Mapping_State_Notification_NotificationConfiguration] FOREIGN KEY ([NotificationId]) REFERENCES [dbo].[NotificationConfiguration] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Mapping_State_Notification_State] FOREIGN KEY ([StateId]) REFERENCES [dbo].[State] ([Id])
);

