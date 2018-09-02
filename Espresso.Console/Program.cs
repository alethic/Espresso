namespace Espresso.Console
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var cover = EspressoNet.CreateCover(1, 2, 1);
            cover.Inputs[0, 0] = 2;
            cover.Inputs[0, 1] = 2;
            cover.Output[0, 0] = 1;
            cover = EspressoNet.Espresso(cover);

            System.Console.ReadLine();
            System.GC.Collect();
            System.Console.ReadLine();
        }

    }

}
