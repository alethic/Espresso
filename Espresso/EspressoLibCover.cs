using System;
using System.Runtime.InteropServices;

namespace Espresso
{

    /// <summary>
    /// <see cref="IEspressoCover"/> implementation mapping unmanaged memory. Frees the unmanaged memory on disposable.
    /// </summary>
    unsafe class EspressoLibCover :
        IEspressoCover,
        IDisposable
    {

        struct EspressoCoverData :
            IEspressoCoverData
        {

            readonly EspressoLibCover cover;
            readonly int count;
            readonly Func<int, int, int> get;
            readonly Action<int, int, int> set;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="cover"></param>
            /// <param name="get"></param>
            /// <param name="set"></param>
            public EspressoCoverData(
                EspressoLibCover cover,
                int count,
                Func<int, int, int> get,
                Action<int, int, int> set)
            {
                this.cover = cover ?? throw new ArgumentNullException(nameof(cover));
                this.count = count;
                this.get = get ?? throw new ArgumentNullException(nameof(get));
                this.set = set ?? throw new ArgumentNullException(nameof(set));
            }

            /// <summary>
            /// Gets the number of items in this data table.
            /// </summary>
            public int Count => count;

            /// <summary>
            /// Gets or sets the value at the given index of the data table.
            /// </summary>
            /// <param name="cube"></param>
            /// <param name="position"></param>
            /// <returns></returns>
            public int this[int cube, int position]
            {
                get => get(cube, position);
                set => set(cube, position, value);
            }

        }

        readonly int ncubes;
        readonly int ninputs;
        readonly int noutput;
        readonly int* data;
        readonly EspressoCoverData inputs;
        readonly EspressoCoverData output;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ncubes"></param>
        /// <param name="ninputs"></param>
        /// <param name="noutput"></param>
        /// <param name="data"></param>
        public EspressoLibCover(int ncubes, int ninputs, int noutput, int* data)
        {
            if (ncubes < 1)
                throw new ArgumentOutOfRangeException(nameof(ncubes));
            if (ninputs < 1)
                throw new ArgumentOutOfRangeException(nameof(ninputs));
            if (noutput < 1)
                throw new ArgumentOutOfRangeException(nameof(noutput));

            this.ncubes = ncubes;
            this.ninputs = ninputs;
            this.noutput = noutput;
            this.data = data;
            this.inputs = new EspressoCoverData(this, ninputs, GetInput, SetInput);
            this.output = new EspressoCoverData(this, noutput, GetOutput, SetOutput);
        }

        /// <summary>
        /// Disposes of the instance
        /// </summary>
        ~EspressoLibCover()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public unsafe void Dispose()
        {
            if (data != null)
                Native.espressonet_free((IntPtr)data);

            GC.SuppressFinalize(this);
        }

        int RowLength => ninputs + noutput;

        public int Count => ncubes;

        public IEspressoCoverData Inputs => inputs;

        public IEspressoCoverData Output => output;

        int GetInput(int cube, int position)
        {
            if (cube < 0 || cube >= ncubes)
                throw new ArgumentNullException(nameof(cube));
            if (position < 0 || position >= ninputs)
                throw new ArgumentNullException(nameof(position));

            return data[RowLength * cube + position];
        }

        void SetInput(int cube, int position, int value)
        {
            if (cube < 0 || cube >= ncubes)
                throw new ArgumentNullException(nameof(cube));
            if (position < 0 || position >= ninputs)
                throw new ArgumentNullException(nameof(position));

            data[RowLength * cube + position] = value;
        }

        int GetOutput(int cube, int position)
        {
            if (cube < 0 || cube >= ncubes)
                throw new ArgumentNullException(nameof(cube));
            if (position < 0 || position >= noutput)
                throw new ArgumentNullException(nameof(position));

            return data[RowLength * cube + ninputs + position];
        }

        void SetOutput(int cube, int position, int value)
        {
            if (cube < 0 || cube >= ncubes)
                throw new ArgumentNullException(nameof(cube));
            if (position < 0 || position >= noutput)
                throw new ArgumentNullException(nameof(position));

            data[RowLength * cube + ninputs + position] = value;
        }

        public int[] ToTable()
        {
            // copy underlying data into new managed array
            var ret = new int[(ninputs + noutput) * ncubes];
            Marshal.Copy((IntPtr)data, ret, 0, ret.Length);
            return ret;
        }

    }

}
