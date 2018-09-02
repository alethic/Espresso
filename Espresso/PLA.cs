using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Espresso
{

    /// <summary>
    /// Partial implementation of the Berkeley PLA format.
    /// </summary>
    public class PLA
    {

        static readonly Regex COMMENT = new Regex(@"^#.*$", RegexOptions.Compiled);
        static readonly Regex NINPUTS = new Regex(@"^\.i\s+(\d+)$", RegexOptions.Compiled);
        static readonly Regex NOUTPUT = new Regex(@"^\.o\s+(\d+)$", RegexOptions.Compiled);
        static readonly Regex PROD = new Regex(@"^\.p\s+(\d+)$", RegexOptions.Compiled);
        static readonly Regex ILB = new Regex(@"^\.ilb\s+(\w+(?:\s+\w+)*)$", RegexOptions.Compiled);
        static readonly Regex OB = new Regex(@"^\.ob\s+(\w+(?:\s+\w+)*)$", RegexOptions.Compiled);
        static readonly Regex TYPE = new Regex(@"^\.type\s+(f|r|fd|fr|dr|fdr)$", RegexOptions.Compiled);
        static readonly Regex CUBE = new Regex(@"^([01-]+)\s+([01-]+)$", RegexOptions.Compiled);
        static readonly Regex END = new Regex(@"^\.e(?:nd)?$", RegexOptions.Compiled);

        static readonly Dictionary<string, EspressoCoverType> TYPES = new Dictionary<string, EspressoCoverType>()
        {
            ["f"] = EspressoCoverType.F_TYPE,
            ["r"] = EspressoCoverType.R_TYPE,
            ["fd"] = EspressoCoverType.F_TYPE | EspressoCoverType.D_TYPE,
            ["fr"] = EspressoCoverType.F_TYPE | EspressoCoverType.R_TYPE,
            ["dr"] = EspressoCoverType.D_TYPE | EspressoCoverType.R_TYPE,
            ["fdr"] = EspressoCoverType.F_TYPE | EspressoCoverType.D_TYPE | EspressoCoverType.R_TYPE,
        };
        static readonly Dictionary<EspressoCoverType, string> TYPESREV = TYPES.ToDictionary(i => i.Value, i => i.Key);

        static readonly Dictionary<char, int> INPUTSCODE = new Dictionary<char, int>() { ['0'] = 1, ['1'] = 2, ['-'] = 3 };
        static readonly Dictionary<int, char> INPUTSCODEREV = INPUTSCODE.ToDictionary(i => i.Value, i => i.Key);

        static readonly Dictionary<char, int> OUTPUTCODE = new Dictionary<char, int>() { ['0'] = 0, ['1'] = 1, ['-'] = 2 };
        static readonly Dictionary<int, char> OUTPUTCODEREV = OUTPUTCODE.ToDictionary(i => i.Value, i => i.Key);

        /// <summary>
        /// Parses the given PLA-format stream.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static PLA Parse(TextReader reader)
        {
            var pla_inputs = 0;
            var pla_output = 0;
            List<string> pla_inputsLabels = null;
            List<string> pla_outputLabels = null;
            var pla_type = EspressoCoverType.None;
            List<(List<int>, List<int>)> cubes = null;

            while (reader.ReadLine()?.Trim() is string line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (COMMENT.IsMatch(line))
                    continue;

                // .i
                if (NINPUTS.Match(line) is Match nins && nins.Success)
                    if (pla_inputs == 0)
                    {
                        pla_inputs = int.Parse(nins.Groups[1].Value);
                        continue;
                    }
                    else
                        throw new EspressoException(".i declared more than once.");

                // .o
                if (NOUTPUT.Match(line) is Match nout && nout.Success)
                    if (pla_output == 0)
                    {
                        pla_output = int.Parse(nout.Groups[1].Value);
                        continue;
                    }
                    else
                        throw new EspressoException(".o declared more than once.");

                // ignore .p
                if (PROD.IsMatch(line))
                    continue;

                // .ilb
                if (ILB.Match(line) is Match ilb && ilb.Success)
                    if (pla_inputsLabels is null)
                    {
                        pla_inputsLabels = new List<string>(ilb.Groups[1].Value.Split(' '));
                        continue;
                    }
                    else
                        throw new EspressoException(".ilb declared more than once.");

                // .ob
                if (OB.Match(line) is Match ob && ob.Success)
                    if (pla_outputLabels is null)
                    {
                        pla_outputLabels = new List<string>(ob.Groups[1].Value.Split(' '));
                        continue;
                    }
                    else
                        throw new EspressoException(".ob declared more than once.");

                // .type
                if (TYPE.Match(line) is Match type && type.Success)
                    if (pla_type != EspressoCoverType.None)
                    {
                        pla_type = TYPES[type.Groups[0].Value];
                        continue;
                    }
                    else
                        throw new EspressoException(".type declared more than once.");

                if (CUBE.Match(line) is Match cube && cube.Success)
                {
                    if (cubes == null)
                    {
                        if (pla_inputs <= 0)
                            throw new EspressoException(".i not found before implicant table.");
                        if (pla_output <= 0)
                            throw new EspressoException(".o not found before implicant table.");

                        cubes = new List<(List<int>, List<int>)>(32);
                    }

                    // parse cube line
                    var inputs = cube.Groups[1].Value;
                    var output = cube.Groups[2].Value;
                    var inputsVec = inputs.Select(i => INPUTSCODE[i]).Take(pla_inputs).ToList();
                    var outputVec = output.Select(i => OUTPUTCODE[i]).Take(pla_output).ToList();

                    // add new cube to temporary table
                    cubes.Add((inputsVec, outputVec));

                    continue;
                }

                // ignore .e
                if (END.IsMatch(line))
                    continue;

                throw new EspressoException($"Syntax error on line '{line}'.");
            }

            var cover = EspressoNet.CreateCover(cubes.Count, pla_inputs, pla_output);

            // configure cover based on parsed information
            for (var i = 0; i < cubes.Count; i++)
            {
                for (var j = 0; j < pla_inputs; j++)
                    cover.Inputs[i, j] = cubes[i].Item1[j];
                for (var j = 0; j < pla_output; j++)
                    cover.Output[i, j] = cubes[i].Item2[j];
            }

            return new PLA(cover, pla_type, pla_inputsLabels, pla_outputLabels);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="cover"></param>
        /// <param name="coverType"></param>
        /// <param name="inputs"></param>
        /// <param name="output"></param>
        public PLA(IEspressoCover cover, EspressoCoverType coverType, IEnumerable<string> inputs, IEnumerable<string> output) :
            this(cover, coverType)
        {
            InputsLabels = inputs?.ToList() ?? new List<string>();
            OutputLabels = output?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="cover"></param>
        public PLA(IEspressoCover cover, EspressoCoverType coverType = EspressoCoverType.None)
        {
            Cover = cover ?? throw new System.ArgumentNullException(nameof(cover));
            CoverType = coverType;
        }

        /// <summary>
        /// Implicant table.
        /// </summary>
        public IEspressoCover Cover { get; set; }

        /// <summary>
        /// Type of the cover.
        /// </summary>
        public EspressoCoverType CoverType { get; set; }

        /// <summary>
        /// Input variable names.
        /// </summary>
        public List<string> InputsLabels { get; }

        /// <summary>
        /// Output variable names.
        /// </summary>
        public List<string> OutputLabels { get; }

        /// <summary>
        /// Writes the PLA file out to the given text stream.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(TextWriter writer)
        {
            writer.WriteLine(".i {0}", Cover.Inputs.Count);
            writer.WriteLine(".o {0}", Cover.Output.Count);

            if (CoverType != EspressoCoverType.None)
                writer.WriteLine(".type {0}", TYPESREV[CoverType]);

            for (var i = 0; i < Cover.Count; i++)
            {
                for (var j = 0; j < Cover.Inputs.Count; j++)
                    writer.Write(INPUTSCODEREV[Cover.Inputs[i, j]]);

                writer.Write(" ");

                for (var j = 0; j < Cover.Output.Count; j++)
                    writer.Write(OUTPUTCODEREV[Cover.Output[i, j]]);

                writer.WriteLine();
            }

            writer.WriteLine(".e");
        }

    }

}
