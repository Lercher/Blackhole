Public Class HashGuidGenerator
    Implements IGuidGenerator

    Public HashProvider As IHashFunction
    Private Shared enc As System.Text.Encoding = System.Text.Encoding.UTF8

    Public Overridable Sub Mangle(ByRef b() As Byte)
        'hook for alternate hash
    End Sub

    Public Function fromGraphUri(uri As Uri) As Guid Implements IGuidGenerator.fromGraphUri
        If uri Is Nothing Then Return Guid.Empty
        Dim b = enc.GetBytes(uri.ToString())
        Mangle(b)
        Return HashProvider.Hash(b, 99)
    End Function

    Public Function fromNode(n As VDS.RDF.INode) As Guid Implements IGuidGenerator.fromNode
        If n Is Nothing Then Return Guid.Empty
        Dim b = enc.GetBytes(n.ToString())
        Mangle(b)
        Return HashProvider.Hash(b, CByte(n.NodeType))
    End Function
End Class
