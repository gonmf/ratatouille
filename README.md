# ratatouille
Remote management tool for Windows


## Master

`Master/bin/Release/Master.exe port`

Default port is 8000

## Slave (invisible)

`SlaveDaemon/bin/Release/COMSystem.exe`

It has the master IP and port hardcoded; recompile to change.

The slave does not produce any output, but will be kept alive until a `quit` command. If it loses the connection to master it will wait 10 seconds before trying again.
