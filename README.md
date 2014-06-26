Rust Zumwalt project
====================
This is an unofficial fork of Magma. Source code were obtained with decompiler and corrected by me (Riketta).  
  
**ZumwaltProject full compatible with Magma plugins.**   
  
To use it you need to add a reference to the project (Zumwalt) patched Assembly-CSharp through Zumwalt_Patcher.  
All code already updated and work. Just compile it or download from *releases*.  

### HOW TO USE:
* Compile patcher
* Patch your clear assembly
* Add patched assembly to ZumwaltProject's References
* Compilt Zumwalt.dll

Than you must rename Magma config folder to Zumwalt.  
That's all. You may launch your server.  
  
**WARNING:** If you using Magma plugins you must rename all calls to class "Magma" in plugin to "Zumwalt".  
Like that: *var player = Magma.Player.FindBySteamID(id);* --> *var player = Zumwalt.Player.FindBySteamID(id);*
  
*I hope that the community will help me develop this project.*  
Use Git Issues system for report bugs, please. Or send me e-mails. =) 
***
###### Forked by Riketta (Zumwalt Project) - Developed by EquiFox & xEnt (Rust++ and Magma)
Riketta - rowneg@bk.ru
