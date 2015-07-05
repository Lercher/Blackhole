Imports System.Web.Http
Imports System.Web.Http.SelfHost

Public Class BlackholeService
    Implements IDisposable

    Private server As HttpSelfHostServer

    Public Function Start(hc As Topshelf.HostControl) As Boolean
        Console.WriteLine("This is Blackhole Server, a hash based RDF triple store for SQL Server.")

        Dim cn = System.Net.Dns.GetHostEntry("localhost").HostName
        Dim config = New HttpSelfHostConfiguration(String.Format("http://{0}:8090/blackhole", cn))

        ConfigureRoutesAndHandlers(config)

        server = New HttpSelfHostServer(config)
        Try
            server.OpenAsync.Wait()
            Console.WriteLine(String.Format("Listening on: {0}", config.BaseAddress))
            Return True
        Catch ex As Exception
            Throw
        End Try
    End Function

    Public Function [Stop](hc As Topshelf.HostControl) As Boolean
        Dispose()
        Console.WriteLine("Blackhole Service stopped.")
        Return True
    End Function



    Private Sub ConfigureRoutesAndHandlers(config As System.Web.Http.HttpConfiguration)
        config.Routes.MapHttpRoute(
            "Blackhole with Extension",
            "{controller}.{ext}",
            New With {.controller = "Home", .ext = "json"}
        )

        config.Routes.MapHttpRoute(
            "Blackhole",
            "{controller}",
            New With {.controller = "Home", .ext = "json"}
        )

        config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always 'TODO: Should be configurable

        config.Formatters.Add(New SparqlResultsetFormatter)
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                If Not server Is Nothing Then
                    server.CloseAsync.Wait()
                    server.Dispose()
                    server = Nothing
                End If
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class