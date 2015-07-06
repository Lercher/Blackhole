Imports VDS.RDF.Query
Imports System.Web.Http

Public Class SelectSyntaxController
    Inherits BlackholeBaseController

    ' http://localhost:8090/blackhole/SelectSyntax?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}&ref=11
    ' http://localhost:8090/blackhole/SelectSyntax?query=select%20?s%20?p%20?o%20where%20I%20think%20{?s%20?p%20?o}&ref=999
    Public Function [Get](<FromUri> query As String, <FromUri> ref As Integer) As SyntaxCheckResult
        Dim r As New SyntaxCheckResult With {.ref = ref, .checked = query}
        Try
            Dim parser = New VDS.RDF.Parsing.SparqlQueryParser(VDS.RDF.Parsing.SparqlQuerySyntax.Extended)
            Dim q = parser.ParseFromString(query)
            r.normalized = q.ToString
        Catch ex As Exception
            r.erromessage = ex.Message
        End Try
        Return r
    End Function
End Class
