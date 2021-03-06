﻿Imports VDS.RDF
Imports VDS.RDF.Query.Datasets
Imports VDS.RDF.Storage
Imports VDS.RDF.Writing

' see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/AdoDataset.cs

Public Class DS
    Inherits BaseTransactionalDataset
    Implements IDisposable

    Private store As SQLStore
    Private ctx As BlackholeDBDataContext
    Private factory As New GraphFactory
    Private graphUriDictionary As IDictionary(Of Guid, Uri)

    Protected Property UseVirtualization As Boolean = True

    Public Shared Function Create(store As SQLStore, ctx As BlackholeDBDataContext, Virtualizing As Boolean) As ISparqlDataset
        Dim ds As DS = New DS With {.store = store, .ctx = ctx, .UseVirtualization = Virtualizing}
        ds.ctx.ObjectTrackingEnabled = False
        store.RecycleCtx()
        Return ds
    End Function

    Public Sub New()
        ' We separate our default graph from it's named graphs
        MyBase.New(unionDefaultGraph:=False)
    End Sub

    Protected Function GraphExists(gid As System.Guid) As Boolean
        Return Aggregate n In ctx.NODEs Where n.ID = gid And n.type = 99 Into Any()
    End Function

    Protected Function HasQUAD(g As Guid, s As Guid, p As Guid, o As Guid) As Boolean
        Return Aggregate q In ctx.QUADs Where q.graph = g And q.subject = s And q.predicate = p And q.object = o Into Any()
    End Function

    Public Shared Function Unpack(q As QUAD) As Tuple(Of NodeType, NodeType, NodeType)
        Dim snt As NodeType, pnt As NodeType, ont As NodeType
        Dim a = CInt(q.s25p5o1type)
        Dim o = CByte(a Mod 5) : a = a \ 5
        Dim p = CByte(a Mod 5) : a = a \ 5
        Dim s = CByte(a)
        snt = BlackholeNodeFactory.ToNodeType(s)
        pnt = BlackholeNodeFactory.ToNodeType(p)
        ont = BlackholeNodeFactory.ToNodeType(o)
        Return Tuple.Create(snt, pnt, ont)
    End Function

    ' ------------- Overrides

    Protected Overrides Function AddGraphInternal(g As VDS.RDF.IGraph) As Boolean
        store.SaveGraph(g)
        Return True
    End Function

    Protected Overrides Function RemoveGraphInternal(graphUri As Uri) As Boolean
        store.DeleteGraph(graphUri)
        Return True
    End Function

    Protected Overrides Function HasGraphInternal(graphUri As Uri) As Boolean
        Dim gid = store.GetGraphID(graphUri)
        Return GraphExists(gid)
    End Function

    Protected Overrides Function GetGraphInternal(graphUri As Uri) As VDS.RDF.IGraph
        Dim created As Boolean
        Dim g = factory.TryGetGraph(graphUri, created)
        If created OrElse g.IsEmpty Then
            If UseVirtualization Then
                store.LoadGraphVirtual(g, graphUri)
            Else
                store.LoadGraph(g, graphUri)
            End If
        End If
        'Tryme.Print(g) 
        Return g
    End Function

    Protected Overrides Function ContainsTripleInternal(t As VDS.RDF.Triple) As Boolean
        Dim s = store.GetID(t.Subject)
        Dim p = store.GetID(t.Predicate)
        Dim o = store.GetID(t.Object)
        Dim g = store.GetGraphID(t.GraphUri)
        Return HasQUAD(g, s, p, o)
    End Function

    Private Sub LoadGraphUriDictionary()
        SyncLock Me
            If graphUriDictionary Is Nothing Then
                graphUriDictionary = New Dictionary(Of Guid, Uri)
                Dim qy = From n In ctx.NODEs Where n.type = 99
                For Each node In qy
                    If Not graphUriDictionary.ContainsKey(node.ID) Then
                        graphUriDictionary.Add(node.ID, BlackholeNodeFactory.toUri(node.value))
                    End If
                Next
            End If
        End SyncLock
    End Sub

    Protected Function Query(filter As Expressions.Expression(Of Func(Of QUAD, Boolean)), subj As VDS.RDF.INode, pred As VDS.RDF.INode, obj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)
        Dim subjf = Function(g As IGraph, q As QUAD) BlackholeNodeFactory.CreateVirtual(g, Unpack(q).Item1, q.subject, store)
        Dim predf = Function(g As IGraph, q As QUAD) BlackholeNodeFactory.CreateVirtual(g, Unpack(q).Item2, q.predicate, store)
        Dim objf = Function(g As IGraph, q As QUAD) BlackholeNodeFactory.CreateVirtual(g, Unpack(q).Item3, q.object, store)
        Dim fixedsubjf = Function(g As IGraph, q As QUAD) Tools.CopyNode(subj, g)
        Dim fixedpredf = Function(g As IGraph, q As QUAD) Tools.CopyNode(pred, g)
        Dim fixedobjf = Function(g As IGraph, q As QUAD) Tools.CopyNode(obj, g)

        LoadGraphUriDictionary()
        Dim qy = _
            From q In ctx.QUADs.Where(filter)
            Let
                g = factory(graphUriDictionary(q.graph)),
                sf = If(subj Is Nothing, subjf, fixedsubjf),
                pf = If(pred Is Nothing, predf, fixedpredf),
                obf = If(obj Is Nothing, objf, fixedobjf)
            Select
                s = sf(g, q),
                p = pf(g, q),
                o = obf(g, q)
            Select
                New Triple(s, p, o)
        Return qy
    End Function

    Protected Overrides Function GetAllTriples() As IEnumerable(Of VDS.RDF.Triple)
        Return Query(Function(q) True, Nothing, Nothing, Nothing)
    End Function

    Protected Overrides Function GetTriplesWithSubjectInternal(subj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)
        Dim sid = store.GetID(subj)
        Return Query(Function(q) q.subject = sid, subj, Nothing, Nothing)
    End Function

    Protected Overrides Function GetTriplesWithPredicateInternal(pred As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)
        Dim pid = store.GetID(pred)
        Return Query(Function(q) q.predicate = pid, Nothing, pred, Nothing)
    End Function

    Protected Overrides Function GetTriplesWithObjectInternal(obj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)
        Dim oid = store.GetID(obj)
        Return Query(Function(q) q.object = oid, Nothing, Nothing, obj)
    End Function

    Protected Overrides Function GetTriplesWithPredicateObjectInternal(pred As VDS.RDF.INode, obj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)
        Dim pid = store.GetID(pred)
        Dim oid = store.GetID(obj)
        Return Query(Function(q) q.predicate = pid And q.object = oid, Nothing, pred, obj)
    End Function


    Protected Overrides Function GetTriplesWithSubjectObjectInternal(subj As VDS.RDF.INode, obj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)
        Dim sid = store.GetID(subj)
        Dim oid = store.GetID(obj)
        Return Query(Function(q) q.subject = sid And q.object = oid, subj, Nothing, obj)
    End Function

    Protected Overrides Function GetTriplesWithSubjectPredicateInternal(subj As VDS.RDF.INode, pred As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)
        Dim sid = store.GetID(subj)
        Dim pid = store.GetID(pred)
        Return Query(Function(q) q.subject = sid And q.predicate = pid, subj, pred, Nothing)
    End Function

    Public Overrides ReadOnly Property Graphs As IEnumerable(Of VDS.RDF.IGraph)
        Get
            Return From u In GraphUris Select Me(u)
        End Get
    End Property

    Public Overrides ReadOnly Property GraphUris As IEnumerable(Of Uri)
        Get
            Dim qy = From n In ctx.NODEs Where n.type = 99 Select BlackholeNodeFactory.toUri(n.value)
            Return qy
        End Get
    End Property


    Protected Overrides Function GetModifiableGraphInternal(graphUri As Uri) As VDS.RDF.ITransactionalGraph
        Dim g = GetGraphInternal(graphUri)
        Return New BlackholeStoreVirtualGraphPersistenceWrapper(store, store, g, graphUri, [writeOnly]:=False)
    End Function


#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                ctx.Dispose()
                ctx = Nothing
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


    Protected Overrides Sub FlushInternal()
        factory.Reset()
    End Sub

    Protected Overrides Sub DiscardInternal()
        factory.Reset()
    End Sub


End Class
