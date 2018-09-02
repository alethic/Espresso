using System.IO;

namespace Espresso
{

    /// <summary>
    /// A multi-output implicant set with each implicant consisting of a pair of row vectors. The input part contains
    /// integers in positional cube notation, and the output part contains entries describing the output type.
    /// </summary>
    public interface IEspressoCover
    {
        
        /// <summary>
        /// Number of cubes in the cover.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The input part of the multi-output implicant. This part contains integers in positional cube notation.
        /// </summary>
        IEspressoCoverData Inputs { get; }

        /// <summary>
        /// The output part of the multi-output implicant. This part contains entries in [0, 1, 2].
        /// </summary>
        IEspressoCoverData Output { get;}

        /// <summary>
        /// Returns the data of the cover as a set of multi-output implicants.
        /// </summary>
        /// <returns></returns>
        int[] ToTable();

    }

}
