using System.IO;

namespace Espresso.Console
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            var pla = PLA.Parse(new StreamReader(File.OpenRead(@"C:\dev\Espresso\tlex\alu4.pla")));
            pla.WriteTo(System.Console.Out);
            System.Console.WriteLine("Cubes: {0}", pla.Cover.Count);
            System.Console.WriteLine("press enter to process...");
            System.Console.ReadLine();

            var rst = EspressoNet.Espresso(pla.Cover, pla.CoverType != EspressoCoverType.None ? pla.CoverType : EspressoCoverType.F_TYPE | EspressoCoverType.D_TYPE);
            pla = new PLA(rst);
            pla.WriteTo(System.Console.Out);
            System.Console.WriteLine("Cubes: {0}", pla.Cover.Count);
            System.Console.ReadLine();
        }

    }

}
