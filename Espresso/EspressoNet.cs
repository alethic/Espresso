using System;

namespace Espresso
{

    /// <summary>
    /// Main library entry point.
    /// </summary>
    public static class EspressoNet
    {

        static readonly object sync = new object();

        /// <summary>
        /// Creates a new cover instance.
        /// </summary>
        /// <param name="cubes"></param>
        /// <param name="inputs"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static IEspressoCover CreateCover(int cubes, int inputs, int output)
        {
            return new EspressoCover(cubes, inputs, output);
        }

        /// <summary>
        /// Invokes the native Espresso method within a lock.
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        static unsafe Native.net_cover_t espressonet(Native.net_cover_t cover, int intype)
        {
            lock (sync)
                return Native.espressonet(cover, intype);
        }

        /// <summary>
        /// Return a logically equivalent, (near) minimal cost set of product-terms to represent the ON-set and
        /// optionally minterms that lie in the DC-set, without containing any minterms of the OFF-set.
        /// </summary>
        /// <param name="cover"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static unsafe IEspressoCover Espresso(
            IEspressoCover cover,
            EspressoCoverType type = EspressoCoverType.F_TYPE | EspressoCoverType.D_TYPE)
        {
            if (cover == null)
                throw new ArgumentNullException(nameof(cover));
            if (cover.Count < 1)
                throw new ArgumentException(nameof(cover));
            if (cover.Inputs.Count < 1)
                throw new ArgumentException(nameof(cover));
            if (cover.Output.Count < 1)
                throw new ArgumentException(nameof(cover));

            // default value for none type
            if (type == EspressoCoverType.None)
                type = EspressoCoverType.F_TYPE | EspressoCoverType.D_TYPE;

            if (!type.HasFlag(EspressoCoverType.F_TYPE) &&
                !type.HasFlag(EspressoCoverType.R_TYPE))
                throw new ArgumentOutOfRangeException(nameof(type), "Expected type in [F, R, FD, FR, DR, FDR].");

            // obtain cover data and pin
            fixed (int* data = cover.ToTable())
            {
                // invoke native method
                var ret = espressonet(new Native.net_cover_t()
                {
                    ncubes = cover.Count,
                    ninputs = cover.Inputs.Count,
                    noutput = cover.Output.Count,
                    data = data,
                }, (int)type);
                if (ret.data == null)
                    throw new EspressoException();

                // return managed wrapper
                return new EspressoLibCover(ret.ncubes, ret.ninputs, ret.noutput, ret.data);
            }
        }

    }

}
