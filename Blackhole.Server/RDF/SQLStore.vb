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

#Const DONTNETRDF_SUPPORTS_UPDATING_VIRTUALIZEDGRAPHS = True

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
    Private ReadOnly AlternateGuidGenerator As IGuidGenerator
    Private Dataset As Query.Datasets.ISparqlDataset 'stores a context and needs to be disposed of
    Private ctx As BlackholeDBDataContext

    Public Sub New()
        GuidGenerator = New HashGuidGenerator With {.HashProvider = New CityHashFunction}
        AlternateGuidGenerator = New AlternateHashGuidGenerator With {.HashProvider = New CityHashFunction}
        ctx = New BlackholeDBDataContext()
    End Sub


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
        Return Aggregate n In ctx.NODEs Where n.ID = id Select BlackholeNodeFactory.toUri(n.value) Into FirstOrDefault()
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
        Dim n =
                Aggregate q In ctx.NODEs
                Where q.ID = id
                Into FirstOrDefault()
        If n IsNot Nothing Then Return BlackholeNodeFactory.Create(g, BlackholeNodeFactory.ToNodeType(n.type), id, n.value, n.metadata)
        Return g.CreateBlankNode(id.ToString)
    End Function

    Public Sub LoadGraphVirtual(g As IGraph, graphUri As Uri) Implements IVirtualRdfProvider(Of Guid, Guid).LoadGraphVirtual
        'LoadGraph(g, graphUri) : Return
        ' works with:  LoadGraph(g, graphUri):Return ' but not with my virtual nodes as follows
        Dim gid = GetGraphID(graphUri)
        g.BaseUri = graphUri
        Dim qy =
                From q In ctx.QUADs
                Where q.graph = gid
                Select
                    s = BlackholeNodeFactory.CreateVirtual(g, DS.Unpack(q).Item1, q.subject, Me),
                    p = BlackholeNodeFactory.CreateVirtual(g, DS.Unpack(q).Item2, q.predicate, Me),
                    o = BlackholeNodeFactory.CreateVirtual(g, DS.Unpack(q).Item3, q.object, Me)
                Select New Triple(s, p, o)
        'Console.WriteLine("Asserting virtual query:")
        g.Assert(qy)
        'Console.WriteLine("Virtual Query asserted")
    End Sub

    Public ReadOnly Property NullID As Guid Implements IVirtualRdfProvider(Of Guid, Guid).NullID
        Get
            Return Guid.Empty
        End Get
    End Property


    ' ---------------------------------------------- IQueryableStorage -----------------------------------------------------------

    ' see https://bitbucket.org/dotnetrdf/dotnetrdf/src/df95e1283cecd046b1f6fbf6fb1d396c888dfe20/Libraries/core/net40/Web/BaseSparqlServer.cs?at=default
    Public Sub Query(rdfHandler As IRdfHandler, resultsHandler As ISparqlResultsHandler, sparqlQuery As String) Implements IQueryableStorage.Query
        Dim Parser As New SparqlQueryParser(SparqlQuerySyntax.Extended)
        'P.ExpressionFactories = ...
        'P.QueryOptimiser = ...
        Dim Query = Parser.ParseFromString(sparqlQuery)
        Query.AlgebraOptimisers = New IAlgebraOptimiser() {New HashingAlgebraOptimizer(Me)} : Dim dataset = DS.CreateVirtualizing(Me, ctx)
        Dim Processor As New LeviathanQueryProcessor(dataset)
        Processor.ProcessQuery(rdfHandler, resultsHandler, Query)
    End Sub

    Public Function Query(sparqlQuery As String) As Object Implements IQueryableStorage.Query
        Dim g As New Graph
        Dim results = New SparqlResultSet
        Query(New GraphHandler(g), New ResultSetHandler(results), sparqlQuery)
        If results.ResultsType = SparqlResultsType.Unknown Then Return g
        Return results
    End Function



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
        Dim x = <x>
DELETE FROM quad WHERE graph = {0}
DELETE FROM node WHERE id = {0} AND node.type  = 99
DELETE FROM node WHERE node.type != 99 AND NOT EXISTS (SELECT * FROM quad WHERE quad.subject=node.id OR quad.predicate=node.id OR quad.object=node.id)
                    </x>
        ctx.ExecuteCommand(x.Value, id)
    End Sub

    Public Function ListGraphs() As IEnumerable(Of Uri) Implements IStorageProvider.ListGraphs
        Dim qy = From n In ctx.NODEs Where n.type = 99 Select u = n.value Distinct Select BlackholeNodeFactory.toUri(u)
        Return qy.ToArray
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
        Dim qy1 =
                From q In ctx.QUADs
                Where q.graph = gid
                Join ns In ctx.NODEs On ns.ID Equals q.subject
                Join np In ctx.NODEs On np.ID Equals q.predicate
                Join no In ctx.NODEs On no.ID Equals q.object
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
    End Sub

    Public ReadOnly Property ParentServer As Management.IStorageServer Implements IStorageProvider.ParentServer
        Get
            Return Nothing
        End Get
    End Property

    Friend Shared Sub DetectCollisions(ctx As BlackholeDBDataContext)
        Dim qy = From n In ctx.NODEs Group By ID = n.ID Into alternates = Group, Count() Where Count > 1
        If qy.Any Then
            Dim list As New List(Of String)
            For Each n In qy
                list.Add(String.Format("'{0}' collides with:", n.ID))
                For Each nn In n.alternates
                    list.Add(String.Format("'{0}/{1}'", nn.value, nn.metadata))
                Next
            Next
            Dim msg = "Hash collision detected, which is not supported by this store. Choose different values: " & Join(list.ToArray, " ")
            Throw New RdfStorageException(msg)
        End If
    End Sub

    Public Sub SaveGraph(g As IGraph) Implements IStorageProvider.SaveGraph
        Dim gid = GetGraphID(g)
        Using tran = New TransactionScope
            DeleteGraph(g.BaseUri)

            'we can have identical node values from graphs saved with different uris, so we load them.
            Dim allnodes As New HashSet(Of Guid)
            For Each node In From n In ctx.NODEs Where n.type <> 99 Select n.ID
                allnodes.Add(node)
            Next
            ' Note g.Nodes does not contain predicate nodes, see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/core/Core/BaseGraph.cs#l152
            Dim nqy = From t In SPONodesDistinct(g) Select n = CreateDBNode(gid, t) Where Not allnodes.Contains(n.ID)
            ctx.NODEs.InsertAllOnSubmit(nqy) ' Insert all node values
            ctx.SubmitChanges()
            DetectCollisions(ctx)
            ctx.NODEs.InsertOnSubmit(New NODE With {.ID = gid, .type = 99, .value = If(g.BaseUri Is Nothing, Nothing, g.BaseUri.ToString)}) ' Insert graphuri as node
            Dim qy = From t In g.Triples Select q = CreateDBQuad(gid, t.Subject, t.Predicate, t.Object)
            ctx.QUADs.InsertAllOnSubmit(qy) ' insert all quads
            ctx.SubmitChanges()
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
            .AlternateID = AlternateGuidGenerator.fromNode(n),
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


    ' ---------------------------------------------- IUpdateableStorage -----------------------------------------------------------

    Public Sub Update(sparqlUpdate As String) Implements IUpdateableStorage.Update
        Dim Updateparser As New SparqlUpdateParser
        Dim cmds = Updateparser.ParseFromString(sparqlUpdate)
