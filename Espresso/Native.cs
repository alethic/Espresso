using System;
using System.Runtime.InteropServices;

namespace EspressoLib
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
                        LoadLibrary(@"native\win-x86\EspressoLib.dll");
                        break;
                    case Architecture.X64:
                        LoadLibrary(@"native\win-x64\EspressoLib.dll");
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

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("EspressoLib.dll")]
        public static extern int main(int argc, string[] argv);

    }

}
