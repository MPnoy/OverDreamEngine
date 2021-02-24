
using System.IO;

namespace ODEngine.Core.Audio
{
    public class WavFile
    {
        public uint riffId;
        public uint size;
        public uint wavId;
        public uint fmtId;
        public uint fmtSize;
        public ushort format;
        public ushort channelCount;
        public uint sampleRate;
        public uint bytePerSec;
        public ushort blockSize;
        public ushort bitsPerSample;
        public uint dataId;
        public uint dataSize;
        public byte[] data;

        public WavFile(string fileName)
        {
            //Читаем данные
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    riffId = br.ReadUInt32();
                    size = br.ReadUInt32();
                    wavId = br.ReadUInt32();
                    fmtId = br.ReadUInt32();
                    fmtSize = br.ReadUInt32();
                    format = br.ReadUInt16();
                    channelCount = br.ReadUInt16();
                    sampleRate = br.ReadUInt32();
                    bytePerSec = br.ReadUInt32();
                    blockSize = br.ReadUInt16();
                    bitsPerSample = br.ReadUInt16();
                    if (fmtSize > 16)
                    {
                        br.ReadBytes((int)fmtSize - 16);
                    }
                    dataId = br.ReadUInt32();
                    dataSize = br.ReadUInt32();
                    data = br.ReadBytes((int)dataSize);
                }
            }
        }

    }
}
