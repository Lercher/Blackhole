Imports VDS.RDF
Imports VDS.RDF.Storage.Virtualisation


Public MustInherit Class StoreVirtualGraphPersistenceWrapper(Of TNodeID, TGraphID)
    Inherits StoreGraphPersistenceWrapper

    Protected MustOverride Function CreateVirtual(provider As IVirtualRdfProvider(Of Guid, Guid), preMaterializedValue As INode) As INode

    Protected provider As IVirtualRdfProvider(Of Guid, Guid)
    Public Sub New(manager As VDS.RDF.Storage.IStorageProvider, provider As IVirtualRdfProvider(Of Guid, Guid), g As VDS.RDF.IGraph, graphUri As System.Uri, [writeOnly] As Boolean)
        MyBase.New(manager, g, graphUri, [writeOnly])
        Me.provider = provider
    End Sub

    Public Overrides Function Assert(t As Triple) As Boolean
        Return MyBase.Assert(VirtualizeTriple(t))
    End Function

    Public Overrides Function Retract(t As Triple) As Boolean
        Return MyBase.Retract(VirtualizeTriple(t))
    End Function

    Public Overrides Function ContainsTriple(t As Triple) As Boolean
        Return MyBase.ContainsTriple(VirtualizeTriple(t))
    End Function

    Protected Function VirtualizeTriple(t As Triple) As Triple
        Dim s = VirtualizeNode(t.Subject)
        Dim p = VirtualizeNode(t.Predicate)
        Dim o = VirtualizeNode(t.Object)
        If s Is t.Subject AndAlso p Is t.Predicate AndAlso o Is t.Object Then Return t
        Return New Triple(s, p, o, Me._g)
    End Function

    Protected Function VirtualizeNode(n As INode) As INode
        If TypeOf n Is IVirtualNode(Of TNodeID, TGraphID) Then Return n
        Return CreateVirtual(provider, n)
    End Function
End Class
