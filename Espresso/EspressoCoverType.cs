using System;

namespace Espresso
{

    /// <summary>
    /// A flag field that indicates the type of the input cover.
    /// </summary>
    [Flags]
    public enum EspressoCoverType : int
    {

        None = 0,
        F_TYPE = 1,
        D_TYPE = 2,
        R_TYPE = 4,

    }

}