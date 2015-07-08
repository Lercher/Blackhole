Imports System.Web.Http
Imports System.Web.Http.SelfHost

Public Class BlackholeService
    Implements IDisposable

    Private server As HttpSelfHostServer

    Public Function Start(hc As Topshelf.HostControl) As Boolean
        Dim cn = System.Net.Dns.GetHostEntry("localhost").HostName
        Dim config = New HttpSelfHostConfiguration(String.Format("http://{0}:8090/blackhole", cn))

        Const LINE As String = "---------------------------------------------------------------------------------"
        Console.WriteLine(LINE)
        Console.WriteLine("This is Blackhole Server, a hash based RDF triple store for SQL Server.")
        Console.WriteLine(LINE)
        Console.WriteLine("Start me with the argument help to get more help text, e.g.:")
        Console.WriteLine("  Blackhole.Server help")
        Console.WriteLine("  Blackhole.Server install --localsystem")
        Console.WriteLine("  Blackhole.Server start")
        Console.WriteLine("  Blackhole.Server stop")
        Console.WriteLine("  Blackhole.Server uninstall")
        Console.WriteLine(LINE)
        Console.WriteLine("There is an interactive UI on:")
        Console.WriteLine("  {0}", config.BaseAddress)
        Console.WriteLine(LINE)

        BlackholeBaseController.ConfigureRoutesAndHandlers(config)
        server = New HttpSelfHostServer(config)
        Try
            server.OpenAsync.Wait()
            Console.WriteLine(String.Format("Listening on: {0}", config.BaseAddress))
            If Debugger.IsAttached Then
                System.Diagnostics.Process.Start(config.BaseAddress.ToString)
            End If
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