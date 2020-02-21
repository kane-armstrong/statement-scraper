# Statement Scraper

Statement scraper is an application that implements scraping transactional data from a personal banking
site and loading it into a SQL database. It is designed for a specific New Zealand-based bank and is not
portable across other banks without changes. If you want to use this code and are able to puzzle out
which bank it is designed for, and you are not banking with them, then you may be able to get away with
just reimplementing StatementScraper and adhering to the schema described in Contracts.

## Motivation

I dislike the idea of paying for personal budgeting software:

* They are glorified spreadsheets
* There is a lot of data entry and manual work involved in categorizing transactions
* Many solutions support automatic categorization (pattern matching on description, mapping on payee, etc.)
but I've yet to find a service that does this with a sufficient degree of accuracy
* I'm forced into whatever (limited) analytics are offered by the product, and these are always far
less flexible than simply writing a SQL query and hooking it up to a PowerBI report
* I don't really care about micromanaging my spending within a budget; I'm more interested in seeing
outliers in spending behavior, forecasting my financial position by various non-trivial rules, and so forth.
This isn't what any of the products on the market do
* Why pay someone to automate loading data into what is essentially a spreadsheet when I can do so myself?

I wrote this to collect my bank transactions into a SQL database so that I can write SQL queries and wire
them up to various PowerBI reports. I have a long list of rules (simple `CASE` expressions in a SQL view)
that look at `payee`, `transaction type`, and patterns in `description` to figure out what the correct
category is.

## Features

* Download bank statements, deserialize them, and load them into a SQL database (without duplication)

## Usage

1. Publish the database project
2. Replace the placeholder values in appsettings.json with real ones (obviously avoid checking them in; I use user
secrets when testing code changes in my IDE, for instance)