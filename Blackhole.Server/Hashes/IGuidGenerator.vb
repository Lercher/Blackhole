Imports VDS.RDF

Public Interface IGuidGenerator
    Function fromGraphUri(uri As Uri) As Guid
    Function fromNode(n As INode) As Guid
End Interface
