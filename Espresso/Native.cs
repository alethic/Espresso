using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Espresso
{

    /// <summary>
    /// Provides access to the native methods of Espresso.
    /// </summary>
    static class Native
    {

        /// <summary>
        /// Initializes the static type.
        /// </summary>
        static Native()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X86:
                        if (LoadLibrary(@"native\win-x86\EspressoLib.dll") == IntPtr.Zero)
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        break;
                    case Architecture.X64:
                        if (LoadLibrary(@"native\win-x64\EspressoLib.dll") == IntPtr.Zero)
                            throw new Win32Exception(Marshal.GetLastWin32Error());
                        break;
                    default:
                        throw new NotSupportedException("Unknown OS architecture.");
                }
            }
            else
            {
                throw new NotSupportedException("Unknown OS platform.");
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("EspressoLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int main(int argc, string[] argv);

    }

}
