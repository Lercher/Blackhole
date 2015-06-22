Imports VDS.RDF.Query.Datasets

' see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/AdoDataset.cs

Public Class DS
    Inherits BaseTransactionalDataset

    Private store As SQLStore

    Public Shared Function Create(store As SQLStore) As ISparqlDataset
        Return New DS With {.store = store}
    End Function

    Protected Overrides Function AddGraphInternal(g As VDS.RDF.IGraph) As Boolean

    End Function

    Protected Overrides Function ContainsTripleInternal(t As VDS.RDF.Triple) As Boolean

    End Function

    Protected Overrides Function GetAllTriples() As IEnumerable(Of VDS.RDF.Triple)

    End Function

    Protected Overrides Function GetGraphInternal(graphUri As Uri) As VDS.RDF.IGraph

    End Function

    Protected Overrides Function GetModifiableGraphInternal(graphUri As Uri) As VDS.RDF.ITransactionalGraph

    End Function

    Protected Overrides Function GetTriplesWithObjectInternal(obj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)

    End Function

    Protected Overrides Function GetTriplesWithPredicateInternal(pred As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)

    End Function

    Protected Overrides Function GetTriplesWithPredicateObjectInternal(pred As VDS.RDF.INode, obj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)

    End Function

    Protected Overrides Function GetTriplesWithSubjectInternal(subj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)

    End Function

    Protected Overrides Function GetTriplesWithSubjectObjectInternal(subj As VDS.RDF.INode, obj As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)

    End Function

    Protected Overrides Function GetTriplesWithSubjectPredicateInternal(subj As VDS.RDF.INode, pred As VDS.RDF.INode) As IEnumerable(Of VDS.RDF.Triple)

    End Function

    Public Overrides ReadOnly Property Graphs As IEnumerable(Of VDS.RDF.IGraph)
        Get

        End Get
    End Property

    Public Overrides ReadOnly Property GraphUris As IEnumerable(Of Uri)
        Get

        End Get
    End Property

    Protected Overrides Function HasGraphInternal(graphUri As Uri) As Boolean

    End Function

    Protected Overrides Function RemoveGraphInternal(graphUri As Uri) As Boolean

    End Function
End Class
