using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnpatch;

namespace gwyfpatcher
{
    class MainClass
    {
        public static string GetModulePath()
        {
            string basePath = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(basePath, @"../Assembly-CSharp.dll");
            return fullPath;
        }

        public static string GetOutputModulePath()
        {
            string basePath = Directory.GetCurrentDirectory();
            string fullPath = Path.Combine(basePath, @"../Assembly-CSharp.patched.dll");
            return fullPath;
        }

        public static MethodDef AddMethod(ModuleDefMD mod, string className, string methodName)
        {
            TypeDef klass = mod.Find(className, false);

            MethodDef method = new MethodDefUser(
                methodName,
                MethodSig.CreateInstance(mod.CorLibTypes.Void),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
            );

            klass.Methods.Add(method);
            method.Body = new CilBody();
            return method;
        }

        public static MethodDef PrependMethod(ModuleDefMD mod, string className, string methodName, MethodDef newMethod)
        {
            TypeDef klass = mod.Find(className, false);
            MethodDef method = klass.FindMethod(methodName);

            if (method == null) {
                Console.WriteLine("fuk");
            }

            method.Body.Instructions.Insert(0, OpCodes.Ldarg_0.ToInstruction());
            method.Body.Instructions.Insert(1, OpCodes.Call.ToInstruction(newMethod));

            return method;
        }

        public static CilBody PreStartBody(ModuleDefMD mod)
        {
            CilBody body = new CilBody();
            var i = body.Instructions;

            TypeDef ScriptScopeTypeDef = mod.Find("Microsoft.Scripting.Hosting.ScriptScope", false);
            TypeDef ScriptSourceTypeDef = mod.Find("Microsoft.Scripting.Hosting.ScriptSource", false);
            TypeDef ScriptEngineTypeDef = mod.Find("Microsoft.Scripting.Hosting.ScriptEngine", false);
            TypeDef PythonTypeDef = mod.Find("IronPython.Hosting.Python", false);

            AssemblyRef UnityEngineRef = mod.GetAssemblyRef("UnityEngine");
            TypeRefUser DebugTypeRef = new TypeRefUser(mod, "UnityEngine", "Debug", UnityEngineRef);

            TypeSig ScriptEngineTypeSig = ScriptEngineTypeDef.ToTypeSig();
            TypeSig ScriptScopeTypeSig = ScriptScopeTypeDef.ToTypeSig();
            TypeSig ScriptSourceTypeSig = ScriptSourceTypeDef.ToTypeSig();

            MemberRef CreateEngineRef = new MemberRefUser(mod, "CreateEngine",
                MethodSig.CreateStatic(ScriptEngineTypeSig), PythonTypeDef);

            MemberRef CreateScopeRef = new MemberRefUser(mod, "CreateScope",
                MethodSig.CreateInstance(ScriptScopeTypeSig), ScriptEngineTypeDef);

            MemberRef CreateScriptSourceFromStringRef = new MemberRefUser(mod, "CreateScriptSourceFromString",
                MethodSig.CreateInstance(ScriptSourceTypeSig, mod.CorLibTypes.String), ScriptEngineTypeDef);

            MemberRef ExecuteRef = new MemberRefUser(mod, "Execute",
                MethodSig.CreateInstance(mod.CorLibTypes.Object, ScriptScopeTypeSig), ScriptSourceTypeDef);

            // MemberRef GetVariableRef = new MemberRefUser(mod, "GetVariable",
            //     MethodSig.CreateInstance(mod.CorLibTypes.String, mod.CorLibTypes.String), ScriptScopeTypeDef);

            MemberRef GetVariableRef = new MemberRefUser(mod, "GetVariable",
                MethodSig.CreateInstanceGeneric(1, mod.CorLibTypes.String, mod.CorLibTypes.String), ScriptScopeTypeDef);

            // MethodDef GetVariableDef = PythonTypeDef.FindMethod("GetVariable",
            //     MethodSig.CreateInstance(mod.CorLibTypes.String, mod.CorLibTypes.String));

            MemberRef LogRef = new MemberRefUser(mod, "LogError",
                MethodSig.CreateStatic(mod.CorLibTypes.Void, mod.CorLibTypes.String), DebugTypeRef);


            /*
                ScriptEngine scriptEngine = Python.CreateEngine();
                ScriptScope ScriptScope = scriptEngine.CreateScope();
            */
            Local local0 = new Local(ScriptScopeTypeSig);
            body.Variables.Add(local0);

            i.Add(OpCodes.Call.ToInstruction(CreateEngineRef));
            i.Add(OpCodes.Dup.ToInstruction());
            i.Add(OpCodes.Callvirt.ToInstruction(CreateScopeRef));
            i.Add(OpCodes.Stloc.ToInstruction(local0));

            /*
                string example = "output = 'hello world'";
            */
            Local local1 = new Local(mod.CorLibTypes.String);
            body.Variables.Add(local1);

            i.Add(OpCodes.Ldstr.ToInstruction("output = 'hello world'"));
            i.Add(OpCodes.Stloc.ToInstruction(local1));
            i.Add(OpCodes.Ldloc.ToInstruction(local1));

            /*
                scriptEngine.CreateScriptSourceFromString(example).Execute(ScriptScope);
            */
            i.Add(OpCodes.Callvirt.ToInstruction(CreateScriptSourceFromStringRef));
            i.Add(OpCodes.Ldloc.ToInstruction(local0));
            i.Add(OpCodes.Callvirt.ToInstruction(ExecuteRef));
            i.Add(OpCodes.Pop.ToInstruction());

            /*
                UnityEngine.Debug.Log(ScriptScope.GetVariable<string>("output"));
            */
            // i.Add(OpCodes.Ldloc.ToInstruction(local0));
            // i.Add(OpCodes.Ldstr.ToInstruction("output"));
            // i.Add(OpCodes.Callvirt.ToInstruction(GetVariableRef));
            // i.Add(OpCodes.Call.ToInstruction(LogRef));

            /*
                UnityEngine.Debug.Log("hello");
            */
            i.Add(OpCodes.Ldstr.ToInstruction("hello"));
            i.Add(OpCodes.Call.ToInstruction(LogRef));

            i.Add(OpCodes.Ret.ToInstruction());
            return body;
        }

        public static void Main(string[] args)
        {
            ModuleDefMD mod = ModuleDefMD.Load(GetModulePath());

            MethodDef PreStart = AddMethod(mod, "BallMovement", "PreStart");
            PreStart.Body = PreStartBody(mod);

            MethodDef preUpdate = AddMethod(mod, "BallMovement", "PreUpdate");

            preUpdate.Body.Instructions.Add(OpCodes.Ret.ToInstruction());

            PrependMethod(mod, "BallMovement", "Start", PreStart);
            PrependMethod(mod, "BallMovement", "Update", preUpdate);

            mod.Write(GetOutputModulePath());

            Console.WriteLine("success");
        }
    }
}
