Imports VDS.RDF
Imports VDS.RDF.Storage.Virtualisation


Public Class BlackholeStoreVirtualGraphPersistenceWrapper
    Inherits StoreVirtualGraphPersistenceWrapper(Of Guid, Guid)

    Public Sub New(manager As VDS.RDF.Storage.IStorageProvider, provider As IVirtualRdfProvider(Of Guid, Guid), g As VDS.RDF.IGraph, graphUri As System.Uri, [writeOnly] As Boolean)
        MyBase.New(manager, provider, g, graphUri, [writeOnly])
    End Sub

    Protected Overrides Function CreateVirtual(provider As IVirtualRdfProvider(Of Guid, Guid), preMaterializedValue As INode) As INode
        Dim id = provider.GetID(preMaterializedValue)
        Return BlackholeNodeFactory.CreateVirtual(id, provider, preMaterializedValue)
    End Function

End Class