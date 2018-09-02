using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Espresso
{

    /// <summary>
    /// Provides access to the native methods of Espresso.
    /// </summary>
    static unsafe class Native
    {

        /// <summary>
        /// Native definition of an Espresso cover.
        /// </summary>
        public struct net_cover_t
        {

            public int ncubes;
            public int ninputs;
            public int noutput;
            public int* data;

        }

        /// <summary>
        /// Initializes the static type.
        /// </summary>
        static Native()
        {
            // just try to load
            // failure will cause PInvoke to fail with better message
            LoadLibLibrary();
        }

        /// <summary>
        /// Attempts to load the native library from various paths.
        /// </summary>
        /// <returns></returns>
        static bool LoadLibLibrary()
        {
            foreach (var path in GetLibPaths())
                if (File.Exists(path))
                    if (LoadLibrary(path) != IntPtr.Zero)
                        return true;

            return false;
        }

        /// <summary>
        /// Gets some library paths the assembly might be located in.
        /// </summary>
        /// <returns></returns>
        static IEnumerable<string> GetLibPaths()
        {
            var self = Directory.GetParent(typeof(Native).Assembly.Location)?.FullName;
            if (self == null)
                yield break;

            switch (Marshal.SizeOf<IntPtr>())
            {
                case 4:
                    yield return Path.Combine(self, @"runtimes\win7-x86\native\EspressoLib.dll");
                    yield return Path.Combine(self, @"runtimes\win-x86\native\EspressoLib.dll");
                    yield return Path.Combine(self, @"x86\EspressoLib.dll");
                    break;
                case 8:
                    yield return Path.Combine(self, @"runtimes\win7-x64\native\EspressoLib.dll");
                    yield return Path.Combine(self, @"runtimes\win-x64\native\EspressoLib.dll");
                    yield return Path.Combine(self, @"x64\EspressoLib.dll");
                    break;
                default:
                    throw new NotSupportedException("Unknown OS architecture.");
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("EspressoLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern net_cover_t espressonet(net_cover_t cover, int intype);

        [DllImport("EspressoLib.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void espressonet_free(IntPtr data);

    }

}
