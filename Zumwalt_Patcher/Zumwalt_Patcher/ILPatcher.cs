namespace Zumwalt_Patcher
{
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using System;

    public class ILPatcher
    {
        private AssemblyDefinition cSharpASM;
        private AssemblyDefinition firstPassASM;
        private TypeDefinition HooksClass;
        private AssemblyDefinition ZumwaltAsm;

        public ILPatcher()
        {
            try
            {
                this.ZumwaltAsm = AssemblyDefinition.ReadAssembly("Zumwalt.dll");
                this.HooksClass = this.ZumwaltAsm.MainModule.GetType("Zumwalt.Hooks");
            }
            catch (Exception exception)
            {
                Logger.Log(exception.ToString());
            }
        }

        private bool AccessPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("Character");
            MethodDefinition definition2 = null;
            foreach (MethodDefinition definition3 in type.Methods)
            {
                if (definition3.Name == "OnDestroy")
                {
                    definition2 = definition3;
                }
            }
            if (definition2 == null)
            {
                return false;
            }
            try
            {
                definition2.Attributes = (MethodAttributes) ((ushort) (((int) definition2.Attributes) - 1));
                definition2.IsPublic = true;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool AntiDecay()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("EnvDecay");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("StructureMaster");
            MethodDefinition definition3 = null;
            MethodDefinition definition4 = null;
            MethodDefinition method = null;
            foreach (MethodDefinition definition6 in type.Methods)
            {
                if (definition6.Name == "Awake")
                {
                    definition3 = definition6;
                }
            }
            foreach (MethodDefinition definition7 in definition2.Methods)
            {
                if (definition7.Name == "DoDecay")
                {
                    definition4 = definition7;
                }
            }
            foreach (MethodDefinition definition8 in this.HooksClass.Methods)
            {
                if (definition8.Name == "DecayDisabled")
                {
                    method = definition8;
                }
            }
            if (((definition4 == null) || (definition3 == null)) || (method == null))
            {
                return false;
            }
            try
            {
                ILProcessor iLProcessor = definition3.Body.GetILProcessor();
                iLProcessor.InsertBefore(definition3.Body.Instructions[0], Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertAfter(definition3.Body.Instructions[0], Instruction.Create(OpCodes.Brtrue, definition3.Body.Instructions[definition3.Body.Instructions.Count - 1]));
                iLProcessor = definition4.Body.GetILProcessor();
                iLProcessor.InsertBefore(definition4.Body.Instructions[0], Instruction.Create(OpCodes.Callvirt, this.cSharpASM.MainModule.Import(method)));
                iLProcessor.InsertAfter(definition4.Body.Instructions[0], Instruction.Create(OpCodes.Brtrue, definition4.Body.Instructions[definition4.Body.Instructions.Count - 1]));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool BootstrapAttachPatch()
        {
            TypeDefinition type = this.ZumwaltAsm.MainModule.GetType("Zumwalt.Bootstrap");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("ServerInit");
            MethodDefinition method = null;
            MethodDefinition definition4 = null;
            foreach (MethodDefinition definition5 in type.Methods)
            {
                if (definition5.Name == "AttachBootstrap")
                {
                    method = definition5;
                }
            }
            foreach (MethodDefinition definition6 in definition2.Methods)
            {
                if (definition6.Name == "Awake")
                {
                    definition4 = definition6;
                }
            }
            if ((method == null) || (definition4 == null))
            {
                return false;
            }
            try
            {
                definition4.Body.GetILProcessor().InsertAfter(definition4.Body.Instructions[0x74], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method)));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool NPCHurtPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("BasicWildLifeAI");
            MethodDefinition orig = null;
            MethodDefinition method = null;
            foreach (MethodDefinition definition4 in type.Methods)
            {
                if (definition4.Name == "OnHurt")
                {
                    orig = definition4;
                }
            }
            foreach (MethodDefinition definition5 in this.HooksClass.Methods)
            {
                if (definition5.Name == "NPCHurt")
                {
                    method = definition5;
                }
            }
            if ((orig == null) || (method == null))
            {
                return false;
            }
            try
            {
                this.CloneMethod(orig);
                ILProcessor iLProcessor = orig.Body.GetILProcessor();
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarga_S));
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Call, this.cSharpASM.MainModule.Import(method));
                iLProcessor.InsertBefore(orig.Body.Instructions[0], Instruction.Create(OpCodes.Ldarg_0));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool ChatPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("chat");
            MethodDefinition orig = null;
            MethodDefinition method = null;
            foreach (MethodDefinition definition4 in type.Methods)
            {
                if (definition4.Name == "say")
                {
                    orig = definition4;
                }
            }
            foreach (MethodDefinition definition5 in this.HooksClass.Methods)
            {
                if (definition5.Name == "ChatReceived")
                {
                    method = definition5;
                }
            }
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
            catch (Exception)
            {
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
            MethodDefinition orig = null;
            MethodDefinition method = null;
            foreach (MethodDefinition definition4 in type.Methods)
            {
                if (definition4.Name == "RunCommand")
                {
                    orig = definition4;
                }
            }
            foreach (MethodDefinition definition5 in this.HooksClass.Methods)
            {
                if (definition5.Name == "ConsoleReceived")
                {
                    method = definition5;
                }
            }
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
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool DoorSharing()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("DeployableObject");
            MethodDefinition definition2 = null;
            MethodDefinition method = null;
            foreach (MethodDefinition definition4 in type.Methods)
            {
                if (definition4.Name == "BelongsTo")
                {
                    definition2 = definition4;
                }
            }
            foreach (MethodDefinition definition5 in this.HooksClass.Methods)
            {
                if (definition5.Name == "CheckOwner")
                {
                    method = definition5;
                }
            }
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
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool EntityHurt()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("StructureComponent");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("DeployableObject");
            MethodDefinition definition3 = null;
            MethodDefinition definition4 = null;
            MethodDefinition method = null;
            foreach (MethodDefinition definition6 in type.Methods)
            {
                if (definition6.Name == "OnHurt")
                {
                    definition3 = definition6;
                }
            }
            foreach (MethodDefinition definition7 in definition2.Methods)
            {
                if (definition7.Name == "OnHurt")
                {
                    definition4 = definition7;
                }
            }
            foreach (MethodDefinition definition8 in this.HooksClass.Methods)
            {
                if (definition8.Name == "EntityHurt")
                {
                    method = definition8;
                }
            }
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
                definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
                definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Call, reference));
                definition4.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool ItemsTablesLoadedPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("DatablockDictionary");
            MethodDefinition orig = null;
            MethodDefinition method = null;
            MethodDefinition definition4 = null;
            foreach (MethodDefinition definition5 in type.Methods)
            {
                if (definition5.Name == "Initialize")
                {
                    orig = definition5;
                }
            }
            foreach (MethodDefinition definition6 in this.HooksClass.Methods)
            {
                if (definition6.Name == "ItemsLoaded")
                {
                    method = definition6;
                }
            }
            foreach (MethodDefinition definition7 in this.HooksClass.Methods)
            {
                if (definition7.Name == "TablesLoaded")
                {
                    definition4 = definition7;
                }
            }
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
            catch (Exception exception)
            {
                Logger.Log(exception.ToString());
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
                this.firstPassASM = AssemblyDefinition.ReadAssembly("Assembly-CSharp-firstpass.dll");
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
                if (!this.EntityHurt())
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
                catch (Exception exception)
                {
                    flag = false;
                    Logger.Log(exception.ToString());
                }
                return flag;
            }
            catch (Exception exception2)
            {
                Logger.Log(exception2.ToString());
                return false;
            }
        }

        private bool PlayerHurtPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("HumanBodyTakeDamage");
            MethodDefinition orig = null;
            MethodDefinition method = null;
            foreach (MethodDefinition definition4 in type.Methods)
            {
                if (definition4.Name == "Hurt")
                {
                    orig = definition4;
                }
            }
            foreach (MethodDefinition definition5 in this.HooksClass.Methods)
            {
                if (definition5.Name == "PlayerHurt")
                {
                    method = definition5;
                }
            }
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
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool PlayerJoinLeavePatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("RustServerManagement");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("ConnectionAcceptor");
            MethodDefinition orig = null;
            MethodDefinition method = null;
            MethodDefinition definition5 = null;
            MethodDefinition definition6 = null;
            foreach (MethodDefinition definition7 in type.Methods)
            {
                if (definition7.Name == "OnUserConnected")
                {
                    orig = definition7;
                }
            }
            foreach (MethodDefinition definition8 in this.HooksClass.Methods)
            {
                if (definition8.Name == "PlayerConnect")
                {
                    method = definition8;
                }
            }
            foreach (MethodDefinition definition9 in definition2.Methods)
            {
                if (definition9.Name == "uLink_OnPlayerDisconnected")
                {
                    definition5 = definition9;
                }
            }
            foreach (MethodDefinition definition10 in this.HooksClass.Methods)
            {
                if (definition10.Name == "PlayerDisconnect")
                {
                    definition6 = definition10;
                }
            }
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
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool PlayerKilledPatch()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("HumanController");
            MethodDefinition orig = null;
            MethodDefinition method = null;
            foreach (MethodDefinition definition4 in type.Methods)
            {
                if (definition4.Name == "OnKilled")
                {
                    orig = definition4;
                }
            }
            foreach (MethodDefinition definition5 in this.HooksClass.Methods)
            {
                if (definition5.Name == "PlayerKilled")
                {
                    method = definition5;
                }
            }
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
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool TalkerNotifications()
        {
            TypeDefinition type = this.cSharpASM.MainModule.GetType("VoiceCom");
            TypeDefinition definition2 = this.cSharpASM.MainModule.GetType("PlayerClient");
            MethodDefinition definition3 = null;
            MethodDefinition method = null;
            FieldDefinition field = null;
            VariableDefinition variable = null;
            foreach (MethodDefinition definition7 in type.Methods)
            {
                if (definition7.Name == "clientspeak")
                {
                    definition3 = definition7;
                }
            }
            foreach (MethodDefinition definition8 in this.HooksClass.Methods)
            {
                if (definition8.Name == "ShowTalker")
                {
                    method = definition8;
                }
            }
            foreach (FieldDefinition definition9 in definition2.Fields)
            {
                if (definition9.Name == "netPlayer")
                {
                    field = definition9;
                }
            }
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
            catch (Exception exception)
            {
                Logger.Log(exception.ToString());
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

