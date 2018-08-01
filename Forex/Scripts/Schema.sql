﻿CREATE TABLE IF NOT EXISTS [RateItems] (
	[Id] INTEGER PRIMARY KEY NOT NULL,
	[CurrencyId] INTEGER NOT NULL,
	[Rate] REAL NOT NULL,
	[Time] NUMERIC NOT NULL
);

CREATE TABLE IF NOT EXISTS [RateSummaries] (
	[Id] INTEGER PRIMARY KEY NOT NULL,
	[CurrencyId] INTEGER NOT NULL,
	[MinRate] REAL NOT NULL,
	[MaxRate] REAL NOT NULL,
	[Date] NUMERIC NOT NULL
);
