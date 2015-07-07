Public Class MimeTypeResolver
    Private Shared map As IDictionary(Of String, String) = New Dictionary(Of String, String)

    Shared Sub New()
        Using k = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("MIME\Database\Content Type", writable:=False)
            For Each mime In k.GetSubKeyNames
                Using sk = k.OpenSubKey(mime)
                    Dim ext = sk.GetValue("Extension", "").ToString
                    map(ext) = mime
                End Using
            Next
        End Using
        map(".js") = "application/javascript"
        map(".html") = "text/html"
        map(".htm") = "text/html"
    End Sub

    Public Shared Function GetMimetypeFromExtension(ext As String) As String
        If Not ext.StartsWith(".") Then ext = "." & ext
        Dim r As String = String.Empty
        If map.TryGetValue(ext, r) Then Return r
        Return "application/octet-stream"
    End Function
End Class
