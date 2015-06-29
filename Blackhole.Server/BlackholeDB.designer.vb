﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.34209
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Data.Linq
Imports System.Data.Linq.Mapping
Imports System.Linq
Imports System.Linq.Expressions
Imports System.Reflection


<Global.System.Data.Linq.Mapping.DatabaseAttribute(Name:="Blackhole")>  _
Partial Public Class BlackholeDBDataContext
	Inherits System.Data.Linq.DataContext
	
	Private Shared mappingSource As System.Data.Linq.Mapping.MappingSource = New AttributeMappingSource()
	
  #Region "Extensibility Method Definitions"
  Partial Private Sub OnCreated()
  End Sub
  Partial Private Sub InsertNODE(instance As NODE)
    End Sub
  Partial Private Sub UpdateNODE(instance As NODE)
    End Sub
  Partial Private Sub DeleteNODE(instance As NODE)
    End Sub
  Partial Private Sub InsertQUAD(instance As QUAD)
    End Sub
  Partial Private Sub UpdateQUAD(instance As QUAD)
    End Sub
  Partial Private Sub DeleteQUAD(instance As QUAD)
    End Sub
  #End Region
	
	Public Sub New()
		MyBase.New(Global.Blackhole.Server.My.MySettings.Default.BlackholeConnectionString, mappingSource)
		OnCreated
	End Sub
	
	Public Sub New(ByVal connection As String)
		MyBase.New(connection, mappingSource)
		OnCreated
	End Sub
	
	Public Sub New(ByVal connection As System.Data.IDbConnection)
		MyBase.New(connection, mappingSource)
		OnCreated
	End Sub
	
	Public Sub New(ByVal connection As String, ByVal mappingSource As System.Data.Linq.Mapping.MappingSource)
		MyBase.New(connection, mappingSource)
		OnCreated
	End Sub
	
	Public Sub New(ByVal connection As System.Data.IDbConnection, ByVal mappingSource As System.Data.Linq.Mapping.MappingSource)
		MyBase.New(connection, mappingSource)
		OnCreated
	End Sub
	
	Public ReadOnly Property NODEs() As System.Data.Linq.Table(Of NODE)
		Get
			Return Me.GetTable(Of NODE)
		End Get
	End Property
	
	Public ReadOnly Property QUADs() As System.Data.Linq.Table(Of QUAD)
		Get
			Return Me.GetTable(Of QUAD)
		End Get
	End Property
End Class

<Global.System.Data.Linq.Mapping.TableAttribute()>  _
Partial Public Class NODE
	Implements System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	
	Private Shared emptyChangingEventArgs As PropertyChangingEventArgs = New PropertyChangingEventArgs(String.Empty)
	
	Private _ID As System.Guid
	
	Private _type As Byte
	
	Private _value As String
	
	Private _metadata As String
	
    #Region "Extensibility Method Definitions"
    Partial Private Sub OnLoaded()
    End Sub
    Partial Private Sub OnValidate(action As System.Data.Linq.ChangeAction)
    End Sub
    Partial Private Sub OnCreated()
    End Sub
    Partial Private Sub OnIDChanging(value As System.Guid)
    End Sub
    Partial Private Sub OnIDChanged()
    End Sub
    Partial Private Sub OntypeChanging(value As Byte)
    End Sub
    Partial Private Sub OntypeChanged()
    End Sub
    Partial Private Sub OnvalueChanging(value As String)
    End Sub
    Partial Private Sub OnvalueChanged()
    End Sub
    Partial Private Sub OnmetadataChanging(value As String)
    End Sub
    Partial Private Sub OnmetadataChanged()
    End Sub
    #End Region
	
	Public Sub New()
		MyBase.New
		OnCreated
	End Sub
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Storage:="_ID", DbType:="UniqueIdentifier NOT NULL", IsPrimaryKey:=true)>  _
	Public Property ID() As System.Guid
		Get
			Return Me._ID
		End Get
		Set
			If ((Me._ID = value)  _
						= false) Then
				Me.OnIDChanging(value)
				Me.SendPropertyChanging
				Me._ID = value
				Me.SendPropertyChanged("ID")
				Me.OnIDChanged
			End If
		End Set
	End Property
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Storage:="_type", DbType:="TinyInt NOT NULL")>  _
	Public Property type() As Byte
		Get
			Return Me._type
		End Get
		Set
			If ((Me._type = value)  _
						= false) Then
				Me.OntypeChanging(value)
				Me.SendPropertyChanging
				Me._type = value
				Me.SendPropertyChanged("type")
				Me.OntypeChanged
			End If
		End Set
	End Property
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Storage:="_value", DbType:="VarChar(MAX)")>  _
	Public Property value() As String
		Get
			Return Me._value
		End Get
		Set
			If (String.Equals(Me._value, value) = false) Then
				Me.OnvalueChanging(value)
				Me.SendPropertyChanging
				Me._value = value
				Me.SendPropertyChanged("value")
				Me.OnvalueChanged
			End If
		End Set
	End Property
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Storage:="_metadata", DbType:="VarChar(MAX)")>  _
	Public Property metadata() As String
		Get
			Return Me._metadata
		End Get
		Set
			If (String.Equals(Me._metadata, value) = false) Then
				Me.OnmetadataChanging(value)
				Me.SendPropertyChanging
				Me._metadata = value
				Me.SendPropertyChanged("metadata")
				Me.OnmetadataChanged
			End If
		End Set
	End Property
	
	Public Event PropertyChanging As PropertyChangingEventHandler Implements System.ComponentModel.INotifyPropertyChanging.PropertyChanging
	
	Public Event PropertyChanged As PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
	
	Protected Overridable Sub SendPropertyChanging()
		If ((Me.PropertyChangingEvent Is Nothing)  _
					= false) Then
			RaiseEvent PropertyChanging(Me, emptyChangingEventArgs)
		End If
	End Sub
	
	Protected Overridable Sub SendPropertyChanged(ByVal propertyName As [String])
		If ((Me.PropertyChangedEvent Is Nothing)  _
					= false) Then
			RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
		End If
	End Sub
