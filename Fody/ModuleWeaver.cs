using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using System.Configuration;
using System.IO;
using FodySpring;
using Spring.Context.Support;
using System.Reflection;
using Spring.Objects.Factory;
using System.Collections.Specialized;

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
        LogInfo("enter execute");
        var configurableTypes = ModuleDefinition.Types.Where(t => t.CustomAttributes
                            .Any(c => c.AttributeType.Name == typeof(ConfigurableAttribute).Name));

        foreach(var type in configurableTypes)
        {
            var isConfiguredField = new FieldDefinition("<>__isConfigured", 
                                                        Mono.Cecil.FieldAttributes.Private, 
                                                        ModuleDefinition.TypeSystem.Boolean);
            type.Fields.Add(isConfiguredField);
            var ensureConfigurationMethod = GenerateEnsureConfigurationMethod(isConfiguredField);
            type.Methods.Add(ensureConfigurationMethod);

            foreach (var ctor in type.GetConstructors().Where(ctor => !ctor.IsStatic))
            {
                var baseCtorCall = ctor.Body.Instructions.Single(i => IsCallToCtor(i));
                var baseCtorCallIndex = ctor.Body.Instructions.IndexOf(baseCtorCall);

                ctor.Body.Instructions.Insert(baseCtorCallIndex + 1, Instruction.Create(OpCodes.Ldarg_0));
                ctor.Body.Instructions.Insert(baseCtorCallIndex + 2, Instruction.Create(OpCodes.Callvirt, ensureConfigurationMethod));
            }
        }
    }

    private MethodDefinition GenerateEnsureConfigurationMethod(FieldDefinition isConfiguredField)
    {
        var configureObjectMethod = ModuleDefinition.Import(typeof(ObjectConfigurator).GetMethod("ConfigureObject"));
        var exitMethod = Instruction.Create(OpCodes.Ret);
        var ensureConfigurationMethod = new MethodDefinition("<>__EnsureConfiguration",
                                                             Mono.Cecil.MethodAttributes.Private,
                                                             ModuleDefinition.TypeSystem.Void);
        ensureConfigurationMethod.Body.SimplifyMacros();

        IfNotIsConfigured(isConfiguredField, exitMethod, ensureConfigurationMethod);
        IfNotAvoidConfigurationSetting(exitMethod, ensureConfigurationMethod);
        ContextRegistryConfigureObject(ensureConfigurationMethod);
        SetIsConfiguredToTrue(isConfiguredField, exitMethod, ensureConfigurationMethod);

        ensureConfigurationMethod.Body.OptimizeMacros();
        return ensureConfigurationMethod;
    }

    private void IfNotIsConfigured(FieldDefinition isConfiguredField, Instruction exitMethod, MethodDefinition ensureConfigurationMethod)
    {
        //if(<>__isConfigured)
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, isConfiguredField));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Brtrue, exitMethod));
    }

    private void IfNotAvoidConfigurationSetting(Instruction exitMethod, MethodDefinition ensureConfigurationMethod)
    {
        //string avoidConfiguration = ConfigurationManager.AppSettings["Spring.Fody.AvoidConfiguration"];
        //if (!string.Equals(avoidConfiguration, "true", StringComparison.OrdinalIgnoreCase))
        ensureConfigurationMethod.Body.Variables.Add(new VariableDefinition("avoidConfiguration", ModuleDefinition.TypeSystem.String));
        ensureConfigurationMethod.Body.Variables.Add(new VariableDefinition("equalsTrue", ModuleDefinition.TypeSystem.Boolean));
        ensureConfigurationMethod.Body.InitLocals = true;
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call,
            ModuleDefinition.Import(typeof(ConfigurationManager).GetMethod("get_AppSettings", Type.EmptyTypes))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "Spring.Fody.AvoidConfiguration"));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
            ModuleDefinition.Import(typeof(NameValueCollection).GetMethod("get_Item", new[] { typeof(string) }))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldstr, "true"));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_5));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call,
            ModuleDefinition.Import(typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(string), typeof(StringComparison) }))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stloc_1));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldloc_1));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Brtrue, exitMethod));
    }

    private void ContextRegistryConfigureObject(MethodDefinition ensureConfigurationMethod)
    {
        // ContextRegistry.GetContext().ConfigureObject(objectToConfigure, objectToConfigure.GetType().Name);
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call,
            ModuleDefinition.Import(typeof(ContextRegistry).GetMethod("GetContext", Type.EmptyTypes))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
            ModuleDefinition.Import(typeof(object).GetMethod("GetType"))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
            ModuleDefinition.Import(typeof(MemberInfo).GetMethod("get_Name"))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
            ModuleDefinition.Import(typeof(IObjectFactory).GetMethod("ConfigureObject"))));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Pop));
    }

    private static void SetIsConfiguredToTrue(FieldDefinition isConfiguredField, Instruction exitMethod, MethodDefinition ensureConfigurationMethod)
    {
        //<>__isConfigured = true;
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4_1));
        ensureConfigurationMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, isConfiguredField));
        ensureConfigurationMethod.Body.Instructions.Add(exitMethod);
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