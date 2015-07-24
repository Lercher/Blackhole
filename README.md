# Blackhole
A hash based RDF triple store for SQL Server with a WebAPI REST API. An AngularJS based 
Web Management UI for the basic management tasks, syntax checking, queries and updates
is provided with the installation.


In this project we bring together the following technologies:


### [dotNetRDF](http://dotnetrdf.org/)
Provides the SPARQL parsers and query execution processors, together with it's virtualization 
infrastructure for efficient storage.

We currently (07/2015) need a slightly [modified version](https://bitbucket.org/MartinLercher/dotnetrdf) of it to make join
queries on virtualized nodes work. See [CORE-452](http://dotnetrdf.org/tracker/Issues/IssueDetail.aspx?id=452) 
for details.

The patch is now (late 07/2015) integrated with dotNetRDF's sources, so version 1.0.10 will be fine.


### [MS SQL Server](https://en.wikipedia.org/wiki/Microsoft_SQL_Server)
Provides robust storage for quads and nodes. Opens the solution for further 
tooling, standardized backups and e.g. replication.

Storage for each *store* consists of a schema bh_ *store* and two tables, bh_ *store*.QUAD and
bh_ *store*.NODE and nothing else.


### [Linq to SQL](https://msdn.microsoft.com/en-us/data/cc298428.aspx)
A lightweight ORM framework for accessing the SQL Database.


### [City Hash 128bit](https://code.google.com/p/cityhash/) 
Provides a non cryptographic hash function that maps nicely to .Net's System.Guid and SQL Server's uniqueidentifier. 
This hash is currently used to virtualize nodes. We do not need to make roundtrips to the database 
to check if a node already exists or to generate the virtual IDs. We rely on the fact that hash collisions 
are *very* unlikely. The store has the ability to detect hash collisions by calculating and storing a second 
128bit hash alongside the nodes.


### [Web API](http://www.asp.net/web-api)
Provides REST based Web Services for syntax check, query and update.


### [Fleck](https://github.com/statianzo/Fleck)
For a websocket server implementation that also runs on windows systems older than Windows 8 
and Windows Server 2012. Provides asynchronous notification on updates via websockets.

Alternatives:
SignalR would use Long polling on Win 7 and requires an OWIN container
that I don't want to use. Socket.io as a client lib would be desireable, but I'm not sure if it supports
Fleck. There is no Socket.io protocol support at the server side in Web API, according to my research.


### [Topshelf](http://topshelf-project.com/)
Console App, Windows Service and it's installation in one binary.


### [WiX](http://wixtoolset.org/)
For building the msi setup.



# The WebSocket Notification API

It sits currently on the url ws://*host*:8091/*store*

As a client, if you send nothing or an illegal ASK query to the socket, you recieve general
notification messages when queries or updates are executed against the *store*. These messages
all start with a square bracket character and end with a square bracket character.

If you send a valid sparql ASK query, the behaviour changes as follows. 

First the client gets the normalized
ask query enclosed in square brackets as an acknowledgement. The standard log on the socket is turned off.
Then all inserted triples for all following update operations on *store* are asserted to a seperate graph as its default
triples. If and only if the ask query succeeds on this graph of inserts, the websocket reponds with the normalized 
ask query it was fed with before.

With this mechanism a client program is notified asynchronously of changes on specified uris or literals in subject, 
predicate or object positions. After notification a client program can requery detail data on the changed triples.

Note: Deletes are currently not notified.


## Obsolete Plans


### [SignalR](http://signalr.net/)
Support for update notifications. Cancelled, because Fleck is simpler and more flexible.

