Imports VDS.RDF
Imports VDS.RDF.Query
Imports VDS.RDF.Parsing
Imports VDS.RDF.Query.Datasets

Public Class Tryme
    Public Shared Sub All()
        Console.WriteLine("-----------------------------------------------------------------")
        DeleteData()
        InsertData()
        'SaveGraph()
        'SaveBigGraph(100)
        'SaveBigGraph(500)
        Console.WriteLine("-----------------------------------------------------------------")
        LoadGraphV()
        Console.WriteLine("-----------------------------------------------------------------")
        LoadGraph()
        Console.WriteLine("-----------------------------------------------------------------")
        LoadAndQuery()
        Query()
        Console.WriteLine("-----------------------------------------------------------------")
    End Sub

    Private Shared Sub DeleteData()
        Using st = New SQLStore
            st.Update("DELETE DATA { <http://example.org> <http://example.org/says> ""Another one hits the dust."" };")
            'st.Update("DELETE { <http://example.org> <http://example.org/says> ""Another one hits the dust."" } where { <http://example.org> <http://example.org/says> ""Another one hits the dust."" };")
        End Using
        Using st = New SQLStore
            Dim g As IGraph = New Graph()
            st.LoadGraph(g, "")
            Print(g)
        End Using
    End Sub

    Private Shared Sub InsertData()
        Using st = New SQLStore
            st.Update("INSERT DATA {<http://example.org> <http://example.org/says> ""Another one hits the dust.""; <http://example.org/says> ""Another one hits the dust again""; <http://example.org/says> ""Another one hits the dust for the third time"".}")
        End Using
        Using st = New SQLStore
            Dim g As IGraph = New Graph()
            st.LoadGraph(g, "")
            Print(g)
        End Using
    End Sub


    Private Shared Sub LoadAndQuery()
        Dim g As IGraph = New Graph()
        Using st = New SQLStore()
            st.LoadGraph(g, "http://example.org/graph#Big100")
            'st.LoadGraph(g, "")
            'Print(g)
        End Using
        'Dim g = CreateBigGraph(100)
        Dim Parser = New SparqlQueryParser(SparqlQuerySyntax.Extended)
        Dim sp = "select  $s $o where { $s <http://example.org/says#12> $o.}"
        'Dim sp = "select $s $p $o where { $s $p ""Hello World #69"". }"
        'Dim sp = "select  $s $p $o where { $s $p $o }"
        Console.WriteLine(sp)
        Dim Query = Parser.ParseFromString(sp)
        Dim store = New TripleStore()
        store.Add(g)
        Dim dataset = New InMemoryDataset(store, g.BaseUri)
        Dim Processor = New LeviathanQueryProcessor(dataset)
        Dim rs = Processor.ProcessQuery(Query)
        Print(TryCast(rs, SparqlResultSet))
    End Sub

    Private Shared Sub Query()
        Using st = New SQLStore()
            'Dim sp = "select $s $p $o where { $s $p $o }"
            'Dim sp = "select $s $p $o where { $s $p ""Hello World"". }"
            'Dim sp = "select $s $p $o where { $s <http://example.org/says> $o}"
            'Dim sp = "select $s $p $o where { $s $p ""Hello World"". $s $p $o. }"
            'Dim sp = "select $s $p $o where { $s $p0 ""Hello World"". $s $p $o. }"
            'Dim sp = "select  $s $o where { $s <http://example.org/says#12> $o.}"
            'Dim sp = "select  $s $o where { Graph <http://example.org/graph#Big100> { $s <http://example.org/says#12> $o.} }"
            'Dim sp = "select  $s $p $o from named <http://example.org/graph#Big100> where { GRAPH <http://example.org/graph#Big100> { $s $p $o. }}"
            'Dim sp = "select  $s $o from named <http://example.org/graph#Big100> where { GRAPH <http://example.org/graph#Big100> { $s <http://example.org/says#12> $o.} }"
            Dim sp = "select  $s ?g $o from named <http://example.org/graph#Big100> where { GRAPH ?g { $s <http://example.org/says#12> $o.} }"
            Console.WriteLine(sp)
            Dim r = st.Query(sp)
            Print(TryCast(r, SparqlResultSet))
        End Using
    End Sub


    Private Shared Sub Print(ByVal rs As SparqlResultSet)
        Dim n = 0
        For Each r In rs.Results
            Console.Write(" | ")
            For Each kv In r
                Console.Write(kv.Key)
                Console.Write(" -> ")
                Console.Write(kv.Value.ToString)
                Console.Write(" | ")
            Next
            Console.WriteLine()
            n += 1
        Next
        Console.WriteLine("---------- {0:n0} line(s) ------------------------", n)
    End Sub

    Public Shared Sub Print(ByVal g As IGraph)
        Dim n = 0
        For Each t In g.Triples
            Console.WriteLine(t)
            n += 1
        Next
        Console.WriteLine("---------- {0:n0} triple(s) ------------------------", n)
    End Sub

    Private Shared Function CreateBigGraph(ByVal n As Integer) As IGraph
        Dim g As IGraph = New Graph()
        g.BaseUri = UriFactory.Create("http://example.org/graph#Big" & n.ToString)
        For i = 1 To n
            Dim s = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org#" & (i Mod 10).ToString))
            Dim p = g.CreateUriNode(UriFactory.Create("http://example.org/says#" & (i Mod 13).ToString))
            Dim o = g.CreateLiteralNode("Hello World #" & i.ToString)
            Dim t = New Triple(s, p, o)
            g.Assert(t)
        Next
        Return g
    End Function

    Private Shared Sub SaveBigGraph(n As Integer)
        Dim g As IGraph = CreateBigGraph(n)
        Print(g)

        Using st = New SQLStore()
            st.SaveGraph(g)
        End Using
    End Sub

    Private Shared Sub SaveGraph()
        Dim g As IGraph = New Graph()
        Dim dotNetRDF = g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org"))
        Dim says = g.CreateUriNode(UriFactory.Create("http://example.org/says"))
        Dim says12 = g.CreateUriNode(UriFactory.Create("http://example.org/says#12"))
        Dim helloWorld = g.CreateLiteralNode("Hello World")
        Dim bonjourMonde = g.CreateLiteralNode("Bonjour tout le Monde", "fr")

        g.Assert(New Triple(dotNetRDF, says, helloWorld))
        g.Assert(New Triple(dotNetRDF, says12, helloWorld))
        g.Assert(New Triple(dotNetRDF, says, bonjourMonde))
        g.Assert(New Triple(helloWorld, helloWorld, helloWorld))
        g.Assert(New Triple(dotNetRDF, dotNetRDF, dotNetRDF))
        g.Assert(New Triple(bonjourMonde, bonjourMonde, bonjourMonde))
        Print(g)

        Using st = New SQLStore()
            st.SaveGraph(g)
        End Using
    End Sub


    Private Shared Sub LoadGraph()
        Using st = New SQLStore()
            Dim g As IGraph = New Graph()
            st.LoadGraph(g, "")
            Print(g)
        End Using
    End Sub


    Private Shared Sub LoadGraphV()
        Using st = New SQLStore()
            Dim g As IGraph = New Graph()
            st.LoadGraphVirtual(g, UriFactory.Create("http://example.org/graph#Big500"))
            Print(g)
        End Using
    End Sub

End Class