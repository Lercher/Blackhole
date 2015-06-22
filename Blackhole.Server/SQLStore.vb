Imports VDS
Imports VDS.RDF
Imports VDS.RDF.Storage
Imports VDS.RDF.Storage.Virtualisation
Imports VDS.RDF.Query.Optimisation
Imports VDS.RDF.Query
Imports VDS.RDF.Parsing.Handlers
Imports VDS.RDF.Parsing
Imports VDS.RDF.Update

' see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/BaseAdoStoreManager.cs
' and http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/Schemas/CreateMicrosoftAdoHashStore.sql

Public Class SQLStore
    Implements IVirtualRdfProvider(Of Guid, Guid)
    Implements IUpdateableStorage
    Implements IDisposable


    '       : IUpdateableStorage, IUpdateableGenericIOManager, IVirtualRdfProvider<int, int>, IConfigurationSerializable, IDisposable

    Private ReadOnly GuidGenerator As IGuidGenerator

    Public Sub New()
        GuidGenerator = New HashGuidGenerator With {.HashProvider = New CityHashFunction}
    End Sub



    Private ReadOnly Property ctx As BlackholeDBDataContext
        Get
            Return New BlackholeDBDataContext()
        End Get
    End Property

    Private ReadOnly Property ctxRO As BlackholeDBDataContext
        Get
            Dim c = ctx
            c.ObjectTrackingEnabled = False
            Return c
        End Get
    End Property



    ' primary implementation
    Public Function GetBlankNodeID(value As IBlankNode, createIfNotExists As Boolean) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetBlankNodeID
        Dim vn = TryCast(value, IVirtualNode(Of Guid, Guid))
        If vn IsNot Nothing AndAlso vn.Provider Is Me Then Return vn.VirtualID
        If Not createIfNotExists Then Return Guid.Empty
        Return Guid.NewGuid
    End Function

    Public Function GetBlankNodeID(value As IBlankNode) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetBlankNodeID
        Return GetBlankNodeID(value, False)
    End Function

    ' primary implementation
    Public Function GetGraphID(graphUri As Uri, createIfNotExists As Boolean) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetGraphID
        Return GuidGenerator.fromGraphUri(graphUri)
    End Function

    Public Function GetGraphID(graphUri As Uri) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetGraphID
        Return GetGraphID(graphUri, False)
    End Function

    Public Function GetGraphID(g As IGraph) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetGraphID
        Return GetGraphID(g.BaseUri)
    End Function

    Public Function GetGraphID(g As IGraph, createIfNotExists As Boolean) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetGraphID
        Return GetGraphID(g.BaseUri, createIfNotExists)
    End Function

    Public Function GetGraphUri(id As Guid) As Uri Implements IVirtualRdfProvider(Of Guid, Guid).GetGraphUri
        If Guid.Empty.Equals(id) Then Return Nothing
        Using c = ctxRO
            Return Aggregate n In c.NODEs Where n.ID = id Select BlackholeNodeFactory.toUri(n.value) Into FirstOrDefault()
        End Using
    End Function

    ' primary implementation
    Public Function GetID(value As INode, createIfNotExists As Boolean) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetID
        Dim vn = TryCast(value, IVirtualNode(Of Guid, Guid))
        If vn IsNot Nothing AndAlso vn.Provider Is Me Then Return vn.VirtualID
        If Not createIfNotExists Then Return Guid.Empty
        Return GuidGenerator.fromNode(value)
    End Function

    Public Function GetID(value As INode) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetID
        Return GetID(value, False)
    End Function

    Public Function GetValue(g As IGraph, id As Guid) As INode Implements IVirtualRdfProvider(Of Guid, Guid).GetValue
        If Guid.Empty.Equals(id) Then Return Nothing
        Using c = ctxRO
            Dim qy =
                Aggregate q In c.NODEs
                Where q.ID = id
                Select BlackholeNodeFactory.Create(g, BlackholeNodeFactory.ToNodeType(q.type), id, q.value, q.metadata)
                Into FirstOrDefault()
            If qy IsNot Nothing Then Return qy
            Return g.CreateBlankNode(id.ToString)
        End Using
    End Function

    Public Sub LoadGraphVirtual(g As IGraph, graphUri As Uri) Implements IVirtualRdfProvider(Of Guid, Guid).LoadGraphVirtual
        Dim gid = GetGraphID(graphUri)
        g.BaseUri = graphUri
        Using c = ctxRO
            Dim qy =
                From q In c.QUADs
                Where q.graph = gid
                Select
                    s = BlackholeNodeFactory.CreateVirtual(g, BlackholeNodeFactory.ToNodeType(q.subjecttype), q.subject, Me),
                    p = BlackholeNodeFactory.CreateVirtual(g, BlackholeNodeFactory.ToNodeType(q.predicatetype), q.predicate, Me),
                    o = BlackholeNodeFactory.CreateVirtual(g, BlackholeNodeFactory.ToNodeType(q.objecttype), q.object, Me)
                Select New Triple(s, p, o)
            g.Assert(qy)
        End Using
    End Sub

    Public ReadOnly Property NullID As Guid Implements IVirtualRdfProvider(Of Guid, Guid).NullID
        Get
            Return Guid.Empty
        End Get
    End Property


    ' ---------------------------------------------- IQueryableStorage -----------------------------------------------------------

    Public Function Query(sparqlQuery As String) As Object Implements IQueryableStorage.Query
        Dim g As New Graph
        Dim results = New SparqlResultSet
        Query(New GraphHandler(g), New ResultSetHandler(results), sparqlQuery)
        If results.ResultsType = SparqlResultsType.Unknown Then Return g
        Return results
    End Function

    Private Function EnsureDataset() As Query.Datasets.ISparqlDataset
        Static Dataset As Query.Datasets.ISparqlDataset = DS.Create(Me)
        Return Dataset
    End Function

    ' see https://bitbucket.org/dotnetrdf/dotnetrdf/src/df95e1283cecd046b1f6fbf6fb1d396c888dfe20/Libraries/core/net40/Web/BaseSparqlServer.cs?at=default
    Public Sub Query(rdfHandler As IRdfHandler, resultsHandler As ISparqlResultsHandler, sparqlQuery As String) Implements IQueryableStorage.Query
        Static Parser As New SparqlQueryParser(SparqlQuerySyntax.Extended)
        Static Optimizer As HashingAlgebraOptimizer = New HashingAlgebraOptimizer(Me)
        'P.ExpressionFactories = ...
        'P.QueryOptimiser = ...
        Dim Query = Parser.ParseFromString(sparqlQuery)
        Query.AlgebraOptimisers = New IAlgebraOptimiser() {Optimizer}
        Static Processor As New LeviathanQueryProcessor(EnsureDataset())
        Processor.ProcessQuery(rdfHandler, resultsHandler, Query)
    End Sub


    Public Sub Update(sparqlUpdate As String) Implements IUpdateableStorage.Update
        Static Updateparser As New SparqlUpdateParser
        Static Optimizer As HashingAlgebraOptimizer = New HashingAlgebraOptimizer(Me)
        Dim cmds = Updateparser.ParseFromString(sparqlUpdate)
        Static Processor As New LeviathanUpdateProcessor(EnsureDataset())
        cmds.AlgebraOptimisers = New IAlgebraOptimiser() {Optimizer}
        Processor.ProcessCommandSet(cmds)
    End Sub


    Public ReadOnly Property DeleteSupported As Boolean Implements IStorageCapabilities.DeleteSupported
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property IOBehaviour As IOBehaviour Implements IStorageCapabilities.IOBehaviour
        Get
            Return IOBehaviour.GraphStore Or IOBehaviour.CanUpdateTriples
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements IStorageCapabilities.IsReadOnly
        Get
            Return False
        End Get
    End Property

    Public ReadOnly Property IsReady As Boolean Implements IStorageCapabilities.IsReady
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property ListGraphsSupported As Boolean Implements IStorageCapabilities.ListGraphsSupported
        Get
            Return True
        End Get
    End Property

    Public ReadOnly Property UpdateSupported As Boolean Implements IStorageCapabilities.UpdateSupported
        Get
            Return True
        End Get
    End Property

    Public Sub DeleteGraph(graphUri As String) Implements IStorageProvider.DeleteGraph
        DeleteGraph(BlackholeNodeFactory.toUri(graphUri))        
    End Sub

    Public Sub DeleteGraph(graphUri As Uri) Implements IStorageProvider.DeleteGraph
        Dim id = GetGraphID(graphUri)
        Using c = ctx
            Dim x = <x>
