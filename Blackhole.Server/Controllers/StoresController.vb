Imports System.Web.Http
Imports System.Net.Http

Public Class StoresController
    Inherits BlackholeBaseController

    ' http://localhost:8090/blackhole/stores
    Public Function [Get]() As IEnumerable(Of String)
        Using sm = New SQLStoreManagement
            Return sm.ListStores.ToArray
        End Using
    End Function

    Public Function Put(st As Store) As String
        Using sm = New SQLStoreManagement
            Try
                sm.CreateStore(sm.GetDefaultTemplate(st.store))
                Return String.Format("OK - store '{0}' created.", st.store)
            Catch ex As Exception
                Return ex.Message
            End Try
        End Using
    End Function

    Public Function Delete(<FromUri> store As String) As String
        Using sm = New SQLStoreManagement
            Try
                sm.DeleteStore(store)
                Return String.Format("OK - store '{0}' deleted.", store)
            Catch ex As Exception
                Return ex.Message
            End Try
        End Using
    End Function

    Public Class Store
        Public Property store As String
    End Class
End Class
