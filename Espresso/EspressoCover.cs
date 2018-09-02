using System;

namespace Espresso
{

    /// <summary>
    /// Describes a managed Espresso cover instance.
    /// </summary>
    class EspressoCover :
        IEspressoCover
    {

        /// <summary>
        /// Provides a <see cref="IEspressoCoverData"/> implementation.
        /// </summary>
        struct EspressoCoverData : IEspressoCoverData
        {

            readonly int count;
            readonly Action<int, int, int> set;
            readonly Func<int, int, int> get;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="count"></param>
            /// <param name="get"></param>
            /// <param name="set"></param>
            public EspressoCoverData(
                int count,
                Func<int, int, int> get,
                Action<int, int, int> set)
            {
                this.count = count;
                this.get = get;
                this.set = set;
            }

            /// <summary>
            /// Gets the number of items in the data.
            /// </summary>
            public int Count => count;

            /// <summary>
            /// Gets or sets the value of the datafor the specified cube and position.
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
        readonly int[] data;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="ncubes"></param>
        /// <param name="ninputs"></param>
        /// <param name="noutput"></param>
        public EspressoCover(int ncubes, int ninputs, int noutput)
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
            this.data = new int[(ninputs + noutput) * ncubes];
        }

        /// <summary>
        /// Gets the length of a single cube in the table.
        /// </summary>
        int CubeLength => ninputs + noutput;

        /// <summary>
        /// Gets the number of cubes in the table.
        /// </summary>
        public int Count => ncubes;

        /// <summary>
        /// Gets an interface to manipulate the input data.
        /// </summary>
        public IEspressoCoverData Inputs => new EspressoCoverData(ninputs, GetInput, SetInput);

        /// <summary>
        /// Gets an interface to manipulate the output data.
        /// </summary>
        public IEspressoCoverData Output => new EspressoCoverData(noutput, GetOutput, SetOutput);

        /// <summary>
        /// Returns the underlying data table.
        /// </summary>
        /// <returns></returns>
        public int[] ToTable()
        {
            return data;
        }

        int GetInput(int cube, int position)
        {
            if (cube < 0 || cube >= ncubes)
                throw new ArgumentNullException(nameof(cube));
            if (position < 0 || position >= ninputs)
                throw new ArgumentNullException(nameof(position));

            return data[CubeLength * cube + position];
        }

        void SetInput(int cube, int position, int value)
        {
            if (cube < 0 || cube >= ncubes)
                throw new ArgumentNullException(nameof(cube));
            if (position < 0 || position >= ninputs)
                throw new ArgumentNullException(nameof(position));

            data[CubeLength * cube + position] = value;
        }

        int GetOutput(int cube, int position)
        {
            if (cube < 0 || cube >= ncubes)
                throw new ArgumentNullException(nameof(cube));
            if (position < 0 || position >= noutput)
                throw new ArgumentNullException(nameof(position));

            return data[CubeLength * cube + ninputs + position];
        }

        void SetOutput(int cube, int position, int value)
        {
            if (cube < 0 || cube >= ncubes)
                throw new ArgumentNullException(nameof(cube));
            if (position < 0 || position >= noutput)
                throw new ArgumentNullException(nameof(position));

            data[CubeLength * cube + ninputs + position] = value;
        }

    }

}