#If DONTNETRDF_SUPPORTS_UPDATING_VIRTUALIZEDGRAPHS Then
        cmds.AlgebraOptimisers = New IAlgebraOptimiser() {New HashingAlgebraOptimizer(Me)}
        Dim dataset = DS.CreateVirtualizing(Me, ctx)
#Else
        Dim dataset = DS.CreateNonVirtualizing(Me, ctx)
#End If
        Dim Processor As New LeviathanUpdateProcessor(dataset)
        Using tran = New TransactionScope
            Processor.ProcessCommandSet(cmds)
            tran.Complete()
        End Using
    End Sub

    Private Shared Function isEmpty(ts As IEnumerable(Of Triple)) As Boolean
        If ts Is Nothing Then Return True
        Return Not ts.Any
    End Function

    Public Sub UpdateGraph(graphUri As String, additions As IEnumerable(Of Triple), removals As IEnumerable(Of Triple)) Implements IStorageProvider.UpdateGraph
        UpdateGraph(BlackholeNodeFactory.toUri(graphUri), additions, removals)
    End Sub

    ' see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/BaseAdoStoreManager.cs#l1099
    Public Sub UpdateGraph(graphUri As Uri, additions As IEnumerable(Of Triple), removals As IEnumerable(Of Triple)) Implements IStorageProvider.UpdateGraph
        If isEmpty(additions) AndAlso isEmpty(removals) Then Return
        Dim gid = GetGraphID(graphUri)
        Dim tempgid = Guid.NewGuid
        If Not isEmpty(removals) Then
            Dim rems = From r In removals
                Select q = CreateDBQuad(gid, r.Subject, r.Predicate, r.Object)
            For Each q In rems
                Dim x = ctx.ExecuteCommand("DELETE quad WHERE graph={0} AND subject={1} AND predicate={2} AND object={3}", q.graph, q.subject, q.predicate, q.object)
                Debug.Assert(x = 1, "Exactly one quad should be deleted")
            Next
        End If
        If Not isEmpty(additions) Then
            ' with a temporary graph id any insert succedes
            ' we delete all duplicates afterwards and re-id the inserts
            Dim inserts =
                From add In additions
                Select CreateDBQuad(tempgid, add.Subject, add.Predicate, add.Object)
            ctx.QUADs.InsertAllOnSubmit(inserts) 'if it bombs, then there are hash collisions in quads which is not very probable, if it is a good hash function
        End If
        ctx.SubmitChanges()
        If Not isEmpty(additions) Then
            ' delete duplicates and give the inserted ones the proper gid:
            ctx.ExecuteCommand("DELETE i FROM quad i INNER JOIN quad q ON q.subject=i.subject AND q.predicate=i.predicate AND q.object=i.object WHERE q.graph={0} AND i.graph={1}", gid, tempgid)
            ' add nodes for quads that have no nodes
            Dim missingsubjguids = From q In ctx.QUADs Where q.graph = tempgid Select qid = q.subject Where Not (Aggregate n In ctx.NODEs Where n.ID = qid Into Any()) Select qid
            Dim missingpredguids = From q In ctx.QUADs Where q.graph = tempgid Select qid = q.predicate Where Not (Aggregate n In ctx.NODEs Where n.ID = qid Into Any()) Select qid
            Dim missingobjguids = From q In ctx.QUADs Where q.graph = tempgid Select qid = q.object Where Not (Aggregate n In ctx.NODEs Where n.ID = qid Into Any()) Select qid
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
            ctx.NODEs.InsertAllOnSubmit(missingnodes)
            ctx.SubmitChanges()
            DetectCollisions(ctx)
            ctx.ExecuteCommand("UPDATE quad SET graph={0} WHERE graph={1}", gid, tempgid)
        End If
        If Not isEmpty(removals) Then
            ' delete all nodes without quads that reference them
            ctx.ExecuteCommand("DELETE FROM node WHERE node.type != 99 AND NOT EXISTS (SELECT * FROM quad WHERE quad.subject=node.id OR quad.predicate=node.id OR quad.object=node.id)")
        End If
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
