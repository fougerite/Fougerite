using System.Collections.Generic;

namespace Fougerite.Patcher
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;

    public class ILPatcher
    {
        private AssemblyDefinition rustAssembly = null;
        private AssemblyDefinition fougeriteAssembly = null;
        private TypeDefinition hooksClass = null;

        public ILPatcher()
        {
            try
            {
                rustAssembly = AssemblyDefinition.ReadAssembly("Assembly-CSharp.dll");
                // rustFirstPassAssembly = AssemblyDefinition.ReadAssembly("Assembly-CSharp-firstpass.dll");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void WrapWildlifeUpdateInTryCatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("WildlifeManager");
            TypeDefinition data = type.GetNestedType("Data");
            MethodDefinition think = data.GetMethod("Think");
            MethodDefinition update = type.GetMethod("Update");

            TypeDefinition logger = fougeriteAssembly.MainModule.GetType("Fougerite.Logger");
            MethodDefinition logex = logger.GetMethod("LogException");

            WrapMethod(update, logex, rustAssembly, false);
            WrapMethod(think, logex, rustAssembly, false);
        }

        private void uLinkLateUpdateInTryCatch()
        {
            AssemblyDefinition ulink = AssemblyDefinition.ReadAssembly("uLink.dll");
            TypeDefinition type = ulink.MainModule.GetType("uLink.InternalHelper");
            MethodDefinition update = type.GetMethod("LateUpdate");

            TypeDefinition logger = fougeriteAssembly.MainModule.GetType("Fougerite.Logger");
            MethodDefinition logex = logger.GetMethod("LogException");

            WrapMethod(update, logex, ulink, false);

            ulink.Write("uLink.dll");
        }

        private void LatePostInTryCatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("NetCull");
            TypeDefinition type2 = type.GetNestedType("Callbacks");
            MethodDefinition def = type2.GetMethod("FirePreUpdate");
            def.SetPublic(true);
        }

        private void LatePostInTryCatch2()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("NetCull");
            TypeDefinition type2 = type.GetNestedType("Callbacks");
            MethodDefinition def = type2.GetMethod("FirePreUpdate");
            Instruction y = null;
            foreach (Instruction x in def.Body.Instructions)
            {
                if (x.ToString().Contains("LogException"))
                {
                    y = x;
                }
            }
            def.Body.Instructions.Remove(y);
        }

        private void ResearchPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("ResearchToolItem`1");
            MethodDefinition TryCombine = type.GetMethod("TryCombine");
            MethodDefinition method = hooksClass.GetMethod("ResearchItem");
            int Position = TryCombine.Body.Instructions.Count - 13;

            ILProcessor iLProcessor = TryCombine.Body.GetILProcessor();
            iLProcessor.InsertBefore(TryCombine.Body.Instructions[Position],
                Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(TryCombine.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));
        }

        private void AntiDecay()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("EnvDecay");
            TypeDefinition definition2 = rustAssembly.MainModule.GetType("StructureMaster");

            MethodDefinition awake = type.GetMethod("Awake");
            MethodDefinition doDecay = type.GetMethod("DoDecay");
            MethodDefinition decayDisabled = hooksClass.GetMethod("DecayDisabled");

            ILProcessor iLProcessor = awake.Body.GetILProcessor();
            iLProcessor.InsertBefore(awake.Body.Instructions[0], Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(decayDisabled)));
            iLProcessor.InsertAfter(awake.Body.Instructions[0], Instruction.Create(OpCodes.Brtrue, awake.Body.Instructions[awake.Body.Instructions.Count - 1]));
            iLProcessor = doDecay.Body.GetILProcessor();
            iLProcessor.InsertBefore(doDecay.Body.Instructions[0], Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(decayDisabled)));
            iLProcessor.InsertAfter(doDecay.Body.Instructions[0], Instruction.Create(OpCodes.Brtrue, doDecay.Body.Instructions[doDecay.Body.Instructions.Count - 1]));
        }

        private void FieldsUpdatePatch()
        {
            TypeDefinition Metabolism = rustAssembly.MainModule.GetType("Metabolism");
            Metabolism.GetField("coreTemperature").SetPublic(true);
            Metabolism.GetField("caloricLevel").SetPublic(true);
            Metabolism.GetField("radiationLevel").SetPublic(true);
            Metabolism.GetField("waterLevelLitre").SetPublic(true);
            Metabolism.GetField("antiRads").SetPublic(true);
            Metabolism.GetField("poisonLevel").SetPublic(true);
            Metabolism.GetField("_lastWarmTime").SetPublic(true);

            TypeDefinition User = rustAssembly.MainModule.GetType("RustProto", "User");
            User.GetField("displayname_").SetPublic(true);

            TypeDefinition ResourceGivePair = rustAssembly.MainModule.GetType("ResourceGivePair");
            ResourceGivePair.GetField("_resourceItemDatablock").SetPublic(true);

            TypeDefinition StructureMaster = rustAssembly.MainModule.GetType("StructureMaster");
            StructureMaster.GetField("_structureComponents").SetPublic(true);

            TypeDefinition InventoryHolder = rustAssembly.MainModule.GetType("InventoryHolder");
            InventoryHolder.GetField("isPlayerInventory").SetPublic(true);
            MethodDefinition m = InventoryHolder.GetMethod("GetPlayerInventory");
            m.SetPublic(true);

            TypeDefinition PlayerInventory = rustAssembly.MainModule.GetType("PlayerInventory");
            PlayerInventory.GetField("_boundBPs").SetPublic(true);

            TypeDefinition BasicDoor = rustAssembly.MainModule.GetType("BasicDoor");
            BasicDoor.GetField("state").SetPublic(true);
            /*foreach (MethodDefinition met in BasicDoor.Methods)
            {
                if (met.Name.Equals("ToggleStateServer"))
                {
                    met.SetPublic(true);
                }
            }*/

            /*TypeDefinition Inventory = rustAssembly.MainModule.GetType("Inventory");
            TypeDefinition SlotOperationsInfo = Inventory.GetNestedType("SlotOperationsInfo").;
            Logger.Log(SlotOperationsInfo.ToString());*/

            /*TypeDefinition wildlifeManager = rustAssembly.MainModule.GetType("WildlifeManager");
            wildlifeManager.GetNestedType("Data").SetPublic(true);*/
        }

        private void MetaBolismMethodMod()
        {
            TypeDefinition Metabolism = rustAssembly.MainModule.GetType("Metabolism");
            MethodDefinition method = hooksClass.GetMethod("RecieveNetwork");
            MethodDefinition RecieveNetwork = Metabolism.GetMethod("RecieveNetwork");
            this.CloneMethod(RecieveNetwork);
            Array a = RecieveNetwork.Parameters.ToArray();
            RecieveNetwork.Body.Instructions.Clear();
            RecieveNetwork.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            foreach (ParameterDefinition p in a)
            {
                RecieveNetwork.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_S, p));
            }
            RecieveNetwork.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            RecieveNetwork.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            TypeDefinition logger = fougeriteAssembly.MainModule.GetType("Fougerite.Logger");
            MethodDefinition logex = logger.GetMethod("LogException");

            WrapMethod(RecieveNetwork, logex, rustAssembly, false);
        }

        private void HumanControllerPatch()
        {
            TypeDefinition HumanController = rustAssembly.MainModule.GetType("HumanController");
            MethodDefinition method = hooksClass.GetMethod("ClientMove");
            MethodDefinition GetClientMove = HumanController.GetMethod("GetClientMove");
            this.CloneMethod(GetClientMove);
            Array a = GetClientMove.Parameters.ToArray();
            Array.Reverse(a);
            ILProcessor iLProcessor = GetClientMove.Body.GetILProcessor();
            iLProcessor.InsertBefore(GetClientMove.Body.Instructions[0],
                Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            foreach (ParameterDefinition p in a)
            {
                iLProcessor.InsertBefore(GetClientMove.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_S, p));
            }
            iLProcessor.InsertBefore(GetClientMove.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));

            TypeDefinition logger = fougeriteAssembly.MainModule.GetType("Fougerite.Logger");
            MethodDefinition logex = logger.GetMethod("LogException");
            WrapMethod(GetClientMove, logex, rustAssembly, false);
        }

        private void CraftingPatch()
        {
            TypeDefinition CraftingInventory = rustAssembly.MainModule.GetType("CraftingInventory");
            MethodDefinition CancelCrafting = CraftingInventory.GetMethod("CancelCrafting");
            CancelCrafting.SetPublic(true);
            MethodDefinition StartCrafting = null;
            IEnumerable<MethodDefinition> allmethods = CraftingInventory.GetMethods();
            foreach (MethodDefinition m in allmethods)
            {
                if (m.Name.Equals("StartCrafting") && m.Parameters.Count == 3)
                {
                    StartCrafting = m;
                    break;
                }
            }
            MethodDefinition method = hooksClass.GetMethod("CraftingEvent");

            this.CloneMethod(StartCrafting);

            ILProcessor iLProcessor = StartCrafting.Body.GetILProcessor();
            int Position = StartCrafting.Body.Instructions.Count - 2;
            iLProcessor.InsertBefore(StartCrafting.Body.Instructions[Position], 
                Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(StartCrafting.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_3));
            iLProcessor.InsertBefore(StartCrafting.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertBefore(StartCrafting.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(StartCrafting.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_0));
        }

        private void NavMeshPatch()
        {
            TypeDefinition BasicWildLifeMovement = rustAssembly.MainModule.GetType("BaseAIMovement");
            MethodDefinition DoMove = BasicWildLifeMovement.GetMethod("DoMove");
            MethodDefinition method = hooksClass.GetMethod("AnimalMovement");
            this.CloneMethod(DoMove);

            ILProcessor iLProcessor = DoMove.Body.GetILProcessor();
            iLProcessor.InsertBefore(DoMove.Body.Instructions[0],
                Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(DoMove.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertBefore(DoMove.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(DoMove.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));

        }

        private void ResourceSpawned()
        {
            TypeDefinition ResourceTarget = rustAssembly.MainModule.GetType("ResourceTarget");
            ResourceTarget.GetField("gatherProgress").SetPublic(true);
            ResourceTarget.GetField("startingTotal").SetPublic(true);

            MethodDefinition Awake = ResourceTarget.GetMethod("Awake");
            this.CloneMethod(Awake);

            MethodDefinition method = hooksClass.GetMethod("ResourceSpawned");
            ILProcessor iLProcessor = Awake.Body.GetILProcessor();

            iLProcessor.InsertBefore(Awake.Body.Instructions[0],
                Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(Awake.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
        }

        private void InventoryModifications()
        {
            TypeDefinition Inventory = rustAssembly.MainModule.GetType("Inventory");
            Inventory.GetField("_netListeners").SetPublic(true);
            MethodDefinition ItemRemoved = Inventory.GetMethod("ItemRemoved");
            this.CloneMethod(ItemRemoved);

            MethodDefinition method = hooksClass.GetMethod("ItemRemoved");
            ILProcessor iLProcessor = ItemRemoved.Body.GetILProcessor();

            iLProcessor.InsertBefore(ItemRemoved.Body.Instructions[0],
                Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(ItemRemoved.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertBefore(ItemRemoved.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(ItemRemoved.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
            TypeDefinition logger = fougeriteAssembly.MainModule.GetType("Fougerite.Logger");
            MethodDefinition logex = logger.GetMethod("LogException");
            WrapMethod(ItemRemoved, logex, rustAssembly, false);
        }

        private void InventoryModifications2()
        {
            TypeDefinition Inventory = rustAssembly.MainModule.GetType("Inventory");
            MethodDefinition ItemAdded = Inventory.GetMethod("ItemAdded");
            ILProcessor iLProcessor = ItemAdded.Body.GetILProcessor();
            this.CloneMethod(ItemAdded);

            MethodDefinition method2 = hooksClass.GetMethod("ItemAdded");

            iLProcessor.InsertBefore(ItemAdded.Body.Instructions[0],
                Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method2)));
            iLProcessor.InsertBefore(ItemAdded.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertBefore(ItemAdded.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertBefore(ItemAdded.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
            TypeDefinition logger = fougeriteAssembly.MainModule.GetType("Fougerite.Logger");
            MethodDefinition logex = logger.GetMethod("LogException");
            WrapMethod(ItemAdded, logex, rustAssembly, false);
        }

        private void AirdropPatch()
        {
            TypeDefinition SupplyDropZone = rustAssembly.MainModule.GetType("SupplyDropZone");
            MethodDefinition CallAirDropAt = SupplyDropZone.GetMethod("CallAirDropAt");
            this.CloneMethod(CallAirDropAt);
            MethodDefinition method = hooksClass.GetMethod("Airdrop");
            ILProcessor iLProcessor = CallAirDropAt.Body.GetILProcessor();
            iLProcessor.InsertBefore(CallAirDropAt.Body.Instructions[0],
                Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            Logger.Log(CallAirDropAt.Body.Method.Parameters[0].ToString());
            iLProcessor.InsertBefore(CallAirDropAt.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
            //iLProcessor.InsertBefore(CallAirDropAt.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg, CallAirDropAt.Body.Method.Parameters[0]));
            //iLProcessor.InsertBefore(CallAirDropAt.Body.Instructions[0], Instruction.Create(method.Parameters[0]));

        }

        private void ClientConnectionPatch()
        {
            TypeDefinition ClientConnection = rustAssembly.MainModule.GetType("ClientConnection");
            MethodDefinition DenyAccess = ClientConnection.GetMethod("DenyAccess");
            ILProcessor iLProcessor = DenyAccess.Body.GetILProcessor();
            this.CloneMethod(DenyAccess);
            MethodDefinition method = hooksClass.GetMethod("SteamDeny");
            iLProcessor.Body.Instructions.Clear();
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_3));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private void ConnectionAcceptorPatch()
        {
            TypeDefinition ConnectionAcceptor = rustAssembly.MainModule.GetType("ConnectionAcceptor");
            MethodDefinition uLink_OnPlayerApproval = ConnectionAcceptor.GetMethod("uLink_OnPlayerApproval");
            ILProcessor iLProcessor = uLink_OnPlayerApproval.Body.GetILProcessor();
            this.CloneMethod(uLink_OnPlayerApproval);
            MethodDefinition method = hooksClass.GetMethod("PlayerApproval");
            iLProcessor.Body.Instructions.Clear();
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private void RayCastPatch()
        {
            AssemblyDefinition Unity = AssemblyDefinition.ReadAssembly("UnityEngine.dll");
            TypeDefinition Physics = Unity.MainModule.GetType("UnityEngine.Physics");
            IEnumerable<MethodDefinition> allmethods = Physics.GetMethods();
            TypeDefinition logger = fougeriteAssembly.MainModule.GetType("Fougerite.Logger");
            MethodDefinition logex = logger.GetMethod("LogException");

            TypeDefinition Object = Unity.MainModule.GetType("UnityEngine.Object");
            MethodDefinition d = Object.GetMethod("FindObjectOfType");
            Logger.Log(d.Parameters.ToString());
            foreach (MethodDefinition f in allmethods)
            {
                if (f.Name.Equals("RaycastAll"))
                {
                    //WrapMethodNull(f, logex, Unity, false);
                }
            }
            //WrapMethodReturnNull(d, logex, Unity, false);
            Unity.Write("UnityEngine.dll");
        }

        private void SupplyCratePatch()
        {
            TypeDefinition SupplyCrate = rustAssembly.MainModule.GetType("SupplyCrate");
            MethodDefinition FixedUpdate = SupplyCrate.GetMethod("FixedUpdate");
            MethodDefinition DoNetwork = SupplyCrate.GetMethod("DoNetwork");

            TypeDefinition logger = fougeriteAssembly.MainModule.GetType("Fougerite.Logger");
            MethodDefinition logex = logger.GetMethod("LogException");
            WrapMethod(FixedUpdate, logex, rustAssembly, false);
            WrapMethod(DoNetwork, logex, rustAssembly, false);
        }

        private void BootstrapAttachPatch()
        {
            TypeDefinition fougeriteBootstrap = fougeriteAssembly.MainModule.GetType("Fougerite.Bootstrap");
            TypeDefinition serverInit = rustAssembly.MainModule.GetType("ServerInit");
            MethodDefinition attachBootstrap = fougeriteBootstrap.GetMethod("AttachBootstrap");
            MethodDefinition awake = serverInit.GetMethod("Awake");
            awake.Body.GetILProcessor().InsertAfter(awake.Body.Instructions[0x74], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(attachBootstrap)));
        }

        private void EntityDecayPatch_StructureMaster()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("StructureMaster");
            MethodDefinition orig = type.GetMethod("DoDecay");
            MethodDefinition method = hooksClass.GetMethod("EntityDecay");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            iLProcessor.InsertBefore(orig.Body.Instructions[244], Instruction.Create(OpCodes.Stloc_S, orig.Body.Variables[6]));
            iLProcessor.InsertBefore(orig.Body.Instructions[244], Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[244], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[6]));
            iLProcessor.InsertBefore(orig.Body.Instructions[244], Instruction.Create(OpCodes.Ldloc_3));
        }

        private void EntityDecayPatch_EnvDecay()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("EnvDecay");
            MethodDefinition orig = type.GetMethod("DecayThink");
            MethodDefinition method = hooksClass.GetMethod("EntityDecay");
            FieldDefinition Field = type.GetField("_deployable");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Stloc_S, orig.Body.Variables[2]));
            iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Callvirt, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[2]));
            iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Ldfld, Field));
            iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Ldarg_0));
        }

        private void NPCHurtKilledPatch_BasicWildLifeAI()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("BasicWildLifeAI");
            MethodDefinition orig = type.GetMethod("OnHurt");
            MethodDefinition method = hooksClass.GetMethod("NPCHurt");

            MethodDefinition NPCKilled = type.GetMethod("OnKilled");
            MethodDefinition NPCKilledHook = hooksClass.GetMethod("NPCKilled");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarga_S, orig.Parameters[0]));

            iLProcessor = NPCKilled.Body.GetILProcessor();
            iLProcessor.InsertBefore(NPCKilled.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(NPCKilledHook)));
            iLProcessor.InsertBefore(NPCKilled.Body.Instructions[0], Instruction.Create(OpCodes.Ldarga_S, NPCKilled.Parameters[0]));
        }

        private void NPCHurtPatch_HostileWildlifeAI()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("HostileWildlifeAI");
            MethodDefinition orig = type.GetMethod("OnHurt");
            MethodDefinition method = hooksClass.GetMethod("NPCHurt");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarga_S, orig.Parameters[0]));
        }

        private void PlayerSpawningSpawnedPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("ServerManagement");
            MethodDefinition orig = type.GetMethod("SpawnPlayer");
            MethodDefinition method = hooksClass.GetMethod("PlayerSpawning");
            MethodDefinition SpawnedHook = hooksClass.GetMethod("PlayerSpawned");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();

            // 141 - playerFor.hasLastKnownPosition = true;
            int Position = orig.Body.Instructions.Count - 2;
            iLProcessor.InsertBefore(orig.Body.Instructions[Position], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(SpawnedHook)));
            iLProcessor.InsertBefore(orig.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertBefore(orig.Body.Instructions[Position], Instruction.Create(OpCodes.Ldloc_0));
            iLProcessor.InsertBefore(orig.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));

            // 114 - user.truthDetector.NoteTeleported(zero, 0.0);
            iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Stloc_0));
            iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Ldarg_2));
            iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Ldloc_0));
            iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Ldarg_1));
        }

        private void ServerShutdownPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("LibRust");
            MethodDefinition orig = type.GetMethod("OnDestroy");
            MethodDefinition method = hooksClass.GetMethod("ServerShutdown");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 5 - Shutdown();
            iLProcessor.InsertBefore(orig.Body.Instructions[5], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(method)));
        }

        private void PlayerGatherWoodPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("MeleeWeaponDataBlock");
            MethodDefinition orig = type.GetMethod("DoAction1");
            MethodDefinition method = hooksClass.GetMethod("PlayerGatherWood");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 184 - if (byName != null)
            iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloca_S, orig.Body.Variables[16]));
            iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloca_S, orig.Body.Variables[14]));
            iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloca_S, orig.Body.Variables[17]));
            iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[11]));
            iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[5]));
        }

        private void PlayerGatherPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("ResourceTarget");
            MethodDefinition orig = type.GetMethod("DoGather");
            MethodDefinition method = hooksClass.GetMethod("PlayerGather");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 30 - int amount = (int) Mathf.Abs(this.gatherProgress);
            iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Ldloca, orig.Body.Variables[1]));
            iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Ldloc_0));
            iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Ldarg_0));
            iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Ldarg_1));
        }

        private void EntityDeployedPatch_DeployableItemDataBlock()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("DeployableItemDataBlock");
            MethodDefinition orig = type.GetMethod("DoAction1");
            MethodDefinition method = hooksClass.GetMethod("EntityDeployed");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 60 - leave (end of try block)
            iLProcessor.InsertBefore(orig.Body.Instructions[60], Instruction.Create(OpCodes.Call, this.rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[60], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[8]));
        }

        private void EntityDeployedPatch_StructureComponentDataBlock()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("StructureComponentDataBlock");
            MethodDefinition orig = type.GetMethod("DoAction1");
            MethodDefinition method = hooksClass.GetMethod("EntityDeployed");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 102 - int count = 1;
            iLProcessor.InsertBefore(orig.Body.Instructions[102], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[102], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[8]));
        }

        private void BlueprintUsePatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("BlueprintDataBlock");
            MethodDefinition orig = type.GetMethod("UseItem");
            MethodDefinition method = hooksClass.GetMethod("BlueprintUse");

            this.CloneMethod(orig);
            orig.Body.Instructions.Clear();
            orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            orig.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(method)));
            orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private void ChatPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("chat");
            MethodDefinition orig = type.GetMethod("say");
            MethodDefinition method = hooksClass.GetMethod("ChatReceived");

            this.CloneMethod(orig);
            orig.Body.Instructions.Clear();
            orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            orig.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(method)));
            orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private MethodDefinition CloneMethod(MethodDefinition orig) // Method Backuping
        {
            MethodDefinition definition = new MethodDefinition(orig.Name + "Original", orig.Attributes, orig.ReturnType);
            foreach (VariableDefinition definition2 in orig.Body.Variables)
            {
                definition.Body.Variables.Add(definition2);
            }
            foreach (ParameterDefinition definition3 in orig.Parameters)
            {
                definition.Parameters.Add(definition3);
            }
            foreach (Instruction instruction in orig.Body.Instructions)
            {
                definition.Body.Instructions.Add(instruction);
            }
            return definition;
        }

        private void ConsolePatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("ConsoleSystem");
            MethodDefinition orig = type.GetMethod("RunCommand");
            MethodDefinition method = hooksClass.GetMethod("ConsoleReceived");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            for (int i = 0; i < 8; i++)
            {
                iLProcessor.Remove(orig.Body.Instructions[11]);
            }
            iLProcessor.InsertBefore(orig.Body.Instructions[11], Instruction.Create(OpCodes.Ret));
            iLProcessor.InsertBefore(orig.Body.Instructions[11], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[11], Instruction.Create(OpCodes.Ldarg_0));
        }

        private void DoorSharing()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("DeployableObject");
            MethodDefinition definition2 = type.GetMethod("BelongsTo");
            MethodDefinition method = hooksClass.GetMethod("CheckOwner");

            definition2.Body.Instructions.Clear();
            definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
            definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
            definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(method)));
            definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private void EntityHurtPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("StructureComponent");
            TypeDefinition definition2 = rustAssembly.MainModule.GetType("DeployableObject");
            MethodDefinition definition3 = type.GetMethod("OnHurt");
            MethodDefinition definition4 = definition2.GetMethod("OnHurt");
            MethodDefinition method = hooksClass.GetMethod("EntityHurt");

            MethodReference reference = rustAssembly.MainModule.Import(method);
            definition3.Body.Instructions.Clear();
            definition3.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            definition3.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarga_S, definition3.Parameters[0]));
            definition3.Body.Instructions.Add(Instruction.Create(OpCodes.Call, reference));
            definition3.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            definition4.Body.Instructions.Clear();
            definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarga_S, definition4.Parameters[0]));
            definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Call, reference));
            definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        private void ItemsTablesLoadedPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("DatablockDictionary");
            MethodDefinition orig = type.GetMethod("Initialize");
            MethodDefinition method = hooksClass.GetMethod("ItemsLoaded");
            MethodDefinition definition4 = hooksClass.GetMethod("TablesLoaded");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            for (int i = 0; i < 13; i++)
            {
                iLProcessor.Remove(orig.Body.Instructions[0x17]);
            }
            orig.Body.Instructions[0x24] = Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method));
            iLProcessor.InsertBefore(orig.Body.Instructions[0x24], Instruction.Create(OpCodes.Ldsfld, type.Fields[2]));
            iLProcessor.InsertBefore(orig.Body.Instructions[0x24], Instruction.Create(OpCodes.Ldsfld, type.Fields[1]));
            iLProcessor.InsertBefore(orig.Body.Instructions[0x3f], Instruction.Create(OpCodes.Stsfld, type.Fields[4]));
            iLProcessor.InsertBefore(orig.Body.Instructions[0x3f], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(definition4)));
            iLProcessor.InsertBefore(orig.Body.Instructions[0x3f], Instruction.Create(OpCodes.Ldsfld, type.Fields[4]));
        }

        private void PlayerHurtPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("HumanBodyTakeDamage");
            MethodDefinition orig = type.GetMethod("Hurt");
            MethodDefinition method = hooksClass.GetMethod("PlayerHurt");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor.InsertAfter(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(method)));
        }

        private void PlayerJoinLeavePatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("RustServerManagement");
            TypeDefinition definition2 = rustAssembly.MainModule.GetType("ConnectionAcceptor");
            MethodDefinition orig = type.GetMethod("OnUserConnected");
            MethodDefinition method = hooksClass.GetMethod("PlayerConnect");
            MethodDefinition definition5 = definition2.GetMethod("uLink_OnPlayerDisconnected");
            MethodDefinition definition6 = hooksClass.GetMethod("PlayerDisconnect");

            this.CloneMethod(orig);
            this.CloneMethod(definition5);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Brfalse, orig.Body.Instructions[orig.Body.Instructions.Count - 1]));
            iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
            iLProcessor = definition5.Body.GetILProcessor();
            iLProcessor.InsertAfter(definition5.Body.Instructions[0x23], Instruction.Create(OpCodes.Ldloc_1));
            iLProcessor.InsertAfter(definition5.Body.Instructions[0x24], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(definition6)));
        }

        private void PlayerKilledPatch()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("HumanController");
            MethodDefinition orig = type.GetMethod("OnKilled");
            MethodDefinition method = hooksClass.GetMethod("PlayerKilled");

            this.CloneMethod(orig);
            ILProcessor iLProcessor = orig.Body.GetILProcessor();
            iLProcessor.InsertAfter(orig.Body.Instructions[0x15], Instruction.Create(OpCodes.Ldarga_S, orig.Parameters[0]));
            iLProcessor.InsertAfter(orig.Body.Instructions[0x16], Instruction.Create(OpCodes.Callvirt, rustAssembly.MainModule.Import(method)));
            iLProcessor.InsertAfter(orig.Body.Instructions[0x17], Instruction.Create(OpCodes.Brfalse, orig.Body.Instructions[0x2f]));
            orig.Body.Instructions[0x11] = Instruction.Create(OpCodes.Brfalse, orig.Body.Instructions[0x16]);
        }

        private void TalkerNotifications()
        {
            TypeDefinition type = rustAssembly.MainModule.GetType("VoiceCom");
            TypeDefinition definition2 = rustAssembly.MainModule.GetType("PlayerClient");
            MethodDefinition definition3 = type.GetMethod("clientspeak");
            MethodDefinition method = hooksClass.GetMethod("ShowTalker");
            FieldDefinition field = definition2.GetField("netPlayer");
            VariableDefinition variable = null;
            variable = definition3.Body.Variables[6];

            ILProcessor iLProcessor = definition3.Body.GetILProcessor();
            iLProcessor.InsertAfter(definition3.Body.Instructions[0x57], Instruction.Create(OpCodes.Ldloc_S, variable));
            iLProcessor.InsertAfter(definition3.Body.Instructions[0x58], Instruction.Create(OpCodes.Ldfld, rustAssembly.MainModule.Import(field)));
            iLProcessor.InsertAfter(definition3.Body.Instructions[0x59], Instruction.Create(OpCodes.Ldloc_0));
            iLProcessor.InsertAfter(definition3.Body.Instructions[90], Instruction.Create(OpCodes.Call, rustAssembly.MainModule.Import(method)));
        }

        public bool FirstPass()
        {
            try
            {
                bool flag = true;

                if (rustAssembly.MainModule.GetType("Fougerite_Patched_FirstPass") != null)
                {
                    Logger.Log("Assembly-CSharp.dll is already patched, please use a clean library.");
                    return false;
                }

                try
                {
                    this.FieldsUpdatePatch();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    flag = false;
                }

                try
                {
                    TypeReference type = AssemblyDefinition.ReadAssembly("mscorlib.dll").MainModule.GetType("System.String");
                    TypeDefinition item = new TypeDefinition("", "Fougerite_Patched_FirstPass", TypeAttributes.AnsiClass | TypeAttributes.Public);
                    rustAssembly.MainModule.Types.Add(item);
                    TypeReference fieldType = rustAssembly.MainModule.Import(type);
                    FieldDefinition definition3 = new FieldDefinition("Version", FieldAttributes.CompilerControlled | FieldAttributes.FamANDAssem | FieldAttributes.Family, fieldType);
                    definition3.HasConstant = true;
                    definition3.Constant = Program.Version;
                    rustAssembly.MainModule.GetType("Fougerite_Patched_FirstPass").Fields.Add(definition3);
                    rustAssembly.Write("Assembly-CSharp.dll");
                }
                catch (Exception ex)
                {
                    flag = false;
                    Logger.Log(ex);
                }
                return flag;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        public bool SecondPass()
        {
            Logger.Log("Prepraching LatePost method...");
            this.LatePostInTryCatch();
            rustAssembly.Write("Assembly-CSharp.dll");
            Logger.Log("Success! Patching other methods...");
            try
            {
                bool flag = true;

                fougeriteAssembly = AssemblyDefinition.ReadAssembly("Fougerite.dll");
                hooksClass = fougeriteAssembly.MainModule.GetType("Fougerite.Hooks");


                if (rustAssembly.MainModule.GetType("Fougerite_Patched_SecondPass") != null)
                {
                    Logger.Log("Assembly-CSharp.dll is already patched, please use a clean library.");
                    return false;
                }

                try
                {
                    this.WrapWildlifeUpdateInTryCatch();
                    this.uLinkLateUpdateInTryCatch();

                    this.BootstrapAttachPatch();
                    this.NPCHurtKilledPatch_BasicWildLifeAI();
                    this.EntityDecayPatch_StructureMaster();
                    this.EntityDecayPatch_EnvDecay();
                    this.NPCHurtPatch_HostileWildlifeAI();
                    this.ServerShutdownPatch();
                    this.BlueprintUsePatch();
                    this.EntityDeployedPatch_DeployableItemDataBlock();
                    this.EntityDeployedPatch_StructureComponentDataBlock();
                    this.PlayerGatherWoodPatch();
                    this.PlayerGatherPatch();
                    this.PlayerSpawningSpawnedPatch();
                    this.ChatPatch();
                    this.ConsolePatch();
                    this.PlayerJoinLeavePatch();
                    this.PlayerKilledPatch();
                    this.PlayerHurtPatch();
                    this.EntityHurtPatch();
                    this.ItemsTablesLoadedPatch();
                    this.DoorSharing();
                    this.TalkerNotifications();
                    this.MetaBolismMethodMod();
                    this.CraftingPatch();
                    this.NavMeshPatch();
                    this.ResourceSpawned();
                    this.InventoryModifications();
                    this.InventoryModifications2();
                    //this.AirdropPatch();
                    this.ClientConnectionPatch();
                    this.ConnectionAcceptorPatch();
                    this.HumanControllerPatch();
                    this.SupplyCratePatch();
                    this.LatePostInTryCatch2();
                    //this.RayCastPatch();
                    this.ResearchPatch();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    flag = false;
                }

                try
                {
                    TypeReference type = AssemblyDefinition.ReadAssembly("mscorlib.dll").MainModule.GetType("System.String");
                    TypeDefinition item = new TypeDefinition("", "Fougerite_Patched_SecondPass", TypeAttributes.AnsiClass | TypeAttributes.Public);
                    rustAssembly.MainModule.Types.Add(item);
                    TypeReference fieldType = rustAssembly.MainModule.Import(type);
                    FieldDefinition definition3 = new FieldDefinition("Version", FieldAttributes.CompilerControlled | FieldAttributes.FamANDAssem | FieldAttributes.Family, fieldType);
                    definition3.HasConstant = true;
                    definition3.Constant = Program.Version;
                    rustAssembly.MainModule.GetType("Fougerite_Patched_SecondPass").Fields.Add(definition3);
                    rustAssembly.Write("Assembly-CSharp.dll");
                }
                catch (Exception ex)
                {
                    flag = false;
                    Logger.Log(ex);
                }
                return flag;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
        }

        private void WrapMethod(MethodDefinition md, MethodDefinition origMethod, AssemblyDefinition asm, bool logEx = false)
        {
            Instruction instruction2;
            ILProcessor iLProcessor = md.Body.GetILProcessor();
            //Instruction instruction = Instruction.Create(OpCodes.Ldarg_0);
            if (md.ReturnType.Name == "Void")
            {
                instruction2 = md.Body.Instructions[md.Body.Instructions.Count - 1];
            }
            else
            {
                instruction2 = md.Body.Instructions[md.Body.Instructions.Count - 2];
            }
            //iLProcessor.InsertBefore(instruction2, instruction);

            iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Nop));
            iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Leave, instruction2));

            TypeReference type = AssemblyDefinition.ReadAssembly("mscorlib.dll").MainModule.GetType("System.Exception");
            md.Body.Variables.Add(new VariableDefinition("ex", asm.MainModule.Import(type))); ;

            Instruction instruction = null;
            if (logEx)
            {
                instruction = Instruction.Create(OpCodes.Stloc_0);
                iLProcessor.InsertBefore(instruction2, instruction);
                iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Nop));
                iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Ldloc_0));
                iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Ldnull));

                for (int i = 0; i < md.Parameters.Count; i++)
                {
                    iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Ldarga_S, md.Parameters[i]));
                }
                iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Call, asm.MainModule.Import(origMethod)));
            }
            else
            {
                instruction = Instruction.Create(OpCodes.Nop);
                iLProcessor.InsertBefore(instruction2, instruction);
                iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Nop));
            }
            iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Nop));
            iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Leave, instruction2));

            ExceptionHandler item = new ExceptionHandler(ExceptionHandlerType.Catch);
            item.TryStart = md.Body.Instructions[0];
            item.TryEnd = instruction;
            item.HandlerStart = instruction;
            item.HandlerEnd = instruction2;
            if (md.ReturnType.Name != "Void")
            {
                Instruction instruction3 = Instruction.Create(OpCodes.Ret);
                iLProcessor.InsertBefore(instruction2, instruction3);
            }

            item.CatchType = asm.MainModule.Import(type);
            md.Body.ExceptionHandlers.Add(item);
        }
    }
}