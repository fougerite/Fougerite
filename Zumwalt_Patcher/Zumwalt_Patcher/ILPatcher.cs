namespace Zumwalt_Patcher
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;

    public class ILPatcher
    {
        private AssemblyDefinition cSharpASM;
        //private AssemblyDefinition firstPassASM;
        private TypeDefinition HooksClass;
        private AssemblyDefinition ZumwaltAsm;

        public ILPatcher()
        {
            try
            {
                this.ZumwaltAsm = AssemblyDefinition.ReadAssembly("Zumwalt.dll");
                this.HooksClass = this.ZumwaltAsm.MainModule.GetType("Zumwalt.Hooks");
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private bool AccessPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("Character");
            MethodDefinition onDestroy = type.GetMethod ("OnDestroy");
            if (onDestroy == null) 
            {
                return false;
            }

            onDestroy.IsPublic = true;
            onDestroy.IsPrivate = false;
            return true;
        }

        private bool AntiDecay()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("EnvDecay");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("StructureMaster");

            MethodDefinition awake = type.GetMethod("Awake");
            MethodDefinition doDecay = type.GetMethod("DoDecay");
            MethodDefinition decayDisabled = this.HooksClass.GetMethod("DecayDisabled");

            if (((awake == null) || (doDecay == null)) || (decayDisabled == null)) 
            {
                return false;
            }

            try
            {
                ILProcessor iLProcessor = awake.Body.GetILProcessor();
                iLProcessor.InsertBefore(awake.Body.Instructions[0], Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(decayDisabled)));
                iLProcessor.InsertAfter(awake.Body.Instructions[0], Instruction.Create(OpCodes.Brtrue, awake.Body.Instructions[awake.Body.Instructions.Count - 1]));
                iLProcessor = doDecay.Body.GetILProcessor();
                iLProcessor.InsertBefore(doDecay.Body.Instructions[0], Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(decayDisabled)));
                iLProcessor.InsertAfter(doDecay.Body.Instructions[0], Instruction.Create(OpCodes.Brtrue, doDecay.Body.Instructions[doDecay.Body.Instructions.Count - 1]));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool FieldsUpdatePatch()
        {
            try
            {
                TypeDefinition Metabolism = cSharpASM.MainModule.GetType("Metabolism");
                Metabolism.GetField("coreTemperature").SetPublic(true);

                TypeDefinition User = cSharpASM.MainModule.GetType("RustProto", "User");
                User.GetField("displayname_").SetPublic(true);

                TypeDefinition ResourceGivePair = cSharpASM.MainModule.GetType("ResourceGivePair");
                ResourceGivePair.GetField("_resourceItemDatablock").SetPublic(true);

                TypeDefinition StructureMaster = cSharpASM.MainModule.GetType("StructureMaster");
                StructureMaster.GetField("_structureComponents").SetPublic(true);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }

            return true;
        }

        private bool BootstrapAttachPatch()
        {
            TypeDefinition zumwaltBootstrap = this.ZumwaltAsm.MainModule.GetType("Zumwalt.Bootstrap");
            TypeDefinition serverInit = this.cSharpASM.MainModule.GetType("ServerInit");
            MethodDefinition attachBootstrap = zumwaltBootstrap.GetMethod("AttachBootstrap");
            MethodDefinition awake = serverInit.GetMethod("Awake");
            if ((attachBootstrap == null) || (awake == null))
            {
                return false;
            }

            try
            {
                awake.Body.GetILProcessor().InsertAfter(awake.Body.Instructions[0x74], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(attachBootstrap)));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }

            return true;
        }

        private bool EntityDecayPatch_StructureMaster()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("StructureMaster");
            MethodDefinition orig = type.GetMethod("DoDecay");
            MethodDefinition method = this.HooksClass.GetMethod("EntityDecay");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                iLProcessor.InsertBefore(orig.Body.Instructions[244], Instruction.Create(OpCodes.Stloc_S, orig.Body.Variables[6]));
                iLProcessor.InsertBefore(orig.Body.Instructions[244], Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[244], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[6]));
                iLProcessor.InsertBefore(orig.Body.Instructions[244], Instruction.Create(OpCodes.Ldloc_3));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        private bool EntityDecayPatch_EnvDecay()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("EnvDecay");
            MethodDefinition orig = type.GetMethod("DecayThink");
            MethodDefinition method = this.HooksClass.GetMethod("EntityDecay");
            FieldDefinition Field = type.GetField("_deployable");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Stloc_S, orig.Body.Variables[2]));
                iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[2]));
                iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Ldfld, Field));
                iLProcessor.InsertBefore(orig.Body.Instructions[49], Instruction.Create(OpCodes.Ldarg_0));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        private bool NPCHurtKilledPatch_BasicWildLifeAI()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("BasicWildLifeAI");
            MethodDefinition orig = type.GetMethod("OnHurt");
            MethodDefinition method = this.HooksClass.GetMethod("NPCHurt");

            MethodDefinition NPCKilled = type.GetMethod("OnKilled");
            MethodDefinition NPCKilledHook = this.HooksClass.GetMethod("NPCKilled");

            if ((orig == null) || (method == null) || (NPCKilled == null) || (NPCKilledHook == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarga_S, orig.Parameters[0]));
                
                iLProcessor = NPCKilled.Body.GetILProcessor();
                iLProcessor.InsertBefore(NPCKilled.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(NPCKilledHook)));
                iLProcessor.InsertBefore(NPCKilled.Body.Instructions[0], Instruction.Create(OpCodes.Ldarga_S, NPCKilled.Parameters[0]));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        private bool NPCHurtPatch_HostileWildlifeAI()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("HostileWildlifeAI");
            MethodDefinition orig = type.GetMethod("OnHurt");
            MethodDefinition method = this.HooksClass.GetMethod("NPCHurt");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarga_S, orig.Parameters[0]));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool PlayerSpawningSpawnedPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("ServerManagement");
            MethodDefinition orig = type.GetMethod("SpawnPlayer");
            MethodDefinition method = this.HooksClass.GetMethod("PlayerSpawning");
            MethodDefinition SpawnedHook = this.HooksClass.GetMethod("PlayerSpawned");

            if (orig == null || method == null || SpawnedHook == null)
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();

                // 141 - playerFor.hasLastKnownPosition = true;
                int Position = orig.Body.Instructions.Count - 2;
                iLProcessor.InsertBefore(orig.Body.Instructions[Position], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(SpawnedHook)));
                iLProcessor.InsertBefore(orig.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_2));
                iLProcessor.InsertBefore(orig.Body.Instructions[Position], Instruction.Create(OpCodes.Ldloc_0));
                iLProcessor.InsertBefore(orig.Body.Instructions[Position], Instruction.Create(OpCodes.Ldarg_1));

                // 114 - user.truthDetector.NoteTeleported(zero, 0.0);
                iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Stloc_0));
                iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Ldarg_2));
                iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Ldloc_0));
                iLProcessor.InsertBefore(orig.Body.Instructions[114], Instruction.Create(OpCodes.Ldarg_1));


            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool ServerShutdownPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("LibRust");
            MethodDefinition orig = type.GetMethod("OnDestroy");
            MethodDefinition method = this.HooksClass.GetMethod("ServerShutdown");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 5 - Shutdown();
                iLProcessor.InsertBefore(orig.Body.Instructions[5], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool PlayerGatherWoodPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("MeleeWeaponDataBlock");
            MethodDefinition orig = type.GetMethod("DoAction1");
            MethodDefinition method = this.HooksClass.GetMethod("PlayerGatherWood");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 184 - if (byName != null)
                iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloca_S, orig.Body.Variables[16]));
                iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloca_S, orig.Body.Variables[14]));
                iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloca_S, orig.Body.Variables[17]));
                iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[11]));
                iLProcessor.InsertBefore(orig.Body.Instructions[184], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[5]));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool PlayerGatherPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("ResourceTarget");
            MethodDefinition orig = type.GetMethod("DoGather");
            MethodDefinition method = this.HooksClass.GetMethod("PlayerGather");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 30 - int amount = (int) Mathf.Abs(this.gatherProgress);
                iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Ldloca, orig.Body.Variables[1]));
                iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Ldloc_0));
                iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Ldarg_0));
                iLProcessor.InsertBefore(orig.Body.Instructions[30], Instruction.Create(OpCodes.Ldarg_1));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool EntityDeployedPatch_DeployableItemDataBlock()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("DeployableItemDataBlock");
            MethodDefinition orig = type.GetMethod("DoAction1");
            MethodDefinition method = this.HooksClass.GetMethod("EntityDeployed");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 60 - leave (end of try block)
                iLProcessor.InsertBefore(orig.Body.Instructions[60], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[60], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[8]));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool EntityDeployedPatch_StructureComponentDataBlock()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("StructureComponentDataBlock");
            MethodDefinition orig = type.GetMethod("DoAction1");
            MethodDefinition method = this.HooksClass.GetMethod("EntityDeployed");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor(); // 102 - int count = 1;
                iLProcessor.InsertBefore(orig.Body.Instructions[102], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[102], Instruction.Create(OpCodes.Ldloc_S, orig.Body.Variables[8]));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool BlueprintUsePatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("BlueprintDataBlock");
            MethodDefinition orig = type.GetMethod("UseItem");
            MethodDefinition method = this.HooksClass.GetMethod("BlueprintUse");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                orig.Body.Instructions.Clear();
                orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                orig.Body.Instructions.Add(Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool ChatPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("chat");
            MethodDefinition orig = type.GetMethod("say");
            MethodDefinition method = this.HooksClass.GetMethod("ChatReceived");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                orig.Body.Instructions.Clear();
                orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                orig.Body.Instructions.Add(Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                orig.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
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

        private bool ConsolePatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("ConsoleSystem");
            MethodDefinition orig = type.GetMethod("RunCommand");
            MethodDefinition method = this.HooksClass.GetMethod("ConsoleReceived");

            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                for (int i = 0; i < 8; i++)
                {
                    iLProcessor.Remove(orig.Body.Instructions[11]);
                }
                iLProcessor.InsertBefore(orig.Body.Instructions[11], Instruction.Create(OpCodes.Ret));
                iLProcessor.InsertBefore(orig.Body.Instructions[11], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[11], Instruction.Create(OpCodes.Ldarg_0));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool DoorSharing()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("DeployableObject");
            MethodDefinition definition2 = type.GetMethod("BelongsTo");
            MethodDefinition method = this.HooksClass.GetMethod("CheckOwner");
            if ((definition2 == null) || (method == null))
            {
                return false;
            }
            try
            {
                definition2.Body.Instructions.Clear();
                definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Nop));
                definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                definition2.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool EntityHurtPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("StructureComponent");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("DeployableObject");
            MethodDefinition definition3 = type.GetMethod("OnHurt");
            MethodDefinition definition4 = definition2.GetMethod("OnHurt");
            MethodDefinition method = this.HooksClass.GetMethod("EntityHurt");

            if (((definition3 == null) || (definition4 == null)) || (method == null))
            {
                return false;
            }
            try
            {
                MethodReference reference = this.cSharpASM.MainModule.Import(method);
                definition3.Body.Instructions.Clear();
                definition3.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                definition3.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                definition3.Body.Instructions.Add(Instruction.Create(OpCodes.Call, reference));
                definition3.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                definition4.Body.Instructions.Clear();
                definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarga_S, definition4.Parameters[0]));
                definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Call, reference));
                definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool ItemsTablesLoadedPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("DatablockDictionary");
            MethodDefinition orig = type.GetMethod("Initialize");
            MethodDefinition method = this.HooksClass.GetMethod("ItemsLoaded");
            MethodDefinition definition4 = this.HooksClass.GetMethod("TablesLoaded");
            if (((orig == null) || (method == null)) || (definition4 == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                for (int i = 0; i < 13; i++)
                {
                    iLProcessor.Remove(orig.Body.Instructions[0x17]);
                }
                orig.Body.Instructions[0x24] = Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(method));
                iLProcessor.InsertBefore(orig.Body.Instructions[0x24], Instruction.Create(OpCodes.Ldsfld, type.Fields[2]));
                iLProcessor.InsertBefore(orig.Body.Instructions[0x24], Instruction.Create(OpCodes.Ldsfld, type.Fields[1]));
                iLProcessor.InsertBefore(orig.Body.Instructions[0x3f], Instruction.Create(OpCodes.Stsfld, type.Fields[4]));
                iLProcessor.InsertBefore(orig.Body.Instructions[0x3f], Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(definition4)));
                iLProcessor.InsertBefore(orig.Body.Instructions[0x3f], Instruction.Create(OpCodes.Ldsfld, type.Fields[4]));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        public bool Patch()
        {
            try
            {
                bool flag = true;
                this.cSharpASM = AssemblyDefinition.ReadAssembly("Assembly-CSharp.dll");
                //this.firstPassASM = AssemblyDefinition.ReadAssembly("Assembly-CSharp-firstpass.dll");
                if (this.cSharpASM.MainModule.GetType("Zumwalt_Patched") != null)
                {
                    Logger.Log("Assembly-CSharp.dll is already patched, please use a clean library.");
                    return false;
                }
                if (!this.BootstrapAttachPatch())
                {
                    Logger.Log("Error while applying 'BootstrapAttach' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.FieldsUpdatePatch())
                {
                    Logger.Log("Error while applying 'FieldsUpdate' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.NPCHurtKilledPatch_BasicWildLifeAI())
                {
                    Logger.Log("Error while applying 'NPCHurtKilled BasicWildLifeAI' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.EntityDecayPatch_StructureMaster())
                {
                    Logger.Log("Error while applying 'EntityDecayPatch StructureMaster' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.EntityDecayPatch_EnvDecay())
                {
                    Logger.Log("Error while applying 'EntityDecayPatch EnvDecay' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.NPCHurtPatch_HostileWildlifeAI())
                {
                    Logger.Log("Error while applying 'NPCHurtPatch HostileWildlifeAI' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.ServerShutdownPatch())
                {
                    Logger.Log("Error while applying 'ServerShutdown' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.BlueprintUsePatch())
                {
                    Logger.Log("Error while applying 'BlueprintUse' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.EntityDeployedPatch_DeployableItemDataBlock())
                {
                    Logger.Log("Error while applying 'EntityDeployed DeployableItemDataBlock' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.EntityDeployedPatch_StructureComponentDataBlock())
                {
                    Logger.Log("Error while applying 'EntityDeployed StructureComponentDataBlock' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.PlayerGatherWoodPatch())
                {
                    Logger.Log("Error while applying 'PlayerGatherWood' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.PlayerGatherPatch())
                {
                    Logger.Log("Error while applying 'PlayerGather' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.PlayerSpawningSpawnedPatch())
                {
                    Logger.Log("Error while applying 'PlayerSpawningSpawned' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.ChatPatch())
                {
                    Logger.Log("Error while applying 'Chat' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.ConsolePatch())
                {
                    Logger.Log("Error while applying 'Console' Patch to Assembly-CSharp-firstpass.dll");
                    flag = false;
                }
                if (!this.PlayerJoinLeavePatch())
                {
                    Logger.Log("Error while applying 'PlayerJoinLeave' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.PlayerKilledPatch())
                {
                    Logger.Log("Error while applying 'PlayerKilled' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.PlayerHurtPatch())
                {
                    Logger.Log("Error while applying 'PlayerHurt' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.EntityHurtPatch())
                {
                    Logger.Log("Error while applying 'EntityHurt' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.ItemsTablesLoadedPatch())
                {
                    Logger.Log("Error while applying 'ItemsTablesLoaded' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.DoorSharing())
                {
                    Logger.Log("Error while applying 'DoorSharing' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                if (!this.TalkerNotifications())
                {
                    Logger.Log("Error while applying 'TalkerNotification' Patch to Assembly-CSharp.dll");
                    flag = false;
                }
                try
                {
                    TypeReference type = AssemblyDefinition.ReadAssembly("mscorlib.dll").MainModule.GetType("System.String");
                    TypeDefinition item = new TypeDefinition("", "Zumwalt_Patched", TypeAttributes.AnsiClass | TypeAttributes.Public);
                    this.cSharpASM.MainModule.Types.Add(item);
                    TypeReference fieldType = this.cSharpASM.MainModule.Import(type);
                    FieldDefinition definition3 = new FieldDefinition("Version", FieldAttributes.CompilerControlled | FieldAttributes.FamANDAssem | FieldAttributes.Family, fieldType);
                    definition3.HasConstant = true;
                    definition3.Constant = Program.Version;
                    this.cSharpASM.MainModule.GetType("Zumwalt_Patched").Fields.Add(definition3);
                    this.cSharpASM.Write("Assembly-CSharp.dll");
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

        private bool PlayerHurtPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("HumanBodyTakeDamage");
            MethodDefinition orig = type.GetMethod("Hurt");
            MethodDefinition method = this.HooksClass.GetMethod("PlayerHurt");
            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
                iLProcessor.InsertAfter(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool PlayerJoinLeavePatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("RustServerManagement");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("ConnectionAcceptor");
            MethodDefinition orig = type.GetMethod("OnUserConnected");
            MethodDefinition method = this.HooksClass.GetMethod("PlayerConnect");
            MethodDefinition definition5 = definition2.GetMethod("uLink_OnPlayerDisconnected");
            MethodDefinition definition6 = this.HooksClass.GetMethod("PlayerDisconnect");

            if (((orig == null) || (method == null)) || ((definition5 == null) || (definition6 == null)))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                this.CloneMethod(definition5);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Brfalse, orig.Body.Instructions[orig.Body.Instructions.Count - 1]));
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_1));
                iLProcessor = definition5.Body.GetILProcessor();
                iLProcessor.InsertAfter(definition5.Body.Instructions[0x23], Instruction.Create(OpCodes.Ldloc_1));
                iLProcessor.InsertAfter(definition5.Body.Instructions[0x24], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(definition6)));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool PlayerKilledPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("HumanController");
            MethodDefinition orig = type.GetMethod("OnKilled");
            MethodDefinition method = this.HooksClass.GetMethod("PlayerKilled");
            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                iLProcessor.InsertAfter(orig.Body.Instructions[0x15], Instruction.Create(OpCodes.Ldarga_S, orig.Parameters[0]));
                iLProcessor.InsertAfter(orig.Body.Instructions[0x16], Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertAfter(orig.Body.Instructions[0x17], Instruction.Create(OpCodes.Brfalse, orig.Body.Instructions[0x2f]));
                orig.Body.Instructions[0x11] = Instruction.Create(OpCodes.Brfalse, orig.Body.Instructions[0x16]);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private bool TalkerNotifications()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("VoiceCom");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("PlayerClient");
            MethodDefinition definition3 = type.GetMethod("clientspeak");
            MethodDefinition method = HooksClass.GetMethod("ShowTalker");
            FieldDefinition field = definition2.GetField("netPlayer");
            VariableDefinition variable = null;
            variable = definition3.Body.Variables[6];

            if (((definition3 == null) || (method == null)) || ((field == null) || (variable == null)))
            {
                return false;
            }
            try
            {
                ILProcessor iLProcessor = definition3.Body.GetILProcessor();
                iLProcessor.InsertAfter(definition3.Body.Instructions[0x57], Instruction.Create(OpCodes.Ldloc_S, variable));
                iLProcessor.InsertAfter(definition3.Body.Instructions[0x58], Instruction.Create(OpCodes.Ldfld, this.cSharpASM.MainModule.Import(field)));
                iLProcessor.InsertAfter(definition3.Body.Instructions[0x59], Instruction.Create(OpCodes.Ldloc_0));
                iLProcessor.InsertAfter(definition3.Body.Instructions[90], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return false;
            }
            return true;
        }

        private void WrapMethod(MethodDefinition md, MethodDefinition origMethod, AssemblyDefinition asm)
        {
            Instruction instruction2;
            ILProcessor iLProcessor = md.Body.GetILProcessor();
            Instruction instruction = Instruction.Create(OpCodes.Ldarg_0);
            if (md.ReturnType.Name == "Void")
            {
                instruction2 = md.Body.Instructions[md.Body.Instructions.Count - 1];
            }
            else
            {
                instruction2 = md.Body.Instructions[md.Body.Instructions.Count - 2];
            }
            iLProcessor.InsertBefore(instruction2, instruction);
            for (int i = 0; i < md.Parameters.Count; i++)
            {
                iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Ldarga_S, md.Parameters[i]));
            }
            iLProcessor.InsertBefore(instruction2, Instruction.Create(OpCodes.Call, origMethod));
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
            TypeReference type = AssemblyDefinition.ReadAssembly("mscorlib.dll").MainModule.GetType("System.Exception");
            item.CatchType = asm.MainModule.Import(type);
            md.Body.ExceptionHandlers.Add(item);
        }
    }
}

