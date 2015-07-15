Imports VDS.RDF.Query
Imports System.Web.Http
Imports System.Net.Http

Public Class QueryController
    Inherits BlackholeBaseController

    ' http://localhost:8090/blackhole/query/standard?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}
    ' http://localhost:8090/blackhole/query/standard.xml?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}
    ' http://localhost:8090/blackhole/query/standard.json?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}
    ' http://localhost:8090/blackhole/query/standard.html?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}
    ' and several types more
    Public Function [Get](store As String, <FromUri> query As String) As SparqlResultSet
        If String.IsNullOrWhiteSpace(query) Then Return Nothing

        Using st = New SQLStore(store)
            Dim r = st.Query(query)
            Dim rs = TryCast(r, SparqlResultSet)
            Return rs
        End Using
    End Function

    Public Function Post(store As String, q As Query) As SparqlResultSet
        Return [Get](store, q.query)
    End Function

    Public Function [Get]() As HttpResponseMessage
        Return AssetsController.Serve(Request, "Query.html")
    End Function

    Public Class Query
        Public Property query As String
    End Class
End Class