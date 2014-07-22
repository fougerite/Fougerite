using Rust;
using Fougerite;
using Fougerite.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace ServerTests
{
    public class ServerTests : Fougerite.Module
    {
        public override string Name
        {
            get { return "ServerTests"; }
        }

        public override string Author
        {
            get { return "Riketta"; }
        }

        public override string Description
        {
            get { return "Testing all hooks and events"; }
        }

        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        private Fougerite.Player TesterPlayer = null;

        public override void Initialize()
        {
            Fougerite.Hooks.OnEntityDecay += new Fougerite.Hooks.EntityDecayDelegate(EntityDecay);
            Fougerite.Hooks.OnDoorUse += new Fougerite.Hooks.DoorOpenHandlerDelegate(DoorUse);
            Fougerite.Hooks.OnEntityHurt += new Fougerite.Hooks.EntityHurtDelegate(EntityHurt);
            Fougerite.Hooks.OnPlayerConnected += new Fougerite.Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Fougerite.Hooks.OnPlayerDisconnected += new Fougerite.Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Fougerite.Hooks.OnPlayerHurt += new Fougerite.Hooks.HurtHandlerDelegate(PlayerHurt);
            Fougerite.Hooks.OnPlayerKilled += new Fougerite.Hooks.KillHandlerDelegate(PlayerKilled);
            Fougerite.Hooks.OnServerShutdown += new Fougerite.Hooks.ServerShutdownDelegate(ServerShutdown);
            Fougerite.Hooks.OnShowTalker += new Fougerite.Hooks.ShowTalkerDelegate(ShowTalker);
            Fougerite.Hooks.OnChat += new Fougerite.Hooks.ChatHandlerDelegate(Chat);
            Fougerite.Hooks.OnChatReceived += new Fougerite.Hooks.ChatRecivedDelegate(ChatReceived);
            Fougerite.Hooks.OnCommand += new Fougerite.Hooks.CommandHandlerDelegate(Command);
        }

        public override void DeInitialize()
        {
            Fougerite.Hooks.OnEntityDecay -= new Fougerite.Hooks.EntityDecayDelegate(EntityDecay);
            Fougerite.Hooks.OnDoorUse -= new Fougerite.Hooks.DoorOpenHandlerDelegate(DoorUse);
            Fougerite.Hooks.OnEntityHurt -= new Fougerite.Hooks.EntityHurtDelegate(EntityHurt);
            Fougerite.Hooks.OnPlayerConnected -= new Fougerite.Hooks.ConnectionHandlerDelegate(PlayerConnect);
            Fougerite.Hooks.OnPlayerDisconnected -= new Fougerite.Hooks.DisconnectionHandlerDelegate(PlayerDisconnect);
            Fougerite.Hooks.OnPlayerHurt -= new Fougerite.Hooks.HurtHandlerDelegate(PlayerHurt);
            Fougerite.Hooks.OnPlayerKilled -= new Fougerite.Hooks.KillHandlerDelegate(PlayerKilled);
            Fougerite.Hooks.OnServerShutdown -= new Fougerite.Hooks.ServerShutdownDelegate(ServerShutdown);
            Fougerite.Hooks.OnShowTalker -= new Fougerite.Hooks.ShowTalkerDelegate(ShowTalker);
            Fougerite.Hooks.OnChat -= new Fougerite.Hooks.ChatHandlerDelegate(Chat);
            Fougerite.Hooks.OnChatReceived -= new Fougerite.Hooks.ChatRecivedDelegate(ChatReceived);
            Fougerite.Hooks.OnCommand -= new Fougerite.Hooks.CommandHandlerDelegate(Command);
        }

        private void Command(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "test" && args.Length == 1 && player.Admin)
            {
                TesterPlayer = player;
                switch (args[0].ToLower())
                {
                    case "all":
                        TestAll();
                        break;
                    case "dump":
                        ObjectsDump();
                        break;
                    case "target":
                        Target_Test();
                        break;
                    case "ground":
                        GetGround_Test();
                        break;
                    case "help":
                        player.Message("Use in only on test server!");
                        player.Message("/test dump - dump info about all objects to log");
                        player.Message("/test target - info about targeted object");
                        player.Message("/test ground - testing Z coord");
                        player.Message("/test all - testing all");
                        player.Message("/test save - saving world");
                        break;
                    case "save":
                        Server.GetServer().Save();
                        break;
                    default:
                        player.Message("Enter valid arg!");
                        break;
                }
            }
        }

        private void TestAll()
        {
            Log("Testing");
            int Range = 50;
            Random Rand = new Random();
            for (int i = 0; i < 5; i++)
            {
                World.GetWorld()
                    .Spawn(";deploy_wood_storage_large", TesterPlayer.X + Rand.Next(-Range, Range), TesterPlayer.Y - 1,
                        TesterPlayer.Z + Rand.Next(-Range, Range));
                World.GetWorld()
                    .Spawn(";deploy_largewoodspikewall", TesterPlayer.X + Rand.Next(-Range, Range), TesterPlayer.Y - 1,
                        TesterPlayer.Z + Rand.Next(-Range, Range));
                World.GetWorld()
                    .Spawn(";struct_wood_foundation", TesterPlayer.X + Rand.Next(-Range, Range), TesterPlayer.Y - 1,
                        TesterPlayer.Z + Rand.Next(-Range, Range));
            }
            Log("Entities placed");

            BlueprintUse_Test();
            ChatReceived_Test();
            ConsoleReceived_Test();
            EntityDecay_Test();
            EntityDeployed_Test();
            EntityHurt_Test();
            NPCHurt_Test();
            NPCKilled_Test();
            PlayerHurt_Test();
            PlayerKilled_Test();
            PlayerConnect_Test();
            PlayerDisconnect_Test();
            Say_Test();
            PlayerBroadcast_Test();
            Notice_Test();
            Broadcast_Test();
            Log_Test();
            FindPlayer_Test();
            GetGround_Test();
            Log("Tested!");
        }

        private void Log(string MSG)
        {
            Logger.LogDebug("[Test] " + MSG);
        }

        //

        private void ObjectsDump()
        {
            try
            {
                var goArray = UnityEngine.Object.FindObjectsOfType(typeof (GameObject));
                Logger.LogDebug("[DUMP] ========== START ==========\n[DUMP] Length: " + goArray.Length);
                for (var i = 0; i < goArray.Length; i++)
                {
                    GameObject GO = (GameObject) goArray[i];
                    Logger.LogDebug("[DUMP]: " + GO.name + " - " + GO.tag + " - " +
                                    GO.layer.ToString());
                }
                Logger.LogDebug("[DUMP] =========== END ===========");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private void Target_Test()
        {
            try
            {
                Vector3 origin = TesterPlayer.Location;
                RaycastHit Hit;

                if (Physics.Raycast(origin, TesterPlayer.PlayerClient.transform.forward, out Hit, float.MaxValue))
                    Logger.LogDebug("GetGroundDist: [" + Hit.distance + "] " + Hit.transform.name + " - " +
                                    Hit.transform.tag + " - " + Hit.transform.gameObject.layer.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void GetGround_Test()
        {
            try
            {
                Log("GetGround_Test: Test 1");
                Log(World.GetWorld().GetGroundDist(TesterPlayer.Location).ToString());

                Log("GetGround_Test: Test 2");
                Vector3 origin = new Vector3(TesterPlayer.X, 2000f, TesterPlayer.Z);
                Vector3 direction = new Vector3(0f, -1f, 0f);
                Log(Physics.RaycastAll(origin, direction)[0].point.y.ToString());
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void FindPlayer_Test()
        {
            Log("FindPlayer_Test: Test 1");
            Fougerite.Player.FindByGameID("");

            Log("FindPlayer_Test: Test 2");
            Fougerite.Player.FindByGameID(null);

            Log("FindPlayer_Test: Test 3");
            Fougerite.Player.FindByName("");

            Log("FindPlayer_Test: Test 4");
            Fougerite.Player.FindByName(null);

            Log("FindPlayer_Test: Test 5");
            Fougerite.Player.FindByPlayerClient(null);

            Log("FindPlayer_Test: Test 6");
            Server.GetServer().FindPlayer("");

            Log("FindPlayer_Test: Test 7");
            Server.GetServer().FindPlayer("");
        }

        void Log_Test()
        {
            Log("Log_Test: Test 1");
            Util.GetUtil().ConsoleLog("");

            Log("Log_Test: Test 2");
            Util.GetUtil().ConsoleLog(null);

            Log("Log_Test: Test 3");
            Util.GetUtil().Log("");

            Log("Log_Test: Test 4");
            Util.GetUtil().Log(null);
        }

        void Broadcast_Test()
        {
            Log("Broadcast_Test: Test 1");
            Server.GetServer().Broadcast("");

            Log("Broadcast_Test: Test 2");
            Server.GetServer().Broadcast(null);

            Log("Broadcast_Test: Test 3");
            Server.GetServer().BroadcastFrom("", "");

            Log("Broadcast_Test: Test 4");
            Server.GetServer().BroadcastFrom("", null);

            Log("Broadcast_Test: Test 5");
            Server.GetServer().BroadcastFrom(null, "");

            Log("Broadcast_Test: Test 6");
            Server.GetServer().BroadcastFrom(null, null);

            Log("Broadcast_Test: Test 7");
            Server.GetServer().BroadcastNotice("");

            Log("Broadcast_Test: Test 8");
            Server.GetServer().BroadcastNotice(null);
        }

        void Notice_Test()
        {
            Log("Notice_Test: Test 1");
            TesterPlayer.Notice("");

            Log("Notice_Test: Test 2");
            TesterPlayer.Notice(null);
        }

        void PlayerBroadcast_Test()
        {
            Log("PlayerBroadcast_Test: Test 1");
            TesterPlayer.Message("");

            Log("PlayerBroadcast_Test: Test 2");
            TesterPlayer.Message(null);

            Log("PlayerBroadcast_Test: Test 3");
            TesterPlayer.MessageFrom("", null);

            Log("PlayerBroadcast_Test: Test 4");
            TesterPlayer.MessageFrom(null, "");

            Log("PlayerBroadcast_Test: Test 5");
            TesterPlayer.MessageFrom(null, null);
        }

        void Say_Test()
        {
            try
            {
                Log("Say_Test: Test 1");
                Util.say(TesterPlayer.PlayerClient.netPlayer, TesterPlayer.Name, "");

                Log("Say_Test: Test 2");
                Util.say(TesterPlayer.PlayerClient.netPlayer, TesterPlayer.Name, null);

                Log("Say_Test: Test 3");
                Util.say(TesterPlayer.PlayerClient.netPlayer, TesterPlayer.Name, " ");

                Log("Say_Test: Test 4");
                Util.sayAll("");

                Log("Say_Test: Test 5");
                Util.sayAll(null);

                Log("Say_Test: Test 6");
                Util.sayUser(TesterPlayer.PlayerClient.netPlayer, "");
                
                Log("Say_Test: Test 7");
                Util.sayUser(TesterPlayer.PlayerClient.netPlayer, null);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void BlueprintUse_Test()
        {
            try
            {
                BlueprintDataBlock BDB = new BlueprintDataBlock();
                Log("BlueprintUse_Test: Test 1");
                Hooks.BlueprintUse(null, BDB);

                Log("BlueprintUse_Test: Test 2");
                Hooks.BlueprintUse(null, null);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void ChatReceived_Test()
        {
            try
            {
                ConsoleSystem.Arg arg = new ConsoleSystem.Arg("");
                Log("ChatReceived_Test: Test 1");
                Hooks.ChatReceived(ref arg);

                arg = null;
                Log("ChatReceived_Test: Test 2");
                Hooks.ChatReceived(ref arg);

                arg = new ConsoleSystem.Arg("say test");
                Log("ChatReceived_Test: Test 3");
                Hooks.ChatReceived(ref arg);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void ConsoleReceived_Test()
        {
            try
            {
                ConsoleSystem.Arg arg = new ConsoleSystem.Arg("");
                Log("ConsoleReceived_Test: Test 1");
                Hooks.ConsoleReceived(ref arg);

                arg = null;
                Log("ConsoleReceived_Test: Test 2");
                Hooks.ConsoleReceived(ref arg);

                arg = new ConsoleSystem.Arg("say test");
                Log("ConsoleReceived_Test: Test 3");
                Hooks.ConsoleReceived(ref arg);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void EntityDecay_Test()
        {
            try
            {
                Entity Ent = null;
                if (World.GetWorld().Entities.ToArray().Length > 0)
                    Ent = World.GetWorld().Entities.ToArray()[0];
                if (Ent == null)
                    Log("EntityDecay_Test: Ent == null!");

                Log("EntityDecay_Test: Test 1");
                Hooks.EntityDecay(Ent, 0f);

                Log("EntityDecay_Test: Test 2");
                Hooks.EntityDecay(Ent, -100f);

                Log("EntityDecay_Test: Test 3");
                Hooks.EntityDecay(Ent, 10000f);

                Log("EntityDecay_Test: Test 4");
                Hooks.EntityDecay(null, 0f);

                Log("EntityDecay_Test: Test 5");
                Hooks.EntityDecay(null, 10000f);

                Ent = new Entity(new object());

                Log("EntityDecay_Test: Test 6");
                Hooks.EntityDecay(Ent, 0f);

                Log("EntityDecay_Test: Test 7");
                Hooks.EntityDecay(Ent, 10000f);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void EntityDeployed_Test()
        {
            try
            {
                Entity Ent = null;
                if (World.GetWorld().Entities.ToArray().Length > 1)
                    Ent = World.GetWorld().Entities.ToArray()[1];
                if (Ent == null)
                    Log("EntityDeployed_Test: Ent == null!");

                Log("EntityDeployed_Test: Test 1");
                Hooks.EntityDeployed(Ent);

                Fougerite.Player player = null;
                Ent.ChangeOwner(player);
                Log("EntityDeployed_Test: Test 2");
                Hooks.EntityDeployed(Ent);

                Log("EntityDeployed_Test: Test 3");
                Hooks.EntityDeployed(null);

                Ent = new Entity(new object());
                Log("EntityDeployed_Test: Test 4");
                Hooks.EntityDeployed(Ent);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void EntityHurt_Test()
        {
            try
            {
                Entity Ent = null;
                if (World.GetWorld().Entities.ToArray().Length > 2)
                    Ent = World.GetWorld().Entities.ToArray()[2];
                if (Ent == null)
                    Log("EntityHurt_Test: Ent == null!");

                DamageEvent damageEvent = new DamageEvent();
                damageEvent.amount = 50f;

                Log("EntityHurt_Test: Test 1");
                Hooks.EntityHurt(Ent, ref damageEvent);

                Fougerite.Player player = null;
                Ent.ChangeOwner(player);
                Log("EntityHurt_Test: Test 2");
                Hooks.EntityHurt(Ent, ref damageEvent);

                Log("EntityHurt_Test: Test 3");
                Hooks.EntityHurt(null, ref damageEvent);

                Ent = new Entity(new object());
                Log("EntityHurt_Test: Test 4");
                Hooks.EntityHurt(Ent, ref damageEvent);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void NPCHurt_Test()
        {
            try
            {
                DamageEvent damageEvent = new DamageEvent();
                Log("NPCHurt_Test: Test 1");
                Hooks.NPCHurt(ref damageEvent);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void NPCKilled_Test()
        {
            try
            {
                DamageEvent damageEvent = new DamageEvent();
                Log("NPCKilled_Test: Test 1");
                Hooks.NPCKilled(ref damageEvent);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void PlayerConnect_Test()
        {
            try
            {
                NetUser NUser = null;
                Log("PlayerConnect_Test: Test 1");
                Hooks.PlayerConnect(NUser);

                Log("PlayerConnect_Test: Test 2");
                Hooks.PlayerConnect(TesterPlayer.PlayerClient.netUser);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void PlayerDisconnect_Test()
        {
            try
            {
                NetUser NUser = null;
                Log("PlayerConnect_Test: Test 1");
                Hooks.PlayerDisconnect(NUser);

                Log("PlayerConnect_Test: Test 2");
                Hooks.PlayerDisconnect(TesterPlayer.PlayerClient.netUser);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void PlayerHurt_Test()
        {
            try
            {
                DamageEvent damageEvent = new DamageEvent();
                Log("PlayerHurt_Test: Test 1");
                Hooks.PlayerHurt(ref damageEvent);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        void PlayerKilled_Test()
        {
            try
            {
                DamageEvent damageEvent = new DamageEvent();
                Log("PlayerKilled_Test: Test 1");
                Hooks.PlayerHurt(ref damageEvent);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        //

        void ChatReceived(ref ConsoleSystem.Arg arg)
        {
        }

        void Chat(Fougerite.Player p, ref ChatString text)
        {
        }

        void ShowTalker(uLink.NetworkPlayer player, PlayerClient p)
        {
        }

        void ServerShutdown()
        {
        }

        void PlayerKilled(DeathEvent deathEvent)
        {
        }

        void EntityDecay(DecayEvent de)
        {
        }

        void DoorUse(Fougerite.Player p, DoorEvent de)
        {
        }

        void PlayerHurt(HurtEvent he)
        {
        }

        void PlayerDisconnect(Fougerite.Player player)
        {
        }

        void PlayerConnect(Fougerite.Player player)
        {
        }

        void EntityHurt(HurtEvent he)
        {
        }
    }
}
