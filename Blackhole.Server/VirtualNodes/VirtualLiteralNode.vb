Imports VDS.RDF
Imports VDS.RDF.Storage.Virtualisation


Public Class VirtualLiteralNode
    Inherits BaseVirtualLiteralNode(Of Guid, Guid)

    Public Sub New(g As IGraph, id As Guid, provider As IVirtualRdfProvider(Of Guid, Guid))
        MyBase.New(g, id, provider)
    End Sub

    Public Overrides Function CompareVirtualId(other As Guid) As Integer
        Return If(Me.VirtualID.Equals(other), 0, 1)
    End Function

    Public Overrides Function CopyNode(target As IGraph) As INode
        Return New VirtualLiteralNode(target, VirtualID, Provider)
    End Function
End Class
