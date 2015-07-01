Imports VDS.RDF
Imports VDS.RDF.Storage.Virtualisation


Public Class VirtualLiteralNode
    Inherits BaseVirtualLiteralNode(Of Guid, Guid)

    Public Sub New(g As IGraph, id As Guid, provider As IVirtualRdfProvider(Of Guid, Guid))
        MyBase.New(g, id, provider)
    End Sub

    Public Sub New(g As IGraph, id As Guid, provider As IVirtualRdfProvider(Of Guid, Guid), value As ILiteralNode)
        MyBase.New(g, id, provider, value)
    End Sub

    Public Overrides Function CompareVirtualId(other As Guid) As Integer
        Return If(Me.VirtualID.Equals(other), 0, 1)
    End Function

    Public Overrides Function CopyNode(target As IGraph) As INode
        If _value Is Nothing Then Return New VirtualLiteralNode(target, VirtualID, Provider)
        Return New VirtualLiteralNode(target, VirtualID, Provider, DirectCast(_value, ILiteralNode))
    End Function

End Class
