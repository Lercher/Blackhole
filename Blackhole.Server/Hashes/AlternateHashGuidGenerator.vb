
Public Class AlternateHashGuidGenerator
    Inherits HashGuidGenerator

    Public Overrides Sub Mangle(ByRef b() As Byte)
        For i = 0 To b.Length - 3 Step 3
            Dim bb = b(i + 2)
            b(i + 2) = b(i + 1)
            b(i + 1) = b(i)
            b(i) = bb
        Next
    End Sub
End Class
