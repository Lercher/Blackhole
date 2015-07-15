Imports Fleck
Imports VDS.RDF.Query
Imports VDS.RDF.Query.Optimisation

Public Class UpdateNotificationService
    Implements INotify

    Public Shared ReadOnly Instance As UpdateNotificationService = New UpdateNotificationService

    Protected allSockets As New Dictionary(Of IWebSocketConnection, WebSocketNotifier)
    Protected server As WebSocketServer

    Private Sub New()
    End Sub

    Public Sub Start()
        FleckLog.Level = LogLevel.Debug
        server = New WebSocketServer("ws://0.0.0.0:8091")
        server.Start(AddressOf Config)
    End Sub

    Protected Sub Config(socket As IWebSocketConnection)
        With socket
            .OnOpen = Sub() allSockets(socket) = New WebSocketNotifier(socket)
            .OnClose = Sub() allSockets.Remove(socket)
            .OnMessage = Sub(msg) allSockets(socket).SetTrigger(msg)
        End With
    End Sub

    Public Sub [Stop]()
        Dim ss = allSockets.Keys
        allSockets.Clear()
        For Each s In ss
            s.Close()
        Next
        server.Dispose()
        server = Nothing
    End Sub

    Public Sub InsertedNodes(store As String, proc As ISparqlQueryProcessor, opt As IAlgebraOptimiser) Implements INotify.InsertedNodes
        For Each v In allSockets.Values
            v.InsertedNodes(store, proc, opt)
        Next
    End Sub

    Public Sub Notify(store As String, s As String) Implements INotify.Notify
        For Each v In allSockets.Values
            v.Notify(store, s)
        Next
    End Sub


End Class