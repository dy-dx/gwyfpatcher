using System;
using System.IO;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace gwyfpatcher
{
    class MainClass
    {
        public static string ModuleFileName = "Assembly-CSharp.dll";
        public static string HelperModuleFileName = "gwyfhelper.dll";
        public static string OutputModuleFileName = "Assembly-CSharp.patched.dll";
        public static string ModuleBackupFileName = "Assembly-CSharp.backup.dll";

        public static ModuleDefMD GetModule(string fileName)
        {
            string currentPath = GetCurrentExecutableDirectory();
            // possible locations
            var paths = new List<string>() {
                currentPath,
                Path.Combine(currentPath, @"../../../deps/Managed/"),
                Path.Combine(currentPath, @"../../../gwyfhelper/bin/Debug/")
            };
            foreach (var p in paths)
            {
                var fullPath = Path.Combine(p, fileName);
                if (File.Exists(fullPath))
                {
                    return ModuleDefMD.Load(fullPath);
                }
            }

            Console.WriteLine("Could not find " + fileName + " in:");
            foreach (var p in paths)
            {
                Console.WriteLine(p);
            }
            throw new Exception("You broke it!");
        }

        public static string GetOutputModulePath()
        {
            string basePath = GetCurrentExecutableDirectory();
            return Path.Combine(basePath, OutputModuleFileName);
        }

        public static string GetCurrentExecutableDirectory()
        {
            return Path.GetDirectoryName(
                System.Reflection.Assembly.GetEntryAssembly().Location
            );
        }

        // adds a public void method that calls a method of the same name on the GoFast class
        public static MethodDef AddMethod(ModuleDefMD mod, TypeDef klass, TypeDef helperClass, string methodName)
        {
            MethodDef method = new MethodDefUser(
                methodName,
                MethodSig.CreateInstance(mod.CorLibTypes.Void),
                MethodImplAttributes.IL | MethodImplAttributes.Managed,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot
            );

            klass.Methods.Add(method);
            method.Body = new CilBody();
            var i = method.Body.Instructions;
            i.Add(CallHelperMethodInstruction(mod, helperClass, methodName));
            i.Add(OpCodes.Ret.ToInstruction());
            return method;
        }

        public static Instruction CallHelperMethodInstruction(ModuleDefMD mod, TypeDef helperClass, string methodName)
        {
            MemberRef methodRef = new MemberRefUser(
                mod,
                methodName,
                MethodSig.CreateStatic(mod.CorLibTypes.Void),
                helperClass
            );

            IMethod importedMethodRef = mod.Import(methodRef);
            return OpCodes.Call.ToInstruction(importedMethodRef);
        }

        public static void PrependMethodCall(MethodDef methodToIntercept, MethodDef methodToCall)
        {
            methodToIntercept.Body.Instructions.Insert(0, OpCodes.Ldarg_0.ToInstruction());
            methodToIntercept.Body.Instructions.Insert(1, OpCodes.Call.ToInstruction(methodToCall));
        }

        public static void Main(string[] args)
        {
            ModuleDefMD Mod = GetModule(ModuleFileName);
            TypeDef BallMovement = Mod.Find("BallMovement", false);

            ModuleDefMD HelperMod = GetModule(HelperModuleFileName);
            TypeDef GoFast = HelperMod.Find("gwyfhelper.GoFast", false);

            if (BallMovement.FindMethod("PreUpdate") != null)
            {
                Console.WriteLine("Your " + ModuleFileName + " is already patched.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                return;
            }

            foreach (var name in new List<string>() {"OnGUI", "OnDestroy", "LateUpdate"})
            {
                AddMethod(Mod, BallMovement, GoFast, name);
            }

            // e.g. create the PreUpdate method on BallMovement, then call this.PreUpdate() at the
            // beginning of the Update method.
            foreach (var name in new List<string>() {"Start", "Update", "FixedUpdate"})
            {
                var methodToIntercept = BallMovement.FindMethod(name);
                var methodToCall = AddMethod(Mod, BallMovement, GoFast, "Pre" + name);
                PrependMethodCall(methodToIntercept, methodToCall);
            }

            string outputModulePath = GetOutputModulePath();
            Console.WriteLine("Writing file " + outputModulePath);
            Mod.Write(outputModulePath);

            // If this executable is running from the actual game dir,
            // copy Assembly-CSharp.dll to Assembly-CSharp.backup.dll
            // and copy Assembly-CSharp.patched.dll to Assembly-CSharp.dll
            string basePath = GetCurrentExecutableDirectory();
            string originalModulePath = Path.Combine(basePath, ModuleFileName);
            if (File.Exists(originalModulePath))
            {
                // Release handles so we can overwrite it
                Mod.Context.AssemblyResolver.Clear();
                Mod.MetaData.PEImage.UnsafeDisableMemoryMappedIO();
                Mod = null;

                string backupModulePath = Path.Combine(basePath, ModuleBackupFileName);
                Console.WriteLine("Copying " + ModuleFileName + " to " + ModuleBackupFileName);
                File.Copy(originalModulePath, backupModulePath, true);
                Console.WriteLine("Overwriting " + ModuleFileName + " with " + OutputModuleFileName);
                File.Copy(outputModulePath, originalModulePath, true);
                File.Delete(outputModulePath);
                Console.WriteLine("Success! Press any key to continue.");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Success!");
            }
        }
    }
}
