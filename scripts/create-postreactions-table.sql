-- Create PostReactions table manually
-- This script creates the PostReactions table that was missing from the database

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PostReactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PostReactions](
        [Id] [uniqueidentifier] NOT NULL,
        [PostId] [uniqueidentifier] NOT NULL,
        [UserId] [uniqueidentifier] NOT NULL,
        [ReactionType] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [CreatedBy] [nvarchar](max) NULL,
        [ModifiedAt] [datetime2](7) NULL,
        [ModifiedBy] [nvarchar](max) NULL,
        [DeletedAt] [datetime2](7) NULL,
        [DeletedBy] [nvarchar](max) NULL,
        [IsDeleted] [bit] NOT NULL,
        [Slug] [nvarchar](max) NULL,
        [RowVersion] [rowversion] NOT NULL,
        CONSTRAINT [PK_PostReactions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PostReactions_Posts_PostId] FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_PostReactions_AspNetUsers_UserId] FOREIGN KEY([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_PostReactions_PostId_UserId] ON [dbo].[PostReactions]
    (
        [PostId] ASC,
        [UserId] ASC
    );

    CREATE NONCLUSTERED INDEX [IX_PostReactions_PostId] ON [dbo].[PostReactions]
    (
        [PostId] ASC
    );

    CREATE NONCLUSTERED INDEX [IX_PostReactions_UserId] ON [dbo].[PostReactions]
    (
        [UserId] ASC
    );

    PRINT 'PostReactions table created successfully';
END
ELSE
BEGIN
    PRINT 'PostReactions table already exists';
END
GO
