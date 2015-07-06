Imports VDS.RDF.Storage.Management.Provisioning
Imports VDS.RDF.Storage


Public Class SQLStoreTemplate
    Inherits StoreTemplate

    Public Sub New(id As String)
        MyBase.New(id, "SQLStore", "A hash virtualized SQL Server Store in schema bh_" & id)
    End Sub

    Public Overrides Iterator Function Validate() As IEnumerable(Of String)
        If String.IsNullOrWhiteSpace(ID) Then
            Yield "The store name (ID) cannot be empty"
            Return
        End If
        If Len(ID) > 30 Then Yield "The store name (ID) needs to be shorter than 30 characters"
        Dim re = New System.Text.RegularExpressions.Regex("^[a-z][a-z0-9_]*$", Text.RegularExpressions.RegexOptions.IgnoreCase)
        If Not re.IsMatch(ID) Then Yield "The store name (ID) can only consist of the letters a-z, digits and '_' and must start with a letter."
    End Function

    Public Sub ValidateAndThrow()
        Dim val = Validate()
        If val.Any Then
            Dim msg = String.Format("Invalid store template '{0}': {1}", Me.ID, Join(val.ToArray, " | "))
            Throw New RdfStorageException(msg)
        End If
    End Sub

End Class