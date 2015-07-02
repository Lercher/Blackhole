Imports System.Transactions
Imports VDS
Imports VDS.RDF
Imports VDS.RDF.Parsing
Imports VDS.RDF.Parsing.Handlers
Imports VDS.RDF.Query
Imports VDS.RDF.Query.Optimisation
Imports VDS.RDF.Storage
Imports VDS.RDF.Storage.Virtualisation
Imports VDS.RDF.Update

' see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/BaseAdoStoreManager.cs
' and http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/Schemas/CreateMicrosoftAdoHashStore.sql

' We use TransactionScope for Transactions:
' Please note that in .Net 4 this does not need the Distributed Transaction Coordinator Service,
' since there is only one connection string per operation such as SaveGraph.


Public Class SQLStore
    Implements IVirtualRdfProvider(Of Guid, Guid)
    Implements IUpdateableStorage
    Implements IDisposable
    'C# : IUpdateableStorage, IUpdateableGenericIOManager, IVirtualRdfProvider<int, int>, IConfigurationSerializable, IDisposable

    Private ReadOnly GuidGenerator As IGuidGenerator
    Private Dataset As Query.Datasets.ISparqlDataset 'stores a context and needs to be disposed of

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



    ' ----------------------------------------------  IVirtualRdfProvider(Of Guid, Guid) -----------------------------------------------------------

    ' primary implementation
    Public Function GetBlankNodeID(value As IBlankNode, createIfNotExists As Boolean) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetBlankNodeID
        Dim vn = TryCast(value, IVirtualNode(Of Guid, Guid))
        If vn IsNot Nothing AndAlso vn.Provider Is Me Then Return vn.VirtualID
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
        Return GuidGenerator.fromNode(value)
    End Function

    Public Function GetID(value As INode) As Guid Implements IVirtualRdfProvider(Of Guid, Guid).GetID
        Return GetID(value, False)
    End Function

    Public Function GetValue(g As IGraph, id As Guid) As INode Implements IVirtualRdfProvider(Of Guid, Guid).GetValue
        Static cache As New Dictionary(Of Guid, INode)
        Dim result As INode = Nothing
        SyncLock cache
            If Not cache.TryGetValue(id, result) Then
                result = GetValueImpl(g, id)
                ' Console.WriteLine("Materialized {0} to {1}", id, result)
                cache(id) = result
            End If
        End SyncLock
        Return result
    End Function

    Private Function GetValueImpl(ByVal g As IGraph, ByVal id As Guid) As INode
        If Guid.Empty.Equals(id) Then Return Nothing
        Using c = ctxRO
            Dim n =
                Aggregate q In c.NODEs
                Where q.ID = id
                Into FirstOrDefault()
            If n IsNot Nothing Then Return BlackholeNodeFactory.Create(g, BlackholeNodeFactory.ToNodeType(n.type), id, n.value, n.metadata)
            Return g.CreateBlankNode(id.ToString)
        End Using
    End Function

    Public Sub LoadGraphVirtual(g As IGraph, graphUri As Uri) Implements IVirtualRdfProvider(Of Guid, Guid).LoadGraphVirtual
        'LoadGraph(g, graphUri) : Return
        ' works with:  LoadGraph(g, graphUri):Return ' but not with my virtual nodes as follows
        Dim gid = GetGraphID(graphUri)
        g.BaseUri = graphUri
        Using c = ctxRO
            c.Log = Console.Out
            Dim qy =
                From q In c.QUADs
                Where q.graph = gid
                Select
                    s = BlackholeNodeFactory.CreateVirtual(g, DS.Unpack(q).Item1, q.subject, Me),
                    p = BlackholeNodeFactory.CreateVirtual(g, DS.Unpack(q).Item2, q.predicate, Me),
                    o = BlackholeNodeFactory.CreateVirtual(g, DS.Unpack(q).Item3, q.object, Me)
                Select New Triple(s, p, o)
            'Console.WriteLine("Asserting virtual query:")
            g.Assert(qy)
            'Console.WriteLine("Virtual Query asserted")
        End Using
    End Sub

    Public ReadOnly Property NullID As Guid Implements IVirtualRdfProvider(Of Guid, Guid).NullID
        Get
            Return Guid.Empty
        End Get
    End Property


    ' ---------------------------------------------- IQueryableStorage -----------------------------------------------------------

    Private Function EnsureDataset() As Query.Datasets.ISparqlDataset
        SyncLock Me
            If Dataset Is Nothing Then Dataset = DS.Create(Me, ctx)
        End SyncLock
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

    Public Function Query(sparqlQuery As String) As Object Implements IQueryableStorage.Query
        Dim g As New Graph
        Dim results = New SparqlResultSet
        Query(New GraphHandler(g), New ResultSetHandler(results), sparqlQuery)
        If results.ResultsType = SparqlResultsType.Unknown Then Return g
        Return results
    End Function


    ' ---------------------------------------------- IUpdateableStorage -----------------------------------------------------------

    Public Sub Update(sparqlUpdate As String) Implements IUpdateableStorage.Update
        Static Updateparser As New SparqlUpdateParser
        Static Optimizer As HashingAlgebraOptimizer = New HashingAlgebraOptimizer(Me)
        Dim cmds = Updateparser.ParseFromString(sparqlUpdate)
        Static Processor As New LeviathanUpdateProcessor(EnsureDataset())
        cmds.AlgebraOptimisers = New IAlgebraOptimiser() {Optimizer}
        Processor.ProcessCommandSet(cmds)
    End Sub

    ' ---------------------------------------------- IStorageCapabilities -----------------------------------------------------------

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

    ' ---------------------------------------------- IStorageProvider -----------------------------------------------------------

    Public Sub DeleteGraph(graphUri As String) Implements IStorageProvider.DeleteGraph
        DeleteGraph(BlackholeNodeFactory.toUri(graphUri))
    End Sub

    Public Sub DeleteGraph(graphUri As Uri) Implements IStorageProvider.DeleteGraph
        Dim id = GetGraphID(graphUri)
        Using c = ctx
            Dim x = <x>
