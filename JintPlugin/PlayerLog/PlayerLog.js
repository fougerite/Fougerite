var Unity = importNamespace('UnityEngine');
var PlayerLogData = function(pl, ev, epoch) {
	this.name = pl.Name;
	this.gid = pl.GameID;
	this.nuid = pl.PlayerClient.netUser.user.Userid.toString();
	this.ip = pl.IP;
	this.loc = pl.Location.ToString();
	this.ts = epoch;
	this.event = ev;
	this.first = false;
	this.camp = false;
};

function On_Command(Player, cmd, args) {
	if(Player.Admin && cmd =="flushplayerlog") {		
		DataStore.Flush('Player_Connect');
		DataStore.Flush('Player_Spawn');
		Player.MessageFrom('PlayerLog', 'Player_Connect and Player_Spawn flushed.  It\'s a brand new day.');
		DataStore.ToIni('PlayerLogDataStoreDump');
	}
}
	
function On_PlayerConnected(Player) {
	DataStore.Add('Player_Connect', Player.GameID, Plugin.Timestamp);
	DataStore.Add('PlayerNameByID', Player.GameID, Player.Name);
}

function On_PlayerSpawned(Player, se) {
	var data = new PlayerLogData(Player, 'spawn', Plugin.Timestamp);
	var connected = DataStore.Get('Player_Connect', data.gid);
	var spawned = DataStore.Get('Player_Spawn', data.gid);
	if(!(spawned > 0)) { spawned = -1; } 

	DataStore.Add('Player_Spawn', data.gid, data.ts);	
	
	if(connected > spawned) { // player just logged in, maybe for 1st time
		data.event = 'connect';
		if(spawned == -1) {
			data.first = true;
		}
		data.camp = se.CampUsed;
	} 
	Plugin.Log('PlayerJSON', JSON.stringify(data));

	/* 
	 * Posting to a website is easy. It's up to you to program the website to accept 
	 * the data at a URL and do something with it. Use your website URL, like this example:
	 * Plugin.POSTJson('http://www.myrustsite.ru/tysmiha', JSON.stringify(data));
	 */
}

function On_PlayerDisconnected(Player) {
	var data = new PlayerLogData(Player, 'disconnect', Plugin.Timestamp);
	Plugin.Log('PlayerJSON', JSON.stringify(data));
	// Plugin.POSTJson('http://www.myrustsite.ru/tysmiha', JSON.stringify(data));
}