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
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
            );

            klass.Methods.Add(method);
            return method;
        }

        public static MethodDef PrependMethod(ModuleDefMD mod, string className, string methodName, MethodDef newMethod)
        {
            TypeDef klass = mod.Find(className, false);
            MethodDef method = klass.FindMethod(methodName);

            if (method == null) {
                Console.WriteLine("fuk");
            }

            //method.Body.Instructions.Insert(0, OpCodes.Ldarg_0.ToInstruction());
            method.Body.Instructions.Insert(0, OpCodes.Call.ToInstruction(newMethod));

            return method;
        }

        public static void Main(string[] args)
        {
            ModuleDefMD mod = ModuleDefMD.Load(GetModulePath());
            MethodDef preUpdate = AddMethod(mod, "BallMovement", "PreUpdate");
            PrependMethod(mod, "BallMovement", "Update", preUpdate);

            mod.Write(GetOutputModulePath());

            Console.WriteLine("success");
        }
    }
}
