namespace Espresso
{

    /// <summary>
    /// Provides access to a cube data set within an Espresso cover.
    /// </summary>
    public interface IEspressoCoverData
    {

        /// <summary>
        /// Number of items in the data set.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the data for the given cube at the specified position.
        /// </summary>
        /// <param name="cube"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        int this[int cube, int position] { get; set; }

    }

}
