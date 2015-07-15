# Blackhole
A hash based RDF triple store for SQL Server with a WebAPI REST server, based on dotNetRDF 1.0

In this project we bring together the following technologies:

### [dotNetRDF](http://dotnetrdf.org/)
Provides the SPARQL parsers and query execution processors, together with it's virtualization 
infrastructure for efficient storage.

### MS SQL Server
Provides robust storage for quads and nodes. Opens the solution for further 
tooling, standardized backups and e.g. replication.

### [City Hash 128bit](https://code.google.com/p/cityhash/) 
Provides a non cryptographic hash function that maps nicely to .Net's System.Guid and SQL Server's uniqueidentifier. 
This hash is currently used to virtualize nodes. We do not make roundtrips to the database to check if a node already exists 
or to generate the virtual IDs, we rely on the fact that hash collisions are very unlikely. The store has the ability 
to detect hash collisions by calculation a second 128bit hash.

### [Web API](http://www.asp.net/web-api)
Provides the REST based Web Services

### [Topshelf](http://topshelf-project.com/)
Console App, Windows Service and it's installation in one binary.

### [WiX](http://wixtoolset.org/)
For building the msi setup.

### [Fleck](https://github.com/statianzo/Fleck)
For a websocket server implementation that also runs on windows systems older than Windows 8 
and Windows Server 2012. 

Alternatives:
SignalR would use Long polling on Win 7 and requires an OWIN container
that I don't want to use. Socket.io as a client lib would be desireable, but I'm not sure if it supports
Fleck. There is no Socket.io protocol support at the server side in Web API, according to my research.


# Obsolete Plans


### [SignalR](http://signalr.net/)
Support for update notifications
