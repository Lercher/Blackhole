Imports System.Net.Http
Imports System.Threading


'Adapted from http://blogs.msdn.com/b/kiranchalla/archive/2012/09/04/handling-compression-accept-encoding-sample.aspx

Public Class BlackholeResponseHeaderHandler
    Inherits DelegatingHandler

    Protected Overrides Async Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        Dim response = Await MyBase.SendAsync(request, cancellationToken)
        ' CORS is done by
        '   Install-Package Microsoft.AspNet.WebApi.Cors
        ' see: http://www.asp.net/web-api/overview/security/enabling-cross-origin-requests-in-web-api
        ' and: http://www.html5rocks.com/en/tutorials/cors/
        ' for an explanation of the preflight OPTIONS request
        'Response.Headers.Add("Access-Control-Allow-Origin", "*")
        Return Response
    End Function

End Class
