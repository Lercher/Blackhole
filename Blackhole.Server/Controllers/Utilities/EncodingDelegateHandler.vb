Imports System.IO.Compression
Imports System.Net.Http
Imports System.Threading



Public Class EncodingDelegateHandler
    Inherits DelegatingHandler

    Protected Overrides Async Function SendAsync(request As HttpRequestMessage, cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        Dim response = Await MyBase.SendAsync(request, cancellationToken)
        If response.Content IsNot Nothing Then
            If response.RequestMessage.Headers.AcceptEncoding IsNot Nothing Then
                If response.RequestMessage.Headers.AcceptEncoding.Count > 0 Then
                    Dim encodingType As String = response.RequestMessage.Headers.AcceptEncoding.First().Value
                    response.Content = New CompressedContent(response.Content, encodingType)
                End If
            End If
        End If
        Return response
    End Function


    Public Class CompressedContent
        Inherits HttpContent

        Private originalContent As HttpContent
        Private encodingType As String

        Public Sub New(content As HttpContent, encodingType As String)
            If content Is Nothing Then Throw New ArgumentNullException("content")
            If encodingType Is Nothing Then Throw New ArgumentNullException("encodingType")

            originalContent = content
            Me.encodingType = encodingType.ToLowerInvariant()

            If Me.encodingType <> "gzip" AndAlso Me.encodingType <> "deflate" Then
                Throw New InvalidOperationException(String.Format("Encoding '{0}' is not supported. Only gzip and deflate encoding are supported.", Me.encodingType))
            End If

            ' copy the headers from the original content
            For Each header In originalContent.Headers
                Me.Headers.TryAddWithoutValidation(header.Key, header.Value)
            Next

            Me.Headers.ContentEncoding.Add(encodingType)
        End Sub

        Protected Overloads Overrides Function TryComputeLength(ByRef length As Long) As Boolean
            length = -1
            Return False
        End Function

        Protected Overrides Async Function SerializeToStreamAsync(stream As IO.Stream, context As Net.TransportContext) As Task
            Dim compressedStream As IO.Stream = Nothing
            If encodingType = "gzip" Then
                compressedStream = New GZipStream(stream, CompressionMode.Compress, leaveOpen:=True)
            ElseIf encodingType = "deflate" Then
                compressedStream = New DeflateStream(stream, CompressionMode.Compress, leaveOpen:=True)
            End If
            Await originalContent.CopyToAsync(compressedStream)
            If compressedStream IsNot Nothing Then compressedStream.Dispose()
        End Function

    End Class

End Class
