Imports Topshelf
' http://topshelf.readthedocs.org/en/latest/overview/commandline.html

#Const TRYME = True

Module MainModule

    Sub Main()
#If TRYME Then
        Tryme.All()
        Console.ReadLine()
        Return
#End If

        HostFactory.Run(
            Sub(c)
                c.Service(Of BlackholeService)(
                    Sub(sc)
                        sc.ConstructUsing(Function() New BlackholeService)
                        sc.WhenStarted(Function(s, hc) s.Start(hc))
                        sc.WhenStopped(Function(s, hc) s.Stop(hc))
                    End Sub
                )
                c.SetServiceName("Blackhole")
                c.SetDisplayName("Blackhole SPARQL Service")
                c.SetDescription("SPARQL Query and UPDATE Services with SQL Server backed triple store.")
                c.EnableShutdown()
                'Extensions:
                c.StartAutomaticallyDelayed()
                c.RunAsLocalService()
            End Sub
        )
    End Sub

End Module
