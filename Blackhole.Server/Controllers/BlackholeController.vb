Imports System.Web.Http
Imports System.Net.Http

Public Class BlackholeController
    Inherits BlackholeBaseController

    ' http://localhost:8090/blackhole
    Public Function [Get]() As HttpResponseMessage
        Return AssetsController.Serve(Request, "Home.html")
    End Function
End Class
