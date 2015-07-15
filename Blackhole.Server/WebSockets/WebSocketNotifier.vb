Imports Fleck
Imports VDS.RDF
Imports VDS.RDF.Parsing
Imports VDS.RDF.Query
Imports VDS.RDF.Query.Optimisation

Public Class WebSocketNotifier
    Implements INotify

    Private Q As SparqlQuery = Nothing
    Public ReadOnly Socket As IWebSocketConnection
    Public ReadOnly MyStore As String
    Public Property MyTrigger As String = Nothing

    Public Sub New(socket As IWebSocketConnection)
        Me.Socket = socket
        MyStore = socket.ConnectionInfo.Path.Replace("/", "")
        Notify(MyStore, MyStore)
    End Sub

    Public Sub SetTrigger(Trigger As String)
        MyTrigger = Nothing
        Q = Nothing
        Try
            Dim p As New SparqlQueryParser(SparqlQuerySyntax.Extended)
            Q = p.ParseFromString(Trigger)
            Dim nq = Q.ToString
            If Q.QueryType <> SparqlQueryType.Ask Then Throw New ApplicationException("Not an ASK query, but " & Q.QueryType.ToString)
            Notify(MyStore, nq)
            MyTrigger = nq
        Catch ex As Exception
            Notify(MyStore, ex.Message)
        End Try
    End Sub

    Public Sub InsertedNodes(store As String, proc As ISparqlQueryProcessor, opt As IAlgebraOptimiser) Implements INotify.InsertedNodes
        If Q Is Nothing Then Exit Sub
        If Not isMatch(store) Then Return
        Q.AlgebraOptimisers = New IAlgebraOptimiser() {opt}
        Dim result = TryCast(proc.ProcessQuery(Q), SparqlResultSet)
        Dim ok = result.Result
        If ok Then Socket.Send(MyTrigger)
    End Sub

    Public Sub Notify(store As String, s As String) Implements INotify.Notify
        If Not String.IsNullOrWhiteSpace(MyTrigger) Then Return
        If Not isMatch(store) Then Return
        Socket.Send(String.Format("[{0}]", s))
    End Sub

    Private Function isMatch(store As String) As Boolean
        Return StringComparer.InvariantCultureIgnoreCase.Compare(MyStore, store) = 0
    End Function


End Class
