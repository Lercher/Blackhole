Imports VDS.RDF.Query

Public Class BlackholeQueryController
    Inherits BlackholeBaseController

    Public Function [Get](query As String) As SparqlResultSet
        If String.IsNullOrWhiteSpace(query) Then Return Nothing

        Using st = New SQLStore
            Dim r = st.Query(query)
            Return TryCast(r, SparqlResultSet)
        End Using
    End Function
End Class
