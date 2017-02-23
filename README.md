# Fougerite project

Active Development Page: https://github.com/Notulp/Fougerite

Fougerite a fully compatible with Magma server mod, featuring better performance with Python and C# plugins.   

## Compilation
* Compile Fougerite.Patcher.
* Copy the patcher to the directory with Assembly-CSharp.dll and run it with a "-1" (first pass) option.
* Copy patched Assembly-CSharp.dll to References directory.
* Compile Fougerite
* Compile plugins.
* Run the patcher with a "-2" (second pass) option.

## Installation
* Backup Your server just incase, if something would go wrong.
* Download the latest release (usually unstable) or the previous one.
* Unpack dlls and install them to rust_server_Data\Managed directory.
* Run Patcher.
* Rename your MagmaDirectory.cfg config to FougeriteDirectory.cfg.  
* If you using Magma copy your plugins from the "save" folder to: "Server_Folder\Fougerite\" (main server's folder, if "Fougerite" folder doesn't exist, just create it) and read read WARNINGs.  
* Launch your server.  

**WARNING:** If you using Magma you must extract all your files from "save" to main server folder (with rust_server.exe).  
Plugins must be placed in "Server_Folder\Fougerite\MagmaPlugin" - for example "Server\Fougerite\MagmaPlugin\SuperPlugin.js"  
Other Plugins and Modules will have their own folders here, too.  See Fougerite.cfg.

Come play on the Official Fougerite Rust server at rust.clandestina.la:28025 (192.200.111.163:28025)

Use Git Issues system to report bugs, please. 
Please visit [our forum](http://fougerite.com/) for more information.

***
###### Developed by EquiFox & xEnt (Rust++ and Magma)
###### Forked by Riketta (Zumwalt Project)
###### Renamed by Alexknvl (from "Zumwalt" to "Fougerite")
###### 19-NOV-2014: Contributions and on-going maintenance undertaken by Team Pluton
