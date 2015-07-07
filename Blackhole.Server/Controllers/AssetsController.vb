Imports System.Net.Http
Imports System.Net.Http.Headers

Public Class AssetsController
    Inherits BlackholeBaseController

    Public Overloads Function [Get](path As String) As HttpResponseMessage
        Dim r = Serve(Request, path)
        If r.StatusCode >= 400 Then
            r.Content = New StringContent(String.Format("Error {0} - {1}", CInt(r.StatusCode), r.StatusCode))
        End If
        Return r
    End Function

    Public Shared Function Serve(ByVal Request As HttpRequestMessage, ByVal path As String) As HttpResponseMessage
        Dim r = Request.CreateResponse
        If path Is Nothing OrElse path.Length > 120 Then
            r.StatusCode = Net.HttpStatusCode.Forbidden
            Console.WriteLine("{0}: {1}", CInt(r.StatusCode), path)
            Return r
        End If
        Dim basedir = AppDomain.CurrentDomain.BaseDirectory
#If DEBUG Then
        basedir = IO.Path.Combine(basedir, "..\..")
#End If
        Dim assets = IO.Path.GetFullPath(IO.Path.Combine(basedir, "assets"))
        Dim fn = IO.Path.GetFullPath(IO.Path.Combine(assets, path))
        If Not fn.StartsWith(assets) OrElse fn.Contains("\_") Then
            r.StatusCode = Net.HttpStatusCode.Forbidden
            Console.WriteLine("{0}: {1}", CInt(r.StatusCode), path)
            Return r
        End If
        If Not IO.File.Exists(fn) Then
            r.StatusCode = Net.HttpStatusCode.NotFound
            Console.WriteLine("{0}: {1}", CInt(r.StatusCode), path)
            Return r
        End If
        Dim etag = String.Format("""{0:X}""", IO.File.GetLastWriteTimeUtc(fn).GetHashCode)
        If Request.Headers.IfNoneMatch IsNot Nothing AndAlso Request.Headers.IfNoneMatch.FirstOrDefault IsNot Nothing AndAlso etag = Request.Headers.IfNoneMatch.FirstOrDefault.Tag Then
            r.StatusCode = Net.HttpStatusCode.NotModified
            Console.WriteLine("{0}: {1} - ETAG {2}", CInt(r.StatusCode), path, etag)
            Return r
        End If
        Dim file = IO.File.Open(fn, IO.FileMode.Open, IO.FileAccess.Read, IO.FileShare.Read)
        r.Content = New StreamContent(file)
        r.Content.Headers.ContentLength = file.Length
        r.Headers.ETag = New System.Net.Http.Headers.EntityTagHeaderValue(etag)
        Dim mt = MimeTypeResolver.GetMimetypeFromExtension(IO.Path.GetExtension(fn))
        r.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(mt)
        Console.WriteLine("{0}: {1} - ETAG {2}", CInt(r.StatusCode), path, etag)
        Return r
    End Function
End Class
