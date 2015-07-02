Imports VDS.RDF
Imports VDS.RDF.Storage.Virtualisation

Public Module BlackholeNodeFactory
    Public Function ToNodeType(b As Byte) As NodeType
        Return CType(CInt(b), NodeType)
    End Function

    Public Function toUri(uri As String) As Uri
        If String.IsNullOrEmpty(uri) Then Return Nothing
        Return UriFactory.Create(uri)
    End Function

    Public Function CreateVirtual(g As IGraph, typ As NodeType, id As Guid, provider As IVirtualRdfProvider(Of Guid, Guid)) As INode
        Select Case typ
            Case NodeType.Blank '0
                Return New VirtualBlankNode(g, id, provider)
            Case NodeType.Uri '1
                Return New VirtualUriNode(g, id, provider)
            Case NodeType.Literal '2
                Return New VirtualLiteralNode(g, id, provider)
            Case NodeType.GraphLiteral, NodeType.Variable
                ' but see https://bitbucket.org/dotnetrdf/dotnetrdf/src/df95e1283cecd046b1f6fbf6fb1d396c888dfe20/Libraries/core/net40/Query/Optimisation/VirtualAlgebraOptimiser.cs?at=default
                Throw New ArgumentException(String.Format("CreateVirtualNode - unimplemented virtual node type: {0}", typ), "typ")
            Case Else
                Throw New ArgumentException(String.Format("CreateVirtualNode - illegal virtual node type: {0}", CInt(typ)), "typ")
        End Select
    End Function

    Public Function CreateVirtual(g As IGraph, typ As NodeType, id As Guid, provider As IVirtualRdfProvider(Of Guid, Guid), preMaterializedValue As INode) As INode
        If preMaterializedValue Is Nothing Then Return CreateVirtual(g, typ, id, provider)
        Select Case typ
            Case NodeType.Blank '0
                Return New VirtualBlankNode(g, id, provider, DirectCast(preMaterializedValue, IBlankNode))
            Case NodeType.Uri '1
                Return New VirtualUriNode(g, id, provider, DirectCast(preMaterializedValue, IUriNode))
            Case NodeType.Literal '2
                Return New VirtualLiteralNode(g, id, provider, DirectCast(preMaterializedValue, ILiteralNode))
            Case NodeType.GraphLiteral, NodeType.Variable
                ' but see https://bitbucket.org/dotnetrdf/dotnetrdf/src/df95e1283cecd046b1f6fbf6fb1d396c888dfe20/Libraries/core/net40/Query/Optimisation/VirtualAlgebraOptimiser.cs?at=default
                Throw New ArgumentException(String.Format("CreateVirtualNode - unimplemented virtual node type: {0}", typ), "typ")
            Case Else
                Throw New ArgumentException(String.Format("CreateVirtualNode - illegal virtual node type: {0}", CInt(typ)), "typ")
        End Select
    End Function

    Public Function Create(f As INodeFactory, typ As NodeType, id As Guid, value As String, meta As String) As INode
        Select Case typ
            Case NodeType.Blank '0
                Return f.CreateBlankNode(id.ToString)
            Case NodeType.Uri '1
                Return f.CreateUriNode(UriFactory.Create(value))
            Case NodeType.Literal '2
                If String.IsNullOrEmpty(meta) Then Return f.CreateLiteralNode(value)
                If meta.StartsWith("@") Then Return f.CreateLiteralNode(value, meta.Substring(1))
                Return f.CreateLiteralNode(value, UriFactory.Create(meta))
            Case Else
                Throw New ArgumentException("CreateNode - illegal node type: " & typ.ToString, "typ")
        End Select
    End Function

    Public Function DBValueOf(n As INode) As String
        If TypeOf n Is IUriNode Then
            Return DirectCast(n, IUriNode).Uri.ToString
        ElseIf TypeOf n Is ILiteralNode Then
            Return DirectCast(n, ILiteralNode).Value
        End If
        Return Nothing
    End Function

    Public Function DBMetaOf(n As INode) As String
        Dim l = TryCast(n, ILiteralNode)
        If l Is Nothing Then Return Nothing
        If Not String.IsNullOrEmpty(l.Language) Then Return "@" & l.Language
        If l.DataType IsNot Nothing Then Return l.DataType.ToString
        Return Nothing
    End Function
End Module