DELETE FROM quad WHERE graph = {0}
DELETE FROM node WHERE id = {0} AND node.type  = 99
DELETE FROM node WHERE node.type != 99 AND NOT EXISTS (SELECT * FROM quad WHERE quad.subject=node.id OR quad.predicate=node.id OR quad.object=node.id)
                    </x>
            c.ExecuteCommand(x.Value, id)
        End Using
    End Sub

    Public Function ListGraphs() As IEnumerable(Of Uri) Implements IStorageProvider.ListGraphs
        Using c = ctxRO
            c.Log = Console.Out
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
            c.Log = Console.Out
            Dim qy1 =
                From q In c.QUADs
                Where q.graph = gid
                Join ns In c.NODEs On ns.ID Equals q.subject
                Join np In c.NODEs On np.ID Equals q.predicate
                Join no In c.NODEs On no.ID Equals q.object
            Dim qy =
                From q In qy1.ToList
                Select
                    s = BlackholeNodeFactory.Create(handler, BlackholeNodeFactory.ToNodeType(q.ns.type), q.ns.ID, q.ns.value, q.ns.metadata),
                    p = BlackholeNodeFactory.Create(handler, BlackholeNodeFactory.ToNodeType(q.np.type), q.np.ID, q.np.value, q.np.metadata),
                    o = BlackholeNodeFactory.Create(handler, BlackholeNodeFactory.ToNodeType(q.no.type), q.no.ID, q.no.value, q.no.metadata)
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
        Using tran = New TransactionScope
            DeleteGraph(g.BaseUri)
            ' Note g.Nodes does not contain predicate nodes, see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/core/Core/BaseGraph.cs#l152
            Using c = ctx
                Dim allnodes As New HashSet(Of Guid)
                For Each node In From n In c.NODEs Where n.type <> 99 Select n.ID
                    allnodes.Add(node)
                Next
                Dim nqy = From t In SPONodesDistinct(g) Select n = CreateDBNode(gid, t) Where Not allnodes.Contains(n.ID)
                c.NODEs.InsertAllOnSubmit(nqy) ' Insert all node values
                c.NODEs.InsertOnSubmit(New NODE With {.ID = gid, .type = 99, .value = If(g.BaseUri Is Nothing, Nothing, g.BaseUri.ToString)}) ' Insert graphuri as node
                Dim qy = From t In g.Triples Select q = CreateDBQuad(gid, t.Subject, t.Predicate, t.Object)
                c.QUADs.InsertAllOnSubmit(qy) ' insert all quads
                c.SubmitChanges()
            End Using
            tran.Complete()
        End Using
    End Sub

    ' Replacement for g.Nodes
    Private Shared Function SPONodesDistinct(g As IGraph) As IEnumerable(Of INode)
        Dim qs = From t In g.Triples Select t.Subject
        Dim qp = From t In g.Triples Select t.Predicate
        Dim qo = From t In g.Triples Select t.Object
        Return qs.Concat(qp).Concat(qo).Distinct
    End Function

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
            .subject = GetID(s),
            .predicate = GetID(p),
            .object = GetID(o),
            .s25p5o1type = PackTypes(s.NodeType, p.NodeType, o.NodeType)
        }
    End Function

    Private Shared Function PackTypes(snt As NodeType, pnt As NodeType, ont As NodeType) As Byte
        Const b5 As Byte = 5
        Return ((CByte(snt) * b5) + CByte(pnt)) * b5 + CByte(ont)
    End Function


    Public Sub UpdateGraph(graphUri As String, additions As IEnumerable(Of Triple), removals As IEnumerable(Of Triple)) Implements IStorageProvider.UpdateGraph
        UpdateGraph(BlackholeNodeFactory.toUri(graphUri), additions, removals)
    End Sub

    ' see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/BaseAdoStoreManager.cs#l1099
    Public Sub UpdateGraph(graphUri As Uri, additions As IEnumerable(Of Triple), removals As IEnumerable(Of Triple)) Implements IStorageProvider.UpdateGraph
        Dim gid = GetGraphID(graphUri)
        Using tran = New TransactionScope
            Using c = ctx
                Dim rems = From r In removals
                    Select q = CreateDBQuad(gid, r.Subject, r.Predicate, r.Object)
                Dim inserts =
                    From add In additions
                    Select CreateDBQuad(gid, add.Subject, add.Predicate, add.Object)
                c.QUADs.DeleteAllOnSubmit(rems)
                c.QUADs.InsertAllOnSubmit(inserts)
                c.SubmitChanges()
                If additions.Any Then
                    ' add nodes for quads that have no nodes
                    Dim missingsubjguids = From q In c.QUADs Select qid = q.subject Where Not (Aggregate n In c.NODEs Where n.ID = qid Into Any()) Select qid
                    Dim missingpredguids = From q In c.QUADs Select qid = q.predicate Where Not (Aggregate n In c.NODEs Where n.ID = qid Into Any()) Select qid
                    Dim missingobjguids = From q In c.QUADs Select qid = q.object Where Not (Aggregate n In c.NODEs Where n.ID = qid Into Any()) Select qid
                    Dim d As New Dictionary(Of System.Guid, INode)
                    For Each add In additions
                        d(GetID(add.Subject)) = add.Subject
                        d(GetID(add.Predicate)) = add.Predicate
                        d(GetID(add.Object)) = add.Object
                    Next
                    Dim missingnodes =
                        From m In missingsubjguids.Concat(missingpredguids).Concat(missingobjguids)
                        Distinct
                        Let node = d(m)
                        Select CreateDBNode(gid, node)
                    c.NODEs.InsertAllOnSubmit(missingnodes)
                    c.SubmitChanges()
                End If
                If removals.Any Then
                    ' delete all nodes without quads that reference them
                    c.ExecuteCommand("DELETE FROM node WHERE node.type != 99 AND NOT EXISTS (SELECT * FROM quad WHERE quad.subject=node.id OR quad.predicate=node.id OR quad.object=node.id)")
                End If
            End Using
            tran.Complete()
        End Using
    End Sub


#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                SyncLock Me
                    If Dataset IsNot Nothing Then DirectCast(Dataset, IDisposable).Dispose()
                End SyncLock
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
