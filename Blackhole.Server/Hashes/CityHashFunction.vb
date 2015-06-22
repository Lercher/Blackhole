Imports System.Data.HashFunction

Public Class CityHashFunction
    Implements IHashFunction

    Private Shared ReadOnly h As New CityHash(128)

    Public Function Hash(b() As Byte, type As Byte) As Guid Implements IHashFunction.Hash
        ReDim Preserve b(0 To b.Length)
        b(b.Length) = type
        Return New Guid(h.ComputeHash(b))
    End Function
End Class
