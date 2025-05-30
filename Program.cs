using System.Diagnostics;
using System.Configuration;
using System.Collections.Specialized;

namespace PicoProjects
{
    internal static class Program
    {
        const string DefaultDrive = "E";

        const string bl = "BuildList.txt";

        const string cml_base = "cml_base.txt";
        const string cml = "CMakeLists.txt";
        const string cml_command = "add_subdirectory(@)";


        const string cmlP_base = "cmlP_base.txt";
        const string cmlP = "@\\CMakeLists.txt";

        const string sep = "@";

        const string CMD = "CMD.exe";

        const string changeDir = "cd @";

        const string buildDir = "build";
        const string cmake_build = "cmake ..";

        const string make = "make";

        static readonly string[] nonProjectsInBuild = new string[5] { ".cmake", "CMakeFiles", "elf2uf2", "generated", "pico-sdk" };


        public static bool Confirmaton(string Text)
        {
            Console.WriteLine(Text + " (y/n)");
            return Console.ReadKey(true).Key == ConsoleKey.Y;
        }

        public static void errorAexit(string Text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Text); Console.ResetColor();
            Environment.Exit(0);
        }

        public static string Get(string f) 
        {
            return File.ReadAllText(f);
        }
        public static void WriteNew(string file, string _base)
        {
            File.WriteAllText(file, _base);
        }

        private static void AddWrite(string file, string text)
        {
            List<string> f = File.ReadAllLines(file).ToList<string>();
            if (!f.Contains(text))
                f.Add(text);
            File.WriteAllLines(file, f);
        }
        private static string Rp(this string? item, string replaceWith)
        {
            item = item.Replace(sep, replaceWith);
            return item;
        }
        //
        //
        //
        private static void Help()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--Help Widnow--");
            Console.WriteLine("new - Create new project ready to code and complie. (0 args)");
            Console.WriteLine("build - builds CMake build directory. (0 args)");
            Console.WriteLine("buildlist - Manually adds an entry to the buildlist. (1 args - Name)");
            Console.WriteLine("clearCmlk - Clears the CMAkeList subdirectories. (0args)");
            Console.WriteLine("clearBuild - Clears the buildlist file. (0 args)");
            Console.WriteLine("make - Compiles the specified project. (1 args - Name)");
            Console.WriteLine("exec - Exectutes the specified project. (2 args - Name, (optional)PicoDrive)");
            Console.ResetColor();
        }
        //
        //
        //
        private static void Execute(string[] args)
        {
            string target;
            string Drive;
            if (args.Length == 2)
            {
                target = args[1];
                Drive = DefaultDrive;
                if (!Directory.Exists(Drive + ":\\"))
                {
                    Console.WriteLine($"The DefaultDrive '{Drive}' does not exists");
                    Console.Write("Pico Drive: "); Drive = Console.ReadKey(true).Key.ToString().ToUpper();
                    Console.WriteLine(Drive);
                }
            }
            else if (args.Length == 3)
            {
                target = args[1];
                Drive = args[2];
            }
            else
            {
                errorAexit("No file, nor drive letter specified!");
                target = "";
                Drive = "";
            }

            if (!Directory.Exists(Drive + ":\\"))
            {
                errorAexit($"The Drive '{Drive}' does not exists");
            }

            File.Copy($"{buildDir}\\{target}\\{target}.uf2 ", $"{Drive}:\\{target}.uf2", true);
        }
        private static void Make(string[] args)
        {
            string target = "";
            if (args.Length == 2)
                target = args[1];
            else
                errorAexit("No file specified!");

            if (!Directory.Exists(buildDir + "\\" + target)) errorAexit($"Project '{target}' does not exits!");
            Process.Start(CMD, changeDir.Rp(buildDir) + "\\" + target);
            Process.Start(CMD, make);
        }
        private static void ExecuteBuild()
        {
            Process.Start(CMD, changeDir.Rp(buildDir)); // Change to Build (1/2)
            Process.Start(CMD, cmake_build);            // Run cmake (2/2)
        }

        private static void clearCml()
        {
            WriteNew(cml, Get(cml_base));
        }
        private static void clearBuild()
        {
            if (!Confirmaton("Are you sure you want to clear the build list?")) errorAexit("Confirmation Declined!");
            File.Create(bl).Close();
        }

        private static void build(string[] args)
        {
            string[] buildList;
            bool all = false;
            if (args[0] == "all")
            {
                buildList = File.ReadAllLines(bl);
                all = true;
            }
            else
            {
                buildList = args;
            }

            if (buildList.Length == 0) errorAexit("BuildList is empty.");

            Console.WriteLine("Added to Next Build: \n\t" + String.Join("\n\t", (all ? buildList.Skip(1) : buildList)));
            if (!Confirmaton("Write CMake file?")) errorAexit("Confirmation Declined!");

            WriteNew(cml, Get(cml_base));
            foreach (var b in (all?buildList.Skip(1): buildList))
            {
                //Console.WriteLine(cml_command.Rp(b));
                AddWrite(cml, cml_command.Rp(b));
            }
            // prepped for execution
        }
        private static void ManualBuild(string[] args)
        {
            if (args.Length == 2)
            {
                AddWrite(bl, args[1]);
            }
            else new ArgumentException("No specified name");
            throw new NotImplementedException("ManualBuild not implemented.");
        }

        private static void newProject()
        {
            Console.WriteLine("Project name?");
            Console.Write("& >"); string? pName = Console.ReadLine();
            if (pName == null || pName == "") throw new ArgumentNullException("Project name can't be null");

            AddWrite(bl, pName); // buildList (1/2)

            Directory.CreateDirectory(pName); // dir

            File.Create(pName + "\\" + pName + ".c").Close(); // script file

            string new_cmlP = cmlP.Rp(pName); // CmakeP file
            File.Create(new_cmlP).Close();    // CmakeP file created
            WriteNew(new_cmlP, Get(cmlP_base).Rp(pName)); // CmakeP file done (2/2)
        }


        static void Main(string[] args)
        {
            //Directory.SetCurrentDirectory(Directory.GetCurrentDirectory() + "\\testFolder");

            if (args.Length != 0)
            {
                if (args[0] == "new") newProject();
                else if (args[0] == "build") ExecuteBuild();
                else if (args[0] == "buildlist") ManualBuild(args);
                else if (args[0] == "clearCml") clearCml();
                else if (args[0] == "clearBuild") clearBuild();
                else if (args[0] == "exec") Execute(args);
                else if (args[0] == "make") Make(args);

                else build(args);
            }
            else
            {
                Help();
            }

            if (!Debugger.IsAttached)
            {
                // Skip WaitKey
                goto End;
            }
            #region END
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n\nProgram Exited!"); Console.ResetColor();
            Console.ReadKey();
        #endregion
        End:;
        }
    }
}