Imports System.Web.Http

Public Class BlackholeBaseController
    Inherits ApiController

    ' http://www.asp.net/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api

    Public Shared Sub ConfigureRoutesAndHandlers(config As System.Web.Http.HttpConfiguration)
        config.Routes.MapHttpRoute("Assets", "assets/{*path}", New With {.controller = "Assets"})
        config.Routes.MapHttpRoute("Query with Extension", "query/{store}.{ext}", New With {.controller = "Query", .ext = "json"})
        config.Routes.MapHttpRoute("Update with Extension", "update/{store}.{ext}", New With {.controller = "Update", .ext = "json"})
        config.Routes.MapHttpRoute("Query", "query/{store}", New With {.controller = "Query", .ext = "json"})
        config.Routes.MapHttpRoute("Update", "update/{store}", New With {.controller = "Update", .ext = "json"})
        config.Routes.MapHttpRoute("Blackhole", "{controller}", New With {.controller = "Blackhole", .ext = "json"})

        config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always 'TODO: Should be configurable

        config.Formatters.Add(New SparqlResultsetFormatter)
    End Sub
End Class
