using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtractCameraXP
{
    class Program
    {
        private static Dictionary<int, bool> _mapPids;
        private static void CreateMap()
        {
            _mapPids = new Dictionary<int, bool>()
            {
                {0x85, false },
                {0x86, false },
                {0x87, false },
                {0x88, false },
                {0x89, false }
            };
        }

        private static void CheckArg(string arg)
        {
            switch (arg)
            {
                case "-1":
                    _mapPids[0x85] = true; 
                    break;
                case "-2":
                    _mapPids[0x86] = true;
                    break;
                case "-3":
                    _mapPids[0x87] = true;
                    break;
                case "-4":
                    _mapPids[0x88] = true;
                    break;
                case "-5":
                    _mapPids[0x89] = true;
                    break;
            }
        }

        private static void CheckArgs(string[] args)
        {
            foreach (var item in args) CheckArg(item);
        }

        private static void WriteInstructions()
        {
            Console.Write("Use the command: \n");
            Console.Write("     ExtractCameraXP \"Key\" \"FileName\" \n");
            Console.Write("Used keys: \n");
            Console.Write("     \"empty string\" - extract all pids; \n");
            Console.Write("     -1 - extract video stream at pid X85; \n");
            Console.Write("     -2 - extract video stream at pid X86; \n");
            Console.Write("     -3 - extract video stream at pid X87; \n");
            Console.Write("     -4 - extract video stream at pid X88; \n");
            Console.Write("     -5 - extract video stream at pid X89; \n");
        }

        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                WriteInstructions();
                return;
            }
            
            CreateMap();
            CheckArgs(args);
            try
            {
                var path = args.Last();
                if (path != null) ScanBytes.SearchSyncByte(path, ref _mapPids);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}
