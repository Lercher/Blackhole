Imports VDS.RDF.Query
Imports System.Web.Http
Imports System.Net.Http

Public Class UpdateController
    Inherits BlackholeBaseController

    Public Function Post(store As String, q As Update) As UpdateResult
        Dim r = New UpdateResult

        If String.IsNullOrWhiteSpace(q.update) Then
            r.result = "OK - Nothing to do"
            Return r
        End If

        Try
            Using st = New SQLStore(store)
                st.Update(q.update)
                r.result = "OK - Updated"
            End Using
        Catch ex As Exception
            r.errormessage = ex.Message
        End Try
        Return r
    End Function

    Public Function [Get]() As HttpResponseMessage
        Return AssetsController.Serve(Request, "Update.html")
    End Function

    Public Class Update
        Public Property update As String
    End Class

    Public Class UpdateResult
        Public Property errormessage As String
        Public Property result As String
    End Class

End Class
