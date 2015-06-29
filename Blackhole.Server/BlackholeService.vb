Public Class BlackholeService
    Public Function Start(hc As Topshelf.HostControl) As Boolean
        Console.WriteLine("This is Blackhole Server, a hash based RDF triple store for SQL Server.")
        Return True
    End Function

    Public Function [Stop](hc As Topshelf.HostControl) As Boolean
        Console.WriteLine("Blackhole Service stopped.")
        Return True
    End Function
End Class