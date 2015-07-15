Imports System.Web.Http
Imports System.Web.Http.Cors

<EnableCors("*", "*", "*", PreFlightMaxAge:=3600)>
Public Class BlackholeBaseController
    Inherits ApiController

    ' http://www.asp.net/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api

    Public Shared Sub ConfigureRoutesAndHandlers(config As System.Web.Http.HttpConfiguration)
        config.EnableCors() ' see: http://www.asp.net/web-api/overview/security/enabling-cross-origin-requests-in-web-api
        config.Routes.MapHttpRoute("Assets", "assets/{*path}", New With {.controller = "Assets"})
        config.Routes.MapHttpRoute("Query with Extension", "query/{store}.{ext}", New With {.controller = "Query", .ext = "json"})
        config.Routes.MapHttpRoute("Update with Extension", "update/{store}.{ext}", New With {.controller = "Update", .ext = "json"})
        config.Routes.MapHttpRoute("Query", "query/{store}", New With {.controller = "Query", .ext = "json"})
        config.Routes.MapHttpRoute("Update", "update/{store}", New With {.controller = "Update", .ext = "json"})
        config.Routes.MapHttpRoute("Monitor", "monitor/{store}", New With {.controller = "Monitor"})
        config.Routes.MapHttpRoute("Blackhole", "{controller}", New With {.controller = "Blackhole", .ext = "json"})
        config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always 'TODO: Should be configurable
        config.Formatters.Add(New SparqlResultsetFormatter)
        config.MessageHandlers.Add(New BlackholeResponseHeaderHandler) ' for injecting response headers
        config.MessageHandlers.Add(New EncodingDelegateHandler) 'gzip and deflate support
    End Sub
End Class
