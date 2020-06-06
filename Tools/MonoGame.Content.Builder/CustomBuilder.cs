﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGame.Content.Builder
{
    class CustomBuilder
    {
        // after Release build: replace C:\Program Files (x86)\MSBuild\MonoGame\v3.0\Tools\MGCB.exe

        public CustomBuilder(string[] args)
        {
            string dir = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), "Content");

            string contentPathSource = Path.Combine(dir, "_Content.mgcb");

            if (!File.Exists(contentPathSource))
                return;

            string contentPathGen = Path.Combine(dir, "Content.mgcb");
            //Console.WriteLine(contentPathSource);
            //Console.WriteLine(contentPathGen);
            //Console.WriteLine("JAUUU^1");
            string content = File.ReadAllText(contentPathSource);

            //.Where(f => f.EndsWith(".ogg"))); || f.EndsWith(".wav"));

            using (StreamWriter sw = File.CreateText(contentPathGen))
            {
                sw.Write(content);

                var oggs = Directory.GetFiles(Path.Combine(dir, "sounds"), "*.ogg", SearchOption.AllDirectories);
                foreach (var file in oggs)
                {
                    sw.Write(string.Format(@"
#begin {0}
/importer:OggImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:{0}
", file.Substring(dir.Length + 1)));
                }

                var wavs = Directory.GetFiles(Path.Combine(dir, "sounds"), "*.wav", SearchOption.AllDirectories);
                foreach (var file in wavs)
                {
                    sw.Write(string.Format(@"
#begin {0}
/importer:WavImporter
/processor:SoundEffectProcessor
/processorParam:Quality=Best
/build:{0}
", file.Substring(dir.Length + 1)));
                }

                var textures = Directory.GetFiles(Path.Combine(dir, "textures"), "*.png", SearchOption.AllDirectories);
                foreach (var file in textures)
                {
                    sw.Write(string.Format(@"
#begin {0}
/importer:TextureImporter
/processor:TextureProcessor
/processorParam:ColorKeyColor=255,0,255,255
/processorParam:ColorKeyEnabled=True
/processorParam:GenerateMipmaps=False
/processorParam:PremultiplyAlpha=True
/processorParam:ResizeToPowerOfTwo=False
/processorParam:MakeSquare=False
/processorParam:TextureFormat=Color
/build:{0}
", file.Substring(dir.Length + 1)));
                }
            }
        }
    }
}
