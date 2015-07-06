Imports VDS.RDF.Storage
Imports VDS.RDF.Storage.Management
Imports VDS.RDF.Storage.Management.Provisioning


' see http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/BaseAdoStoreManager.cs
' and http://sourceforge.net/p/dotnetrdf/svn/2338/tree/Trunk/Libraries/data.sql/Schemas/CreateMicrosoftAdoHashStore.sql

' We use TransactionScope for Transactions:
' Please note that in .Net 4 this does not need the Distributed Transaction Coordinator Service,
' since there is only one connection string per operation such as SaveGraph.

Public Class SQLStoreManagement
    Implements IStorageServer

    Private ctx As BlackholeDBDataContext

    Public Sub New()
        ctx = New BlackholeDBDataContext()
    End Sub

    Private Function CreateStore(storeID As String) As Boolean
        If StoreExists(storeID) Then Return True 'Throw New RdfStorageException(String.Format("Store '{0}' exists already", storeID))
        ctx.ExecuteCommand(String.Format("CREATE SCHEMA [bh_{0}] AUTHORIZATION [dbo]", storeID))
        Dim createNODE =
            <x>
                CREATE TABLE [bh_<%= storeID %>].[NODE](
	                [ID] [uniqueidentifier] NOT NULL,
	                [AlternateID] [uniqueidentifier] NOT NULL,
	                [type] [tinyint] NOT NULL,
	                [value] [varchar](max) NULL,
	                [metadata] [varchar](max) NULL,
                CONSTRAINT [PK_NODE_<%= storeID %>] PRIMARY KEY CLUSTERED 
                (
	                [ID] ASC,
	                [AlternateID] ASC
                ))
            </x>
        ctx.ExecuteCommand(createNODE.Value)
        Dim createQUAD =
            <x>
                CREATE TABLE [bh_<%= storeID %>].[QUAD](
	                [subject] [uniqueidentifier] NOT NULL,
	                [predicate] [uniqueidentifier] NOT NULL,
	                [object] [uniqueidentifier] NOT NULL,
	                [graph] [uniqueidentifier] NOT NULL,
	                [s25p5o1type] [tinyint] NOT NULL,
                 CONSTRAINT [PK_QUAD_<%= storeID %>] PRIMARY KEY CLUSTERED 
                (
	                [subject] ASC,
	                [predicate] ASC,
	                [object] ASC,
	                [graph] ASC
                ))
            </x>
        ctx.ExecuteCommand(createQUAD.Value)
        Return True
    End Function

    Public Sub DeleteStore(storeID As String) Implements IStorageServer.DeleteStore
        If StoreExists(storeID) Then
            Dim drops =
                <x>
                    DROP TABLE [bh_<%= storeID %>].[NODE]
                    DROP TABLE [bh_<%= storeID %>].[QUAD]
                    DROP SCHEMA [bh_<%= storeID %>] 
                </x>
            ctx.ExecuteCommand(drops.Value)
            Return
        End If
        Throw New RdfStorageException("Unknown store " & storeID)
    End Sub

    Private Function StoreExists(ByVal storeID As String) As Boolean
        Return ListStores.Where(Function(s) 0 = StringComparer.InvariantCultureIgnoreCase.Compare(s, storeID)).Any
    End Function

    Public Function CreateStore(template As Provisioning.IStoreTemplate) As Boolean Implements IStorageServer.CreateStore
        Dim t = TryCast(template, SQLStoreTemplate)
        If t Is Nothing Then Throw New RdfStorageException("Invalid template. It must not be null and one of my available templates.")
        t.ValidateAndThrow()
        Return CreateStore(template.ID)
    End Function

    Public Iterator Function GetAvailableTemplates(id As String) As IEnumerable(Of Provisioning.IStoreTemplate) Implements IStorageServer.GetAvailableTemplates
        Yield GetDefaultTemplate(id)
    End Function

    Public Function GetDefaultTemplate(id As String) As Provisioning.IStoreTemplate Implements IStorageServer.GetDefaultTemplate
        Return New SQLStoreTemplate(id)
    End Function

    Public Function ListStores() As IEnumerable(Of String) Implements IStorageServer.ListStores
        Return From s In ctx.schemas Where s.name Like "bh_*" Order By s.name Select s.name.Substring(3)
    End Function

    Public Function GetStore(storeID As String) As IStorageProvider Implements IStorageServer.GetStore
        Return New SQLStore(storeID)
    End Function

    Public ReadOnly Property IOBehaviour As IOBehaviour Implements IStorageServer.IOBehaviour
        Get
            Return IOBehaviour.GraphStore Or IOBehaviour.CanUpdateTriples Or IOBehaviour.CanCreateStores Or IOBehaviour.CanDeleteStores
        End Get
    End Property


#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                ctx.Dispose()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
