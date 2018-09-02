using System;

namespace Espresso
{

    public class EspressoException :
        Exception
    {

        public EspressoException()
        {

        }

        public EspressoException(string message) :
            base(message)
        {

        }

    }

}