using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtractCameraXP
{
    internal class ScanBytes
    {
        private const byte SyncByte = 0x47;
        private const byte TpSize = 188;
        private const byte PidMask = 0x1F;
        private const int  PidsSubtract = 65;
        private static Dictionary<int, FileStream> _fsStreams;
        private static Dictionary<int, bool> _mapPids;

        private static void WriteBytePacket(ref byte[] byteArray, int offset, int id)
        {
            try
            {
                if (byteArray.Length < (offset + 188)) return;
                var buffer = new byte[188];
                Array.Copy(byteArray, offset, buffer, 0, 188);
                _fsStreams[id].Write(buffer, 0, 188);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void WritePatPacket(ref byte[] byteArray, int offset)
        {
            try
            {
                if (byteArray.Length < (offset + 188)) return;
                var buffer = new byte[188];
                Array.Copy(byteArray, offset, buffer, 0, 188);

                foreach (var item in _mapPids.Where(item => item.Value))
                {
                    Pat.RewritePat(ref buffer, (byte)(item.Key - PidsSubtract));
                    _fsStreams[item.Key].Write(buffer, 0, 188);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void Extract(ref byte[] byteArray, int offset)
        {
            var croppedByte = (byte) (byteArray[offset + 1] & PidMask);
            var id = (ushort) (croppedByte * 256 + byteArray[offset + 2]);
            if (id == 0) WritePatPacket(ref byteArray, offset);
            else if (_fsStreams.ContainsKey(id)) WriteBytePacket(ref byteArray, offset, id);
            else if (_fsStreams.ContainsKey(id + PidsSubtract)) WriteBytePacket(ref byteArray, offset, id + PidsSubtract);
        }

        private static void RunBuffer(int count, ref byte[] byteArray)
        {
            if (count < 188) return;

            for (var i = 0; i < byteArray.Length; i++)
            {
                if (byteArray[i] != SyncByte && byteArray[i+1] != 0x40) continue;
                Extract(ref byteArray, i);
                i += (TpSize - 1);
            }
        }

        private static void CreateAllFsStreams(string fileName)
        {
            foreach (var item in _mapPids)
            {
                var outFileName = fileName + "_X" + (item.Key).ToString("X") + ".ts";
                _fsStreams[item.Key] = new FileStream("Camera X" + (item.Key).ToString("X") + ".ts", FileMode.Create);
                Console.WriteLine("Created {0} ", outFileName);
            }
        }
        private static void CreateFsStreams(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            _fsStreams = new Dictionary<int, FileStream>();

            foreach (var item in _mapPids.Where(item => item.Value))
            {
                var outFileName = fileName + "_X" + (item.Key).ToString("X") + ".ts";
                _fsStreams[item.Key] = new FileStream(outFileName, FileMode.Create);
                Console.WriteLine("Created {0} ", outFileName);
            }

            if (_fsStreams.Count == 0) CreateAllFsStreams(fileName);
        }

        private static void CloseFsStreams()
        {
            foreach (var fsStream in _fsStreams)
            {
                fsStream.Value?.Close();
            }
        }

        private static void ReadFile(string path)
        {
            using (var fsSource = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                fsSource.Seek(0, SeekOrigin.Begin);
                var byteArray = new byte[fsSource.Length];
                var bytesRead = fsSource.Read(byteArray, 0, byteArray.Length);
                RunBuffer(bytesRead, ref byteArray);
            }
        }
        public static void SearchSyncByte(string path, ref Dictionary<int, bool> mapPids)
        {
            _mapPids = mapPids;
            CreateFsStreams(path);
            ReadFile(path);
            CloseFsStreams();
        }
    }
}
