Imports Fleck

Public Class UpdateNotificationService

    Protected allSockets As New Dictionary(Of IWebSocketConnection, CFG)
    Protected server As New WebSocketServer("ws://0.0.0.0:8091")


    Public Sub Start()
        FleckLog.Level = LogLevel.Debug
        server.Start(AddressOf Config)
    End Sub

    Protected Sub Config(socket As IWebSocketConnection)
        With socket
            .OnOpen = Sub() allSockets(socket) = New CFG With {.socket = socket}
            .OnClose = Sub() allSockets.Remove(socket)
            .OnMessage = Sub(msg) allSockets(socket).SetTrigger(msg)
        End With
    End Sub

    Public Sub [Stop]()
        server.Dispose()
    End Sub


    Public Class CFG
        Public Property socket As IWebSocketConnection
        Public Sub SetTrigger(Trigger As String)
            socket.Send("Echo: " & Trigger)
        End Sub
    End Class
End Class
