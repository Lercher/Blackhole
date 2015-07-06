Imports System.Data.Linq.Mapping
Imports <xmlns:s="http://schemas.microsoft.com/linqtosql/mapping/2007">

Public Class BlackholeDBMappingSource
    Public Shared Function CreateMappingSourceFor(schemaname As String) As MappingSource
        ' The XML is generated as the file bh.xml by executing
        ' "C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\sqlmetal" /database:Blackhole /map:bh.xml /code:x.vb
        ' plus capitalizing the Member/Storage attributes
        Dim sqlmetal =
            <?xml version="1.0" encoding="utf-8"?>
            <Database Name="BlackholeDBDataContext" xmlns="http://schemas.microsoft.com/linqtosql/mapping/2007">
                <Table Name=<%= schemaname & ".NODE" %> Member="NODE">
                    <Type Name="NODE">
                        <Column Name="ID" Member="ID" Storage="_ID" DbType="UniqueIdentifier NOT NULL" IsPrimaryKey="true"/>
                        <Column Name="AlternateID" Member="AlternateID" Storage="_AlternateID" DbType="UniqueIdentifier NOT NULL" IsPrimaryKey="true"/>
                        <Column Name="type" Member="type" Storage="_type" DbType="TinyInt NOT NULL"/>
                        <Column Name="value" Member="value" Storage="_value" DbType="VarChar(MAX)" UpdateCheck="Never"/>
                        <Column Name="metadata" Member="metadata" Storage="_metadata" DbType="VarChar(MAX)" UpdateCheck="Never"/>
                    </Type>
                </Table>
                <Table Name=<%= schemaname & ".QUAD" %> Member="QUAD">
                    <Type Name="QUAD">
                        <Column Name="subject" Member="subject" Storage="_subject" DbType="UniqueIdentifier NOT NULL" IsPrimaryKey="true"/>
                        <Column Name="predicate" Member="predicate" Storage="_predicate" DbType="UniqueIdentifier NOT NULL" IsPrimaryKey="true"/>
                        <Column Name="object" Member="object" Storage="_object" DbType="UniqueIdentifier NOT NULL" IsPrimaryKey="true"/>
                        <Column Name="graph" Member="graph" Storage="_graph" DbType="UniqueIdentifier NOT NULL" IsPrimaryKey="true"/>
                        <Column Name="s25p5o1type" Member="s25p5o1type" Storage="_s25p5o1type" DbType="TinyInt NOT NULL"/>
                    </Type>
                </Table>
            </Database>
        Using rd = sqlmetal.CreateReader
            Return XmlMappingSource.FromReader(rd)
        End Using
    End Function
End Class
