Read this!

Package updates remove previous versions of the config and will need to be restored.

In the config file update the appSettings Application.Name. Optionally configure the
serilog:minimum-level. Update the application's [Web|App].Release.config transform to
use the production Serilog server in production by adding a transform for the key
serilog:write-to:Seq.serverUrl.
