USE [CosineSearch]
GO

/****** Object: Table [dbo].[TextFragments] Script Date: 30-4-2023 09:16:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TextFragments] (
    [Id]                INT             IDENTITY (1, 1) NOT NULL,
    [Prefix]            NVARCHAR (128)  NOT NULL,
    [Index]             INT             NOT NULL,
    [Text]              NVARCHAR (MAX)  NOT NULL,
    [Tokens]            INT             NOT NULL,
    [EmbeddingAsBinary] VARBINARY (MAX) NOT NULL
);