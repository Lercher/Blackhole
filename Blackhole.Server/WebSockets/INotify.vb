Imports VDS.RDF.Query
Imports VDS.RDF.Query.Optimisation

Public Interface INotify
    Sub Notify(store As String, s As String)
    Sub InsertedNodes(store As String, proc As ISparqlQueryProcessor, opt As IAlgebraOptimiser)
End Interface
