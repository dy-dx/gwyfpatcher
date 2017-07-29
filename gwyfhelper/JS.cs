using System;
using System.IO;
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
using FSWatcher;

namespace gwyfhelper
{
    public class JS
    {
        public static Engine engine;
        public static Watcher fsWatcher;
        public static string scriptDirectory = "/Users/chris/projects/gwyfpatcher/gwyfhelper/js";

        public static void Initialize()
        {
            if (engine != null)
            {
                return;
            }
            engine = new Engine(cfg => cfg.AllowClr(AppDomain.CurrentDomain.GetAssemblies().ToArray()));

            fsWatcher = new Watcher(
                scriptDirectory,
                (s) => { /* System.Console.WriteLine("Dir created " + s) */ },
                (s) => { /* System.Console.WriteLine("Dir deleted " + s) */ },
                OnFileCreated,
                OnFileChanged,
                (s) => { /* System.Console.WriteLine("File deleted " + s) */ }
            );

            fsWatcher.Watch();

            engine.Execute(@"
                var UnityEngine = importNamespace('UnityEngine');
                var gwyfhelper = importNamespace('gwyfhelper');
            ");

            // todo: load all files in the dir
            LoadScript(Path.Combine(scriptDirectory, "index.js"));
        }

        public static void OnFileChanged(string path)
        {
            System.Console.WriteLine("File changed " + path);
            LoadScript(path);
        }
        public static void OnFileCreated(string path)
        {
            System.Console.WriteLine("File created " + path);
            LoadScript(path);
        }

        public static void LoadScript(string path)
        {
            var source = File.ReadAllText(path);
            engine.Execute(source, new ParserOptions { Source = Path.GetFileName(path) });
        }

        public static void PreUpdate()
        {
            engine.Execute(@"index && index();");
        }
    }
}
