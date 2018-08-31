namespace Espresso.Console
{

    public static class Program
    {

        public static int Main(string[] args)
        {
            return EspressoLib.Native.main(args.Length, args);
        }

    }

}