BEGIN TRAN
DELETE n FROM node n inner join quad q on n.id = q.subject WHERE q.graph = {0}
DELETE n FROM node n inner join quad q on n.id = q.predicate WHERE q.graph = {0}
DELETE n FROM node n inner join quad q on n.id = q.object WHERE q.graph = {0}
DELETE FROM node WHERE id = {0}
DELETE FROM quad WHERE graph = {0}
COMMIT
                    </x>
            c.ExecuteCommand(x.Value, id)
        End Using
    End Sub

    Public Function ListGraphs() As IEnumerable(Of Uri) Implements IStorageProvider.ListGraphs
        Using c = ctxRO
            Dim qy = From n In c.NODEs Where n.type = 99 Select u = n.value Distinct Select BlackholeNodeFactory.toUri(u)
            Return qy.ToArray
        End Using
    End Function

    Public Sub LoadGraph(g As IGraph, graphUri As String) Implements IStorageProvider.LoadGraph
        LoadGraph(g, BlackholeNodeFactory.toUri(graphUri))
    End Sub

    Public Sub LoadGraph(g As IGraph, graphUri As Uri) Implements IStorageProvider.LoadGraph
        If g.IsEmpty Then g.BaseUri = graphUri
        LoadGraph(New GraphHandler(g), graphUri)
    End Sub

    Public Sub LoadGraph(handler As IRdfHandler, graphUri As String) Implements IStorageProvider.LoadGraph
        LoadGraph(handler, BlackholeNodeFactory.toUri(graphUri))
    End Sub

    Public Sub LoadGraph(handler As IRdfHandler, graphUri As Uri) Implements IStorageProvider.LoadGraph
        If handler Is Nothing Then Throw New RdfStorageException("Cannot load a Graph using a null RDF Handler")
        handler.StartRdf()
        Dim gid = GetGraphID(graphUri)
        Using c = ctxRO
            Dim qy =
                From q In c.QUADs
                Where q.graph = gid
                Join ns In c.NODEs On ns.ID Equals q.subject
                Join np In c.NODEs On np.ID Equals q.predicate
                Join no In c.NODEs On no.ID Equals q.object
                Select
                    s = BlackholeNodeFactory.Create(handler, BlackholeNodeFactory.ToNodeType(ns.type), ns.ID, ns.value, ns.metadata),
                    p = BlackholeNodeFactory.Create(handler, BlackholeNodeFactory.ToNodeType(np.type), np.ID, np.value, np.metadata),
                    o = BlackholeNodeFactory.Create(handler, BlackholeNodeFactory.ToNodeType(no.type), no.ID, no.value, no.metadata)
                Select New Triple(s, p, o)
            For Each t In qy
                If Not handler.HandleTriple(t) Then ParserHelper.Stop()
            Next
        End Using
    End Sub

    Public ReadOnly Property ParentServer As Management.IStorageServer Implements IStorageProvider.ParentServer
        Get
            Return Nothing
        End Get
    End Property

    Public Sub SaveGraph(g As IGraph) Implements IStorageProvider.SaveGraph
        Dim gid = GetGraphID(g)
        DeleteGraph(g.BaseUri)
        Using c = ctx
            Dim nqy = From t In g.Nodes Distinct Select n = CreateDBNode(gid, t)
            c.NODEs.InsertAllOnSubmit(nqy)
            c.NODEs.InsertOnSubmit(New NODE With {.ID = gid, .type = 99})
            Dim qy = From t In g.Triples Select q = CreateDBQuad(gid, t.Subject, t.Predicate, t.Object)
            c.QUADs.InsertAllOnSubmit(qy)
            c.SubmitChanges()
        End Using
    End Sub

    Private Function CreateDBNode(gid As Guid, n As INode) As NODE
        Return New NODE With {
            .ID = GetID(n),
            .type = CByte(n.NodeType),
            .value = BlackholeNodeFactory.DBValueOf(n),
            .metadata = BlackholeNodeFactory.DBMetaOf(n)
        }
    End Function

    Private Function CreateDBQuad(gid As Guid, s As INode, p As INode, o As INode) As QUAD
        Return New QUAD With {
            .graph = gid,
            .subjecttype = CByte(s.NodeType),
            .subject = GetID(s),
            .predicatetype = CByte(p.NodeType),
            .predicate = GetID(p),
            .objecttype = CByte(p.NodeType),
            .object = GetID(p)
        }
    End Function

    Public Sub UpdateGraph(graphUri As String, additions As IEnumerable(Of Triple), removals As IEnumerable(Of Triple)) Implements IStorageProvider.UpdateGraph
        UpdateGraph(BlackholeNodeFactory.toUri(graphUri), additions, removals)
    End Sub

    ' see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/BaseAdoStoreManager.cs#l1099
    Public Sub UpdateGraph(graphUri As Uri, additions As IEnumerable(Of Triple), removals As IEnumerable(Of Triple)) Implements IStorageProvider.UpdateGraph

    End Sub


#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
