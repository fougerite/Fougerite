Fougerite project
====================
This is an unofficial fork of Magma. Source code were obtained with decompiler and corrected by me (Riketta).  
  
**Fougerite full compatible with Magma plugins.**   
  
To use it you need to add a reference to the project (Zumwalt) patched Assembly-CSharp through Zumwalt_Patcher.  
All code already updated and work. Just compile it or download from *releases*.  

### HOW TO USE:
* Compile patcher
* Patch your clear assembly
* Add patched assembly to FougeriteProject's References
* Compile Fougerite.dll

Than you must rename Magma config folder to Fougerite.  
That's all. You may launch your server.  
  
**WARNING:** If you using Magma plugins you must rename all calls to class "Magma" in plugin to "Fougerite".  
Like that: *var player = Magma.Player.FindBySteamID(id);* --> *var player = Fougerite.Player.FindBySteamID(id);*
  
*I hope that the community will help me develop this project.*  
Use Git Issues system for report bugs, please. Or send me e-mails. =) 
***
###### Developed by EquiFox & xEnt (Rust++ and Magma)  
###### Forked by Riketta (Zumwalt Project)  
###### Renamed by Alexknvl (from "Zumwalt" to "Fougerite")  
Riketta - rowneg@bk.ru

Website - Available Soon
