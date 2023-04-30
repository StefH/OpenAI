﻿CREATE TABLE "TextFragments" (
	"Id"	INTEGER NOT NULL,
	"Prefix"	TEXT NOT NULL,
	"Index"	INTEGER NOT NULL,
	"Text"	TEXT NOT NULL,
	"Tokens"	INTEGER NOT NULL,
	"EmbeddingAsBinary"	INTEGER NOT NULL,
	PRIMARY KEY("Id" AUTOINCREMENT)
)