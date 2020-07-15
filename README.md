# ratatouille
Remote management tool for Windows


## Master

`Master/bin/Release/Master.exe port`

## Slave

`Slave/bin/Release/Slave.exe remote_ip port`

Slave does not write to the standard output, but will be kept alive until a `quit` command. If it loses the connection to master it will wait 10 seconds before trying again.
