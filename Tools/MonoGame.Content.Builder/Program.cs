// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;

namespace MonoGame.Content.Builder
{
    class Program
    {
        static readonly bool testSE = false;

        static int Main(string[] args)
        {
            if (testSE)
            {
                bool demo = false;

                Environment.CurrentDirectory = "C:\\Projects\\SE\\SE\\";

                string additionalArgs = $"/define:Debug /define:Demo={(demo ? "true" : "false")} /define:Configuration=Debug /define:RuntimeIdentifier= /define:ConfigRuntime=Debug_ /@:C:\\Projects\\SE\\SE\\Content\\Content.Generated.mgcb";

                args = [
                    //"/rebuildOnVersionUpdate:true",
                    "/parallelCores:0.5",
                    "/platform:DesktopGL",
                    @"/outputDir:C:/Projects/SE/SE/Content/bin/DesktopGL/Content.Generated",
                    @"/intermediateDir:C:/Projects/SE/SE/Content/obj/DesktopGL/net8.0/Content.Generated",
                    @"/workingDir:C:/Projects/SE/SE/Content/",
                ];

                args = additionalArgs.Split([' ']).Concat(args).ToArray();
            }

            // We force all stderr to redirect to stdout
            // to avoid any out of order console output.
            Console.SetError(Console.Out);

            if (!Environment.Is64BitProcess && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                Console.Error.WriteLine("The MonoGame content tools only work on a 64bit OS.");
                return -1;
            }

            var content = new BuildContent();

            // Parse the command line.
            var parser = new MGBuildParser(content)
            {
                Title = "MonoGame Content Builder\n" +
                        "Builds optimized game content for MonoGame projects."
            };

            if (!parser.Parse(args))
                return -1;           
            
            // Launch debugger if requested.
            if (content.LaunchDebugger)
            {
                try {
                    System.Diagnostics.Debugger.Launch();
                } catch (NotImplementedException) {
                    // not implemented under Mono
                    Console.Error.WriteLine("The debugger is not implemented under Mono and thus is not supported on your platform.");
                }
            }

            // Print a startup message.            
            var buildStarted = DateTime.Now;
            if (!content.Quiet)
                Console.WriteLine("Build started {0}\n", buildStarted);

            // Let the content build.
            int successCount, errorCount;
            content.Build(out successCount, out errorCount);

            // Print the finishing info.
            if (!content.Quiet)
            {
                Console.WriteLine("\nBuild {0} succeeded, {1} failed.\n", successCount, errorCount);
                Console.WriteLine("Time elapsed {0:hh\\:mm\\:ss\\.ff}.", DateTime.Now - buildStarted);
            }

            // Return the error count.
            return errorCount;
        }
    }
}
