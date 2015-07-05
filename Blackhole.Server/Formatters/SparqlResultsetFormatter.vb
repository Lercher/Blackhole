Imports System.Net.Http.Formatting
Imports System.Net.Http.Headers
Imports VDS.RDF.Query
Imports VDS.RDF.Writing
Imports VDS.RDF

' see http://www.asp.net/web-api/overview/formats-and-model-binding/media-formatters

Public Class SparqlResultsetFormatter
    Inherits BufferedMediaTypeFormatter
    Private Const STR_Applicationjson As String = "application/json"
    Private Const STR_Applicationxml As String = "application/xml"
    Private Const STR_Texthtml As String = "text/html"

    Public Sub New()
        'SupportedMediaTypes.Add(New MediaTypeHeaderValue("text/csv"))
        For Each d In MimeTypesHelper.Definitions
            If d.CanWriteSparqlResults Then
                For Each mt In d.MimeTypes
                    Console.WriteLine("Register {0}", mt)
                    SupportedMediaTypes.Add(New MediaTypeHeaderValue(mt))
                Next
            End If
        Next
        For Each d In MimeTypesHelper.Definitions
            If d.CanWriteSparqlResults Then
                Register(d.CanonicalFileExtension, d.CanonicalMimeType)
            End If
        Next
        Register("json", STR_Applicationjson)
        Register("xml", STR_Applicationxml)
        SupportedEncodings.Add(System.Text.Encoding.UTF8)
    End Sub

    Private Sub Register(ByVal ext As String, ByVal mt As String)
        Console.WriteLine("====== .{0} URIs served by {1} ======", ext, mt)
        AddUriPathExtensionMapping(ext, mt)
    End Sub

    Public Overrides Function CanReadType(type As Type) As Boolean
        Return False
    End Function

    Public Overrides Function CanWriteType(type As Type) As Boolean
        If type Is GetType(SparqlResultSet) Then Return True
        Return False
    End Function

    Public Overrides Sub WriteToStream(type As Type, value As Object, writeStream As IO.Stream, content As Net.Http.HttpContent)
        Using tw = New IO.StreamWriter(writeStream, System.Text.Encoding.UTF8)
            Dim rs = DirectCast(value, SparqlResultSet)
            Dim w = CreateWriter(content.Headers.ContentType.MediaType)
            w.Save(rs, tw)
        End Using
    End Sub

    Private Shared Function CreateWriter(mt As String) As ISparqlResultsWriter
        Return MimeTypesHelper.GetSparqlWriter(mt)
    End Function
End Class