End Class

<Global.System.Data.Linq.Mapping.TableAttribute()>  _
Partial Public Class QUAD
	Implements System.ComponentModel.INotifyPropertyChanging, System.ComponentModel.INotifyPropertyChanged
	
	Private Shared emptyChangingEventArgs As PropertyChangingEventArgs = New PropertyChangingEventArgs(String.Empty)
	
	Private _subject As System.Guid
	
	Private _predicate As System.Guid
	
	Private _object As System.Guid
	
	Private _graph As System.Guid
	
	Private _s25p5o1type As Byte
	
    #Region "Extensibility Method Definitions"
    Partial Private Sub OnLoaded()
    End Sub
    Partial Private Sub OnValidate(action As System.Data.Linq.ChangeAction)
    End Sub
    Partial Private Sub OnCreated()
    End Sub
    Partial Private Sub OnsubjectChanging(value As System.Guid)
    End Sub
    Partial Private Sub OnsubjectChanged()
    End Sub
    Partial Private Sub OnpredicateChanging(value As System.Guid)
    End Sub
    Partial Private Sub OnpredicateChanged()
    End Sub
    Partial Private Sub OnobjectChanging(value As System.Guid)
    End Sub
    Partial Private Sub OnobjectChanged()
    End Sub
    Partial Private Sub OngraphChanging(value As System.Guid)
    End Sub
    Partial Private Sub OngraphChanged()
    End Sub
    Partial Private Sub Ons25p5o1typeChanging(value As Byte)
    End Sub
    Partial Private Sub Ons25p5o1typeChanged()
    End Sub
    #End Region
	
	Public Sub New()
		MyBase.New
		OnCreated
	End Sub
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Storage:="_subject", DbType:="UniqueIdentifier NOT NULL", IsPrimaryKey:=true)>  _
	Public Property subject() As System.Guid
		Get
			Return Me._subject
		End Get
		Set
			If ((Me._subject = value)  _
						= false) Then
				Me.OnsubjectChanging(value)
				Me.SendPropertyChanging
				Me._subject = value
				Me.SendPropertyChanged("subject")
				Me.OnsubjectChanged
			End If
		End Set
	End Property
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Storage:="_predicate", DbType:="UniqueIdentifier NOT NULL", IsPrimaryKey:=true)>  _
	Public Property predicate() As System.Guid
		Get
			Return Me._predicate
		End Get
		Set
			If ((Me._predicate = value)  _
						= false) Then
				Me.OnpredicateChanging(value)
				Me.SendPropertyChanging
				Me._predicate = value
				Me.SendPropertyChanged("predicate")
				Me.OnpredicateChanged
			End If
		End Set
	End Property
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Name:="object", Storage:="_object", DbType:="UniqueIdentifier NOT NULL", IsPrimaryKey:=true)>  _
	Public Property [object]() As System.Guid
		Get
			Return Me._object
		End Get
		Set
			If ((Me._object = value)  _
						= false) Then
				Me.OnobjectChanging(value)
				Me.SendPropertyChanging
				Me._object = value
				Me.SendPropertyChanged("[object]")
				Me.OnobjectChanged
			End If
		End Set
	End Property
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Storage:="_graph", DbType:="UniqueIdentifier NOT NULL", IsPrimaryKey:=true)>  _
	Public Property graph() As System.Guid
		Get
			Return Me._graph
		End Get
		Set
			If ((Me._graph = value)  _
						= false) Then
				Me.OngraphChanging(value)
				Me.SendPropertyChanging
				Me._graph = value
				Me.SendPropertyChanged("graph")
				Me.OngraphChanged
			End If
		End Set
	End Property
	
	<Global.System.Data.Linq.Mapping.ColumnAttribute(Storage:="_s25p5o1type", DbType:="TinyInt NOT NULL")>  _
	Public Property s25p5o1type() As Byte
		Get
			Return Me._s25p5o1type
		End Get
		Set
			If ((Me._s25p5o1type = value)  _
						= false) Then
				Me.Ons25p5o1typeChanging(value)
				Me.SendPropertyChanging
				Me._s25p5o1type = value
				Me.SendPropertyChanged("s25p5o1type")
				Me.Ons25p5o1typeChanged
			End If
		End Set
	End Property
	
	Public Event PropertyChanging As PropertyChangingEventHandler Implements System.ComponentModel.INotifyPropertyChanging.PropertyChanging
	
	Public Event PropertyChanged As PropertyChangedEventHandler Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
	
	Protected Overridable Sub SendPropertyChanging()
		If ((Me.PropertyChangingEvent Is Nothing)  _
					= false) Then
			RaiseEvent PropertyChanging(Me, emptyChangingEventArgs)
		End If
	End Sub
	
	Protected Overridable Sub SendPropertyChanged(ByVal propertyName As [String])
		If ((Me.PropertyChangedEvent Is Nothing)  _
					= false) Then
			RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
		End If
	End Sub
End Class
