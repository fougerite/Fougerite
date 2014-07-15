using Rust;
using Fougerite;
using Fougerite.Events;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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

        void Command(Fougerite.Player player, string text, string[] args)
        {

        }

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
