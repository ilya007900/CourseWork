using System;
using System.Runtime.InteropServices;

namespace AppDomain.Entities
{
    public class SmartPointer : IDisposable
    {
        public static readonly SmartPointer Zero = new SmartPointer(0);

        public int Size { get; }

        public IntPtr Ptr { get; }

        public SmartPointer(int size)
        {
            Size = size;
            Ptr = Marshal.AllocHGlobal(size);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(Ptr);
        }
    }
}