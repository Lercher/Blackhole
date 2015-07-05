Imports VDS.RDF
Imports VDS.RDF.Storage.Virtualisation
Imports VDS.RDF.Query.Optimisation

' see https://bitbucket.org/dotnetrdf/dotnetrdf/src/df95e1283cecd046b1f6fbf6fb1d396c888dfe20/Libraries/core/net40/Query/Optimisation/VirtualAlgebraOptimiser.cs?at=default

Public Class HashingAlgebraOptimizer
    Inherits VirtualAlgebraOptimiser(Of Guid, Guid)

    Public Sub New(Store As IVirtualRdfProvider(Of Guid, Guid))
        MyBase.New(Store)
    End Sub

    Protected Overrides Function CreateVirtualNode(id As Guid, value As INode) As INode
        ' We need to construct an already materialized virtual node, because we cannot materialize
        ' literals and uris that come from an e.g. SPARQL query and are thus not stored in the database
        Return BlackholeNodeFactory.CreateVirtual(id:=id, provider:=Me._provider, preMaterializedValue:=value)
    End Function
End Class
