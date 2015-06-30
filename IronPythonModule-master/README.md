Copy the files to the right place.
Make sure you have:
IronPythonModule=PyPlugins
and not:
;IronPythonModule=PyPlugins
in Save\Fougerite.cfg

There are two example plugins for tests.

Server, DataStore, Util, World are working the same way as in other Magma/Jint plugins

TimedEvent.ElapsedCount:int | how many time the timer ran

Entities class from Mike (initial, it will change a lot, don't get used to it):
	- Entities.Structures:Lookup<string, StructureMaster>
	- Entities.Doors:Lookup<string, BasicDoor>
	- Entities.Respawns:Lookup<string, DeployedRespawn>
	- Entities.Sleepers:Lookup<string, SleepingAvatar>
	- Entities.Inventories:Lookup<string, SaveableInventory>
	- Entities.Boxes:Lookup<string, SaveableInventory>
	- Entities.Stashes:Lookup<string, SaveableInventory>
	- Entities.Fires:Lookup<string, FireBarrel>
	- Entities.Shelters:Lookup<string, DeployableObject>

Plugin class:
	- Plugin.CreateDir(string path):bool
	
	- Plugin.DumpObjToFile(string path, object obj, (optional)string prefix):bool
	- Plugin.DumpObjToFile(string path, object obj, int depth, (optional)string prefix):bool
	- Plugin.DumpObjToFile(string path, object obj, int depth, int maxItems, (optional)string prefix):bool
	- Plugin.DumpObjToFile(string path, object obj, int depth, int maxItems, bool displayPrivate, (optional)string prefix):bool
	- Plugin.DumpObjToFile(string path, object obj, int depth, int maxItems, bool displayPrivate, bool useFullClassname, (optional)string prefix):bool
	
	- Plugin.DeleteLog(string path):void
	- Plugin.Log(string path, string text):void
	- Plugin.RotateLog(string logfile, (optional)int max = 6):void
	
	- Plugin.GetIni(string path):IniParser
	- Plugin.IniExists(string path):bool
	- Plugin.CreateIni(string path):IniParser
	- Plugin.GetInis(string path):List<IniParser>
	
	- Plugin.GetPlugin(string name):IPPlugin
	
	- Plugin.GetDate():string
	- Plugin.GetTicks():int
	- Plugin.GetTime():string
	- Plugin.GetTimeStamp():long

	- Plugin.CreateTimer(string name, int timoutInMilis):IPTimedEvent
	- Plugin.CreateTimer(string name, int timoutInMilis, Dictionary<string, object> args):IPTimedEvent | yeah, I changed paramslist for dictionary
	- Plugin.GetTimer(string name):IPTimedEvent
	- Plugin.KillTimer(string name):void
	- Plugin.KillTimers():void

ExtraHooks:
	- On_EntityDestroyed(DestroyEvent de):void
	- On_AllPluginsLoaded():void
	# note: On_EntityHurt isn't hooked, when it's caused by decay or if the entity was destroyed

DestroyEvent:
	- DestroyEvent.Destroyer:Fougerite.Player
	- DestroyEvent.DamageAmount:float
	- DestroyEvent.DamageEvent:DamageEvent
	- DestroyEvent.DamageType:string
	- DestroyEvent.Entity:Fougerite.Entity
	- DestroyEvent.IsDecay:bool
	- DestroyEvent.WeaponData:WeaponImpact
	- DestroyEvent.WeaponName:string

- DumpObjToFile
"(optional)" values have a default value, you can skip those. For example:
Plugin.DumpObjToFile("Players", Player, "")
Plugin.DumpObjToFile("Players", Player)
^ these are the same.
Use this only for debugging.

- GetPlugin
Use this to call other plugin's function like:
# myPlugin.py
class myPlugin:
	def myFunctionCallsAnother(self):
		otherPlugin = Plugin.GetPlugin("OtherPluginName")
		if(otherPlugin != null)
			otherPlugin.Invoke("otherPluginFunctionToInvoke", param1, param2)

# OtherPluginName.py
class OtherPluginName:
	def otherPluginFunctionToInvoke(self, param1, param2):
		doStuff()



to access UnityEngine.dll start your script with:
import clr
clr.AddReferenceByPartialName("UnityEngine")
import UnityEngine

class PluginName:
	def On_PluginInit(self):
		UnityEngine.Debug.Log("Hello world!")
