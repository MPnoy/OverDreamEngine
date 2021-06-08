using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ODEngine.Helpers
{
    public static class MemoryAllocator
    {
        public struct Memory
        {
            public IntPtr data;
            public int length;

            public Memory(IntPtr data, int length)
            {
                this.data = data;
                this.length = length;
            }

            public Memory Clone()
            {
                var ptr = Marshal.AllocHGlobal(length);
                unsafe
                {
                    Buffer.MemoryCopy((void*)data, (void*)ptr, length, length);
                }
                var rawImage = new Memory()
                {
                    data = ptr,
                    length = length,
                };
                return rawImage;
            }

            public void Free()
            {
                Marshal.FreeHGlobal(data);
                data = IntPtr.Zero;
                length = 0;
            }

            public Task FreeAsync()
            {
                var data = this.data;
                return Task.Run(() =>
                {
                    Marshal.FreeHGlobal(data);
                });
            }
        }

        public static Memory Allocate(int length)
        {
            var ptr = Marshal.AllocHGlobal(length);
            var memory = new Memory()
            {
                data = ptr,
                length = length,
            };
            return memory;
        }

        public static Task<Memory> AllocateAsync(int length)
        {
            return Task.Run(() =>
            {
                var ptr = Marshal.AllocHGlobal(length);
                var memory = new Memory()
                {
                    data = ptr,
                    length = length,
                };
                return memory;
            });
        }

    }
}
