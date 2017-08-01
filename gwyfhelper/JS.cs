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
        public static string scriptDirectory;

        public static void Initialize()
        {
            if (engine != null)
            {
                return;
            }
            engine = new Engine(cfg => cfg.AllowClr(AppDomain.CurrentDomain.GetAssemblies().ToArray()));

            scriptDirectory = Path.Combine(Util.GetCurrentDLLDirectory(), "js");
            if (!Directory.Exists(scriptDirectory)) {
                scriptDirectory = "/Users/chris/projects/gwyfpatcher/gwyfhelper/js";
            }

            var noop = new Action<string>((s) => {});
            fsWatcher = new Watcher(
                scriptDirectory,
                noop, // dir created
                noop, // dir deleted
                OnFileChangedOrCreated, // file created
                OnFileChangedOrCreated, // file changed
                noop // file deleted
            );

            fsWatcher.Watch();

            // todo: load all files in the dir
            LoadScript(Path.Combine(scriptDirectory, "index.js"));
        }

        public static void OnFileChangedOrCreated(string path)
        {
            if (Path.GetExtension(path) != ".js")
            {
                return;
            }
            System.Console.WriteLine("File changed " + path);
            LoadScript(path, true);
        }

        public static bool LoadScript(string path, bool callOnScriptReload = false)
        {
            if (Path.GetExtension(path) != ".js")
            {
                return false;
            }
            if (!File.Exists(path))
            {
                // todo: throw
                return false;
            }
            var source = File.ReadAllText(path);
            var parserOptions = new ParserOptions { Source = Path.GetFileName(path) };

            Util.HideAlert();
            bool success = Execute(source, parserOptions);
            if (success && callOnScriptReload)
            {
                success = Execute(@"
                    if (typeof global !== 'undefined') {
                        global.OnScriptReload && global.OnScriptReload();
                        global.OnScriptReload = null;
                    }
                ", parserOptions);
            }
            return success;
        }

        public static bool Execute(string source, ParserOptions options = null)
        {
            try
            {
                engine.Execute(source, options);
            }
            catch (ParserException e)
            {
                Console.WriteLine(String.Join(
                    Environment.NewLine,
                    $"File: {e.Source} Line: {e.LineNumber} Column: {e.Column}",
                    e.Description,
                    e.StackTrace
                ));
                Util.Alert(e.Description);
                return false;
            }
            catch (JavaScriptException e)
            {
                var obj = e.Error.ToObject() as ErrorInstance;
                var description = (obj == null)
                    ? e.Message
                    : $"{obj.Get("name").AsString()} {obj.Get("message").AsString()}";
                Console.WriteLine(String.Join(
                    Environment.NewLine,
                    $"File: {e.Location.Source} Line: {e.LineNumber} Column: {e.Column}",
                    description,
                    e.StackTrace
                ));
                Util.Alert(description);
                return false;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Util.Alert(e.GetType().Name);
                return false;
            }
            return true;
        }

        // called during every BallMovement::PreUpdate
        public static void Update()
        {
            Execute(@"
                if (typeof global !== 'undefined') {
                    global.index && global.index();
                }
            ");
        }
    }
}
