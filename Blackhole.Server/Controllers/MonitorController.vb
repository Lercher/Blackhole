Imports System.Net.Http

Public Class MonitorController
    Inherits BlackholeBaseController

    Public Function [Get]() As HttpResponseMessage
        Return AssetsController.Serve(Request, "Monitor.html")
    End Function
End Class
