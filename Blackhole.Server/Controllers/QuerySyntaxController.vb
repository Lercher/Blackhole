Imports System.Web.Http
Imports System.Net.Http

Public Class QuerySyntaxController
    Inherits BlackholeBaseController

    ' http://localhost:8090/blackhole/QuerySyntax?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}&ref=11
    ' http://localhost:8090/blackhole/QuerySyntax?query=select%20?s%20?p%20?o%20where%20I%20think%20{?s%20?p%20?o}&ref=999
    Public Function [Get](<FromUri> query As String, <FromUri> Optional ref As Integer = 0) As SyntaxCheckResult
        Dim r As New SyntaxCheckResult With {.ref = ref, .checked = query}
        Try
            Dim parser = New VDS.RDF.Parsing.SparqlQueryParser(VDS.RDF.Parsing.SparqlQuerySyntax.Extended)
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

    ' http://localhost:8090/blackhole/QuerySyntax
    Public Function [Get]() As HttpResponseMessage
        Return AssetsController.Serve(Request, "Syntax.html")
    End Function

    Public Class QueryRef
        Public Property query As String
        Public Property ref As Integer
    End Class
End Class
