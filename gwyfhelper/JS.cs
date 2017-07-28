using System;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Error;
using Jint.Native.Object;
using Jint.Parser;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Microsoft.Scripting.Ast;

namespace gwyfhelper
{
    public class JS
    {
        public static Engine engine;

        public static void Initialize()
        {
            if (engine != null)
            {
                return;
            }
            //https://github.com/OxideMod/Oxide/blob/d472c69e0e231ded3d4da1fde55c256c797b19cb/Extensions/Oxide.Core.JavaScript/JavaScriptExtension.cs
			engine = new Engine(cfg => cfg.AllowClr(AppDomain.CurrentDomain.GetAssemblies().ToArray()));
            engine.Global.FastSetProperty("importNamespace", new PropertyDescriptor(new ClrFunctionInstance(engine, (thisObj, arguments) =>
            {
                var nspace = TypeConverter.ToString(arguments.At(0));
                return new NamespaceReference(engine, nspace);
            }), false, false, false));

            engine.SetValue("Log", new Action<object>(UnityEngine.Debug.Log));
        }

        public static void Log(string msg)
        {
            engine.Execute("Log('" + msg + "');");
        }
    }
}
