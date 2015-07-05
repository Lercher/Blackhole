Imports VDS.RDF.Query
Imports System.Web.Http

Public Class QueryController
    Inherits BlackholeBaseController

    ' http://localhost:8090/blackhole/query?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}
    ' http://localhost:8090/blackhole/query.xml?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}
    ' http://localhost:8090/blackhole/query.json?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}
    ' http://localhost:8090/blackhole/query.html?query=select%20?s%20?p%20?o%20where%20{?s%20?p%20?o}
    ' and several types more
    Public Function [Get](<FromUri> query As String) As SparqlResultSet
        If String.IsNullOrWhiteSpace(query) Then Return Nothing

        Using st = New SQLStore
            Dim r = st.Query(query)
            Dim rs = TryCast(r, SparqlResultSet)
            Return rs
        End Using
    End Function
End Class
