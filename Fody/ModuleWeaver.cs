using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using FluentIL.Cecil;
using System.Configuration;
using System.IO;
using FodySpring;

public class ModuleWeaver
{
    public Action<string> LogInfo { get; set; }

    // An instance of Mono.Cecil.ModuleDefinition for processing
    public ModuleDefinition ModuleDefinition { get; set; }

    public IAssemblyResolver AssemblyResolver { get; set; }

    // Init logging delegates to make testing easier
    public ModuleWeaver()
    {
        LogInfo = m => { };
    }

    public void Execute()
    {
        var configurableTypes = ModuleDefinition.Types.Where(t => t.CustomAttributes
                            .Any(c => c.AttributeType.Name == typeof(ConfigurableAttribute).Name));

        foreach(var type in configurableTypes)
        {
            var isConfiguredField = new FieldDefinition("<>__isConfigured", FieldAttributes.Private, ModuleDefinition.TypeSystem.Boolean);
            type.Fields.Add(isConfiguredField);

            var configureObjectMethod = ModuleDefinition.Import(typeof(ObjectConfigurator).GetMethod("ConfigureObject"));
            var ensureConfigurationMethod = new MethodDefinition("<>__EnsureConfiguration", 
                                                                 MethodAttributes.Private, 
                                                                 ModuleDefinition.TypeSystem.Void);

            var exitMethod = Instruction.Create(OpCodes.Ret);
            ensureConfigurationMethod.Body.SimplifyMacros();
            ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, isConfiguredField));
            ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Brtrue, exitMethod));
            ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, configureObjectMethod));
            ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
            ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
            ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, isConfiguredField));
            ensureConfigurationMethod.Body.Instructions.Add(exitMethod);
            ensureConfigurationMethod.Body.OptimizeMacros();
            type.Methods.Add(ensureConfigurationMethod);

            foreach (var ctor in type.GetConstructors())
            {
                var baseCtorCall = ctor.Body.Instructions.Single(i => IsCallToCtor(i));
                var baseCtorCallIndex = ctor.Body.Instructions.IndexOf(baseCtorCall);

                ctor.Body.Instructions.Insert(baseCtorCallIndex + 1, Instruction.Create(OpCodes.Ldarg_0));
                ctor.Body.Instructions.Insert(baseCtorCallIndex + 2, Instruction.Create(OpCodes.Callvirt, ensureConfigurationMethod));
            }
        }
    }

    private bool IsCallToCtor(Instruction instruction)
    {
        const string ctor = ".ctor";

        if (instruction.OpCode == OpCodes.Call)
        {
            var methodReference = instruction.Operand as MethodReference;
            if (methodReference != null)
            {
                return methodReference.Name == ctor;
            }

            var methodDefinition = instruction.Operand as MethodDefinition;
            if (methodDefinition != null)
            {
                return methodDefinition.Name == ctor;
            }
        }

        return false;
    }
}