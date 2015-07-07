Imports System.Web.Http
Imports System.Net.Http

Public Class UpdateSyntaxController
    Inherits BlackholeBaseController

    ' http://localhost:8090/blackhole/UpdateSyntax?query=insert%20data20{"hello"%20"isfor"%20"theworld"}&ref=22
    ' http://localhost:8090/blackhole/UpdateSyntax?query=insert+data+{%22hello%22+%22is+for%22+%22the+world%22}&ref=22
    Public Function [Get](<FromUri> query As String, <FromUri> Optional ref As Integer = 0) As SyntaxCheckResult
        Dim r As New SyntaxCheckResult With {.ref = ref, .checked = query}
        Try
            Dim parser = New VDS.RDF.Parsing.SparqlUpdateParser()
            Dim q = parser.ParseFromString(query)
            r.normalized = q.ToString
        Catch ex As Exception
            r.errormessage = ex.Message
        End Try
        Return r
    End Function

    Public Function Post(q As QueryRef) As SyntaxCheckResult
        Return [Get](q.query, q.ref)
    End Function

    ' http://localhost:8090/blackhole/UpdateSyntax
    Public Function [Get]() As HttpResponseMessage
        Return AssetsController.Serve(Request, "Syntax.html")
    End Function

    Public Class QueryRef
        Public Property query As String
        Public Property ref As Integer
    End Class

End Class
