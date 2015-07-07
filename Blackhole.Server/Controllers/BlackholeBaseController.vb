Imports System.Web.Http

Public Class BlackholeBaseController
    Inherits ApiController

    ' http://www.asp.net/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api

    Public Shared Sub ConfigureRoutesAndHandlers(config As System.Web.Http.HttpConfiguration)
        config.Routes.MapHttpRoute(
            "Assets Route",
            "assets/{*path}",
            New With {.controller = "Assets"}
        )

        config.Routes.MapHttpRoute(
            "Blackhole Storeless",
            "{controller}",
            New With {.controller = "Blackhole", .ext = "json"}
        )

        config.Routes.MapHttpRoute(
            "Blackhole with Extension",
            "{store}/{controller}.{ext}",
            New With {.ext = "json"}
        )

        config.Routes.MapHttpRoute(
            "Blackhole",
            "{store}/{controller}",
            New With {.ext = "json"}
        )


        config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always 'TODO: Should be configurable

        config.Formatters.Add(New SparqlResultsetFormatter)
    End Sub
End Class
