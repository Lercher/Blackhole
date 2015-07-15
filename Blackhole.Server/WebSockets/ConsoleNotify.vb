Imports VDS.RDF.Query
Imports VDS.RDF.Query.Optimisation

Public Class ConsoleNotify
    Implements INotify

    Public Sub InsertedNodes(store As String, proc As ISparqlQueryProcessor, opt As IAlgebraOptimiser) Implements INotify.InsertedNodes
    End Sub

    Public Sub Notify(store As String, s As String) Implements INotify.Notify
        Console.WriteLine(store & ": " & s)
    End Sub
End Class
