Imports VDS.RDF
Imports VDS.RDF.Query

Public Class Tryme
    Public Shared Sub All()
        Console.WriteLine("-----------------------------------------------------------------")
        SaveGraph()
        Console.WriteLine("-----------------------------------------------------------------")
        LoadGraphV()
        Console.WriteLine("-----------------------------------------------------------------")
        LoadGraph()
        Console.WriteLine("-----------------------------------------------------------------")
        Query()
        Console.WriteLine("-----------------------------------------------------------------")
    End Sub

    Private Shared Sub Print(ByVal rs As SparqlResultSet)
        For Each r In rs.Results
            Console.Write(" | ")
            For Each kv In r
                Console.Write(kv.Key)
                Console.Write(" -> ")
                Console.Write(kv.Value.ToString)
                Console.Write(" | ")
            Next
            Console.WriteLine()
        Next
    End Sub

    Private Shared Sub Print(ByVal g As IGraph)
        For Each t In g.Triples
            Console.WriteLine(t)
        Next
    End Sub

    Private Shared Sub SaveGraph()
        Dim g As IGraph = New Graph()
        Dim dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"))
        Dim says = g.CreateUriNode(UriFactory.Create("http://example.org/says"))
        Dim helloWorld = g.CreateLiteralNode("Hello World")
        Dim bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr")

        g.Assert(New Triple(dotNetRDF, says, helloWorld))
        g.Assert(New Triple(dotNetRDF, says, bonjourMonde))
        g.Assert(New Triple(helloWorld, helloWorld, helloWorld))
        g.Assert(New Triple(dotNetRDF, dotNetRDF, dotNetRDF))
        g.Assert(New Triple(bonjourMonde, bonjourMonde, bonjourMonde))
        Print(g)

        Dim st = New SQLStore()
        st.SaveGraph(g)
    End Sub


    Private Shared Sub LoadGraph()
        Dim st = New SQLStore()
        Dim g As IGraph = New Graph()
        st.LoadGraph(g, "")
        Print(g)
    End Sub


    Private Shared Sub LoadGraphV()
        Dim st = New SQLStore()
        Dim g As IGraph = New Graph()
        st.LoadGraphVirtual(g, Nothing)
        Print(g)
    End Sub

    Private Shared Sub Query()
        Dim st = New SQLStore()
        'Dim sp = "select $s $p $o where { $s $p $o }"
        'Dim sp = "select $s $p $o where { $s $p ""Hello World"". }"
        'Dim sp = "select $s $p $o where { $s $p ""Hello World"". $s $p $o. }"
        Dim sp = "select $s $p $o where { $s $p0 ""Hello World"". $s $p $o. }"
        Console.WriteLine(sp)
        Dim r = st.Query(sp)
        Print(TryCast(r, SparqlResultSet))
    End Sub

End Class