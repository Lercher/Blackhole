Imports VDS.RDF
Imports VDS.RDF.Storage.Virtualisation

Public Class VirtualUriNode
    Inherits BaseVirtualUriNode(Of Guid, Guid)

    Public Sub New(g As IGraph, id As Guid, provider As IVirtualRdfProvider(Of Guid, Guid))
        MyBase.New(g, id, provider)
    End Sub

    Public Sub New(g As IGraph, id As Guid, provider As IVirtualRdfProvider(Of Guid, Guid), value As IUriNode)
        MyBase.New(g, id, provider, value)
    End Sub

    Public Overrides Function CompareVirtualId(other As Guid) As Integer
        Return If(Me.VirtualID.Equals(other), 0, 1)
    End Function

    Public Overrides Function CopyNode(target As IGraph) As INode
        If _value Is Nothing Then Return New VirtualUriNode(target, VirtualID, Provider)
        Return New VirtualUriNode(target, VirtualID, Provider, DirectCast(_value, IUriNode))
    End Function

    Public Overrides Function Equals(other As IUriNode) As Boolean
        Return MyBase.Equals(other)
    End Function

    Public Overrides Function Equals(other As IVariableNode) As Boolean
        Return MyBase.Equals(other)
    End Function
End Class
