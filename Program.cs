using System.Text;

namespace NoitaEyes
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using FileStream stream = new FileStream(".\\out.txt", FileMode.Create);
            using StreamWriter writer = new StreamWriter(stream);
            Console.SetOut(writer);
            foreach (var eyemap in EyeMap.CreateVariants())
            {
                //foreach (var eyeMessage in EyeMessages.Combined)
                //{
                //}
                var eyeMessage = EyeMessages.East1;
                StringBuilder sb = new StringBuilder("'");
                StringBuilder sb2 = new StringBuilder();
                StringBuilder sb3 = new StringBuilder();
                StringBuilder sb4 = new StringBuilder();

                char[] linear = Trigram(eyemap, eyeMessage.Chars).Select(e => Convert.ToChar(e + 32)).ToArray();

                sb4.Append('"').Append(linear).Append('"');

                var temp = GetDoubleLines(eyeMessage.Chars);
                foreach (var doubleLine in temp)
                {
                    foreach (var method in GetTrigramMappings())
                    {
                        var temp2 = MapDoubleLines(eyemap, method, doubleLine);
                        foreach (var num in temp2)
                        {
                            sb2.Append(num.ToString("D3") + " ");
                            sb.Append(Convert.ToChar(num + 32));
                        }
                        char[] chars = temp2.Select(e => Convert.ToChar(e + 32)).ToArray();
                        sb3.Append(ToBraille(chars));
                    }
                    //sb.AppendLine();
                    //sb2.AppendLine();
                    //sb3.AppendLine();
                    //Console.WriteLine(string.Join(", ", temp2));
                }
                sb.Append("'");
                Console.WriteLine(sb.ToString());
                Console.WriteLine(sb4.ToString());
                Console.WriteLine();
                Console.WriteLine(sb2.ToString());
                Console.WriteLine();
                Console.WriteLine(sb3.ToString());
                Console.WriteLine();

            }

            Console.WriteLine("Fin.");
        }

        private static IEnumerable<char[][]> GetDoubleLines(char[] input)
        {
            List<char> output1 = new List<char>();
            List<char> output2 = new List<char>();
            int nCount = 0;
            foreach (char c in input)
            {
                if (c == 'n')
                {
                    nCount++;
                    if (nCount == 2)
                    {
                        char[][] output = [[.. output1], [.. output2]];
                        yield return output;
                        output1.Clear();
                        output2.Clear();
                        nCount = 0;
                    }
                }
                else
                {
                    if (nCount == 0)
                        output1.Add(c);
                    else
                        output2.Add(c);
                }
            }
        }
        const string brailleMap = " A1B'K2L@CIF/MSP\"E3H9O6R^DJG>NTQ,*5<-U8V.%[$+X!&;:4\\0Z7(_?W]#Y)=";
        private static char[] ToBraille(char[] input)
        {
            char[] output = new char[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = (char)(brailleMap.IndexOf(char.ToUpper(input[i])) + 0x2800);
            }
            return output;
        }

        private static int[] MapDoubleLines(EyeMap map, char[][] input)
        {
            List<int> output = new List<int>();

            for (int i = 0; i < input[0].Length / 3; i += 3)
            {
                output.Add(Base5(map, input[0][i], input[0][i + 1], input[1][i]));
                output.Add(Base5(map, input[1][i + 2], input[1][i + 1], input[0][i + 2]));
                //output.Add(Base5(map, input[1][i + 1], input[0][i + 2], input[1][i + 2]));
            }
            return output.ToArray();
        }
        private static int[] MapDoubleLines(EyeMap map, Func<int, (int[] a, int[] b)> func, char[][] input)
        {
            List<int> output = new List<int>();

            for (int i = 0; i < input[0].Length / 3; i += 3)
            {
                (int[] a, int[] b) = func(i);
                output.Add(Base5(map, input[a[0]][a[1]], input[a[2]][a[3]], input[a[4]][a[5]]));
                output.Add(Base5(map, input[b[0]][b[1]], input[b[2]][b[3]], input[b[4]][b[5]]));
            }
            return output.ToArray();
        }

        private static IEnumerable<Func<int, (int[] a, int[] b)>> GetTrigramMappings()
        {
            yield return i => ([0, i, 0, i + 1, 1, i], [1, i + 2, 1, i + 1, 0, i + 2]);
            yield return i => ([0, i, 0, i + 1, 1, i], [1, i + 1, 0, i + 2, 1, i + 2]);
            yield return i => ([1, i, 0, i, 0, i + 1], [1, i + 1, 0, i + 2, 1, i + 2]);
        }

        private static void CheckMod6()
        {
            foreach (var eyeMessage in EyeMessages.Combined)
            {
                var count = GetInner(eyeMessage.Chars).Select(e => e.Count()).ToArray();
                Console.WriteLine(string.Join(", ", count.Select(e => e % 3)));
                Console.WriteLine(string.Join(", ", count.Select(e => e % 6)));

                for (int i = 0; i < count.Length / 2; i += 2)
                {
                    int value = count[i] + count[i + 1];
                    Console.WriteLine($"Mod 3: {value % 3}, Mod 6: {value % 6}");
                }
            }
        }

        public static IEnumerable<IEnumerable<char>> GetInner(IEnumerable<char> input)
        {
            List<char> output = new List<char>();
            foreach (char c in input)
            {
                if (c == 'n')
                {
                    yield return output;
                    output.Clear();
                }
                else
                {
                    output.Add(c);
                }
            }
        }

        private static char[] Clean(char[] input)
        {
            return input.Where(c => c != 'n').ToArray();
        }

        private static int[] Trigram(EyeMap map, char[] input)
        {
            char[] cleaned = input.Where(e => e != 'n').ToArray();
            int[] output = new int[cleaned.Length / 3];
            for (int i = 0; i < cleaned.Length; i += 3)
            {
                output[i / 3] = Base5(map, cleaned[i], cleaned[i + 1], cleaned[i + 2]);
            }
            return output;
        }

        private static int Base5(EyeMap map, char a, char b, char c)
        {
            int i_a = map.Map(a);
            int i_b = map.Map(b);
            int i_c = map.Map(c);

            return (i_a * 25) + (i_b * 5) + i_c;
        }

        private static void ModLengths()
        {
            int[] lengths = EyeMessages.Combined.Select(e => e.Chars.Where(x => x != 'n').Count()).ToArray();
            Console.WriteLine(string.Join(", ", lengths));

            for (int i = 1; i < 50; i++)
            {
                Console.WriteLine($"Mod {i}: " + string.Join(" | ", lengths.Select(e => e % i)));
            }
        }
    }

    internal class EyeMap
    {
        private readonly IDictionary<char, int> map;
        public int Map(char c)
        {
            return map[c];
        }

        public EyeMap(IDictionary<char, int> map)
        {
            this.map = map;
        }

        public static IEnumerable<EyeMap> CreateVariants()
        {
            for (int i = 0; i < 5; i++)
            {
                Dictionary<char, int> map = new Dictionary<char, int>
                {
                    {'m', (0 + i) % 5},
                    {'t', (1 + i) % 5},
                    {'r', (2 + i) % 5},
                    {'b', (3 + i) % 5},
                    {'l', (4 + i) % 5},
                };
                yield return new EyeMap(map);
            }
        }
    }

    internal readonly struct EyeMessages
    {
        public char[] Chars { get; }

        private EyeMessages(char[] chars)
        {
            Chars = chars;
        }

        public static EyeMessages East1 = new EyeMessages(['r', 'm', 't', 'm', 't', 'b', 'r', 'r', 'b', 'b', 'm', 'l', 'm', 'l', 't', 't', 'b', 'm', 'r', 'b', 'r', 't', 't', 'l', 'b', 't', 'b', 'm', 'b', 'b', 'm', 'm', 'l', 'm', 'r', 'l', 'm', 'm', 'm', 'n', 'm', 'b', 'r', 'm', 'l', 't', 'r', 'r', 'm', 'm', 'm', 't', 'l', 'r', 'r', 'r', 'l', 'r', 't', 'r', 'r', 'r', 'r', 'm', 't', 't', 'm', 'm', 'm', 'b', 'r', 'm', 't', 'b', 'l', 't', 't', 't', 'b', 'n', 'b', 't', 'm', 'r', 'r', 't', 'm', 'l', 'l', 'm', 'm', 'm', 'r', 'm', 'm', 't', 'm', 'l', 'm', 'l', 'm', 't', 'l', 'l', 't', 'l', 'r', 'm', 'b', 'b', 'm', 'r', 'r', 'm', 'b', 'l', 'r', 'l', 't', 'n', 'r', 'b', 't', 'b', 't', 'b', 't', 'b', 'm', 'm', 'b', 't', 't', 'b', 'r', 't', 'r', 'm', 't', 'l', 'r', 'r', 'b', 't', 'b', 'b', 't', 'l', 'l', 't', 'b', 'l', 't', 'l', 'l', 't', 'r', 't', 't', 'n', 'm', 't', 'l', 'm', 'm', 'b', 'r', 't', 'r', 't', 't', 'l', 't', 'b', 'm', 'm', 'l', 't', 't', 't', 'm', 't', 'm', 'm', 'r', 'l', 't', 'r', 'l', 't', 'm', 'm', 'l', 'm', 'b', 't', 'm', 'm', 't', 'n', 'm', 'l', 'm', 'b', 'b', 't', 'l', 'b', 'r', 'b', 'l', 't', 't', 'r', 'r', 't', 'm', 't', 'm', 't', 'm', 'm', 'l', 'm', 't', 'r', 'm', 'l', 't', 'r', 'l', 'l', 'r', 'l', 'l', 'r', 'l', 'm', 'r', 'n', 't', 'b', 'b', 'b', 't', 'r', 'r', 'm', 'b', 'b', 'm', 't', 'm', 'b', 't', 't', 'b', 't', 't', 't', 'r', 't', 't', 'r', 't', 'm', 'b', 'r', 'r', 'b', 't', 'l', 'n', 't', 'b', 't', 'm', 'l', 'r', 'l', 'r', 'r', 'l', 't', 'b', 'm', 'b', 'm', 'l', 't', 't', 'm', 'r', 'm', 'b', 't', 'r', 'b', 'r', 'm', 'l', 'b', 't', 'b', 'n']);
        public static EyeMessages West1 = new EyeMessages(['b', 't', 't', 'm', 't', 'b', 'r', 'r', 'b', 'b', 'm', 'l', 'm', 'l', 't', 't', 'b', 'm', 'r', 'b', 'r', 't', 't', 'l', 'b', 't', 'b', 'm', 'b', 'b', 'm', 'm', 'l', 'm', 'r', 'l', 'm', 'm', 'l', 'n', 'm', 'b', 'r', 'm', 'l', 't', 'r', 'r', 'm', 'm', 'm', 't', 'l', 'r', 'r', 'r', 'l', 'r', 't', 'r', 'r', 'r', 'r', 'm', 't', 't', 'm', 'm', 'm', 'b', 'r', 'm', 't', 'b', 'l', 't', 't', 'm', 't', 'n', 'm', 'r', 'm', 'r', 'm', 't', 'm', 'l', 'l', 'm', 'm', 'm', 't', 'm', 'l', 'm', 'l', 'l', 'm', 'l', 'm', 't', 'l', 'l', 't', 'l', 'r', 'm', 'b', 'b', 'm', 'r', 'r', 'm', 'b', 'l', 't', 'b', 't', 'n', 't', 't', 't', 'r', 't', 'b', 't', 'b', 'm', 'm', 'm', 't', 't', 'm', 'r', 'm', 'r', 'm', 't', 'l', 'r', 'r', 'b', 't', 'b', 'b', 't', 'l', 'l', 't', 'b', 'l', 't', 'l', 'l', 't', 'l', 'm', 't', 'n', 'r', 't', 'r', 'r', 'r', 'b', 'b', 'm', 'b', 'r', 'l', 'l', 'm', 'm', 'm', 'r', 'l', 'b', 'r', 'b', 't', 't', 't', 'm', 'r', 'r', 't', 'r', 'b', 't', 'm', 'b', 't', 'm', 'r', 'r', 'm', 'l', 'b', 'n', 'l', 'm', 'b', 'l', 'b', 't', 'l', 'm', 't', 'r', 'r', 'r', 't', 't', 't', 'b', 'l', 'm', 'r', 't', 'm', 'b', 'm', 't', 'l', 't', 'b', 'b', 'l', 't', 'r', 'r', 't', 'b', 'b', 'm', 't', 'b', 'r', 'n', 'm', 'r', 'l', 't', 'l', 'r', 'r', 't', 'l', 'r', 'r', 'r', 'm', 'b', 'm', 'r', 'l', 'r', 'm', 'm', 't', 'r', 'b', 'r', 't', 'r', 'l', 'm', 'r', 'b', 'r', 'b', 'r', 'm', 't', 'l', 'm', 'b', 'n', 'b', 't', 'm', 't', 'b', 'r', 'r', 't', 't', 'r', 't', 'b', 'm', 'r', 'm', 'b', 'r', 'r', 'r', 'r', 'm', 'm', 'l', 'r', 'r', 'b', 't', 'm', 'b', 't', 'b', 'r', 'r', 'l', 't', 't', 'b', 'n']);
        public static EyeMessages East2 = new EyeMessages(['t', 'r', 't', 'm', 't', 'b', 'r', 'r', 'b', 'b', 'm', 'l', 'm', 'l', 't', 't', 'b', 'm', 'r', 'b', 'r', 't', 't', 'l', 'b', 't', 'b', 'm', 'b', 'b', 'm', 'm', 'l', 'm', 'r', 'l', 'm', 'm', 'l', 'n', 't', 'b', 'r', 'm', 'l', 't', 'r', 'r', 'm', 'm', 'm', 't', 'l', 'r', 'r', 'r', 'l', 'r', 't', 'r', 'r', 'r', 'r', 'm', 't', 't', 'm', 'm', 'm', 'b', 'r', 'm', 't', 'b', 'l', 't', 't', 'b', 'r', 'n', 'b', 'm', 'r', 'm', 't', 'b', 'r', 'b', 'm', 'm', 'l', 'l', 'r', 't', 'm', 't', 'l', 'b', 'm', 'm', 't', 'r', 't', 'l', 't', 'l', 'm', 'b', 't', 't', 'm', 'r', 'l', 't', 'm', 'l', 'r', 'r', 'b', 'n', 't', 'm', 'r', 'l', 'l', 't', 't', 't', 'b', 'r', 'r', 'r', 'r', 'b', 't', 'l', 'm', 'b', 'b', 'b', 'm', 't', 'b', 'm', 'r', 'b', 't', 'm', 't', 'm', 'b', 'r', 'r', 'l', 'l', 't', 'l', 'r', 'r', 'n', 'm', 't', 'l', 't', 't', 'b', 'm', 'b', 'm', 't', 'l', 'l', 't', 'm', 'r', 'm', 'r', 'm', 'b', 't', 't', 't', 't', 'l', 'r', 'l', 't', 'm', 'b', 'l', 'r', 'b', 'r', 't', 'b', 'r', 't', 't', 'r', 'n', 't', 'l', 't', 't', 'r', 'm', 't', 'r', 'm', 'm', 'l', 'm', 't', 'm', 'b', 'm', 'r', 'r', 't', 'r', 'r', 'l', 'm', 'r', 'm', 'l', 'm', 'm', 'm', 'm', 't', 'm', 'b', 'r', 'r', 't', 'm', 'l', 'm', 'n', 'm', 'm', 't', 't', 'b', 'r', 'r', 't', 'm', 'm', 'l', 'r', 'r', 'b', 't', 'm', 'l', 'b', 'r', 'l', 'r', 'm', 't', 'b', 't', 'm', 'b', 'm', 't', 'm', 'r', 'm', 'm', 'b', 'm', 'm', 'r', 'r', 't', 'n', 'm', 'r', 'm', 't', 'l', 'r', 'r', 'l', 'm', 'b', 't', 'r', 'm', 'b', 't', 'b', 'b', 'm', 'r', 'b', 't', 'm', 'm', 'm', 't', 'm', 'b', 'b', 't', 'm', 'l', 'l', 't', 'r', 'm', 't', 'l', 'r', 'r', 'n', 'm', 'b', 'l', 'r', 'm', 't', 'm', 'l', 'b', 't', 'm', 't', 't', 'm', 'm', 'r', 'm', 'm', 't', 'r', 'l', 'n', 't', 'b', 't', 'l', 'm', 'r', 'm', 'r', 'r', 'm', 'r', 'm', 't', 'l', 't', 'b', 'r', 'r', 'b', 't', 't', 'n']);
        public static EyeMessages West2 = new EyeMessages(['b', 'm', 't', 'm', 't', 'l', 'b', 'm', 'l', 'r', 'b', 't', 't', 't', 't', 't', 'b', 'm', 't', 'm', 'b', 'r', 'm', 'm', 't', 't', 'l', 'r', 't', 't', 't', 'l', 'r', 'm', 'l', 'r', 't', 'l', 'l', 'n', 't', 'b', 'r', 'm', 'l', 't', 'm', 'm', 'r', 'l', 'l', 't', 'r', 'm', 'm', 'r', 'r', 'r', 't', 'l', 't', 'm', 't', 'b', 'r', 'l', 'm', 'm', 'r', 'r', 'r', 'r', 'm', 't', 'r', 'm', 'l', 'm', 'r', 'n', 't', 't', 'm', 't', 'r', 'm', 'r', 't', 'm', 'm', 'l', 'l', 'm', 't', 'r', 'm', 'r', 'r', 'm', 't', 'l', 't', 'm', 'm', 'r', 'm', 'r', 't', 'b', 'm', 'm', 't', 'b', 'r', 'l', 'b', 'b', 't', 'r', 'n', 'l', 'm', 't', 't', 'b', 'm', 't', 't', 'r', 'm', 't', 'm', 'b', 'r', 'r', 'b', 't', 'b', 'l', 'b', 't', 'l', 'r', 'r', 'b', 't', 'b', 'r', 't', 'b', 'm', 'b', 't', 't', 'm', 'm', 'm', 'm', 'b', 'n', 't', 'l', 'b', 't', 't', 'm', 'r', 'r', 'b', 'm', 'r', 'l', 'r', 'r', 'l', 'r', 'm', 't', 'm', 'r', 't', 'r', 'r', 'b', 't', 'l', 'r', 'r', 'm', 'm', 't', 'm', 'b', 't', 't', 't', 'r', 'r', 'b', 'n', 'r', 'm', 'b', 'l', 'm', 't', 'r', 'b', 'm', 'm', 'l', 't', 'r', 'r', 'r', 'r', 't', 'b', 't', 'b', 'r', 'r', 'r', 'm', 'r', 'b', 'm', 'r', 'l', 'r', 't', 'l', 'm', 'r', 't', 't', 'l', 'l', 'm', 'n', 't', 'r', 'r', 'r', 'm', 't', 'm', 'm', 'm', 'm', 't', 'r', 't', 'l', 'b', 't', 'm', 't', 'r', 'b', 'b', 'b', 't', 'r', 'm', 't', 'm', 'r', 'r', 'l', 'r', 'm', 'b', 'r', 'r', 't', 'n', 'm', 't', 't', 'm', 't', 'm', 't', 'm', 't', 'b', 'r', 't', 'r', 'b', 't', 't', 'm', 'b', 'm', 'b', 'r', 'm', 'b', 'm', 'r', 'l', 't', 'b', 'r', 'm', 'b', 'r', 'r', 'm', 'b', 'm', 'n']);
        public static EyeMessages East3 = new EyeMessages(['r', 'r', 't', 'm', 't', 'l', 'b', 'm', 'l', 'm', 'm', 'm', 't', 'm', 'm', 'b', 'm', 'r', 'r', 'r', 'm', 'r', 'b', 't', 'r', 'r', 'r', 'r', 'b', 'r', 't', 'l', 'l', 't', 'l', 'l', 'r', 't', 't', 'n', 'b', 'b', 'r', 'm', 'l', 't', 'm', 'm', 'r', 'r', 'r', 'r', 'l', 'b', 't', 'b', 'l', 't', 'm', 'm', 'b', 'r', 'l', 'r', 'm', 'm', 'm', 'm', 't', 'm', 'r', 'r', 'm', 'm', 'l', 'r', 'l', 'b', 't', 'n', 'b', 't', 'b', 'r', 'r', 'b', 'b', 't', 'r', 't', 'r', 'm', 't', 'b', 'l', 'm', 'm', 'l', 't', 'l', 't', 'b', 'm', 'r', 'b', 't', 'm', 'm', 'm', 't', 'r', 'b', 't', 'm', 'l', 'b', 't', 'b', 'm', 'n', 'm', 'r', 'm', 'm', 'r', 'm', 't', 'l', 'm', 'm', 'm', 'r', 'm', 'r', 't', 'r', 't', 'r', 'b', 't', 't', 't', 'm', 'm', 'm', 'm', 'b', 't', 't', 'r', 'r', 'r', 'm', 't', 't', 'm', 'm', 'b', 'r', 'n', 't', 'l', 'm', 'r', 't', 'l', 'r', 'r', 'r', 'm', 'r', 'b', 'm', 'l', 'r', 'm', 'm', 't', 'r', 't', 'l', 'r', 'l', 't', 'r', 't', 't', 'r', 'r', 'b', 't', 'm', 'l', 'm', 't', 'm', 'm', 'b', 'l', 'n', 'm', 'm', 'b', 'm', 'r', 't', 'm', 'b', 't', 'b', 'm', 'm', 'r', 't', 'r', 'r', 't', 'm', 'b', 't', 'm', 'm', 'm', 'm', 'b', 't', 'r', 'b', 'b', 'r', 'm', 'm', 'b', 'r', 'l', 'm', 'l', 'r', 'r', 'n', 'm', 'm', 't', 'r', 'l', 'm', 'r', 'l', 't', 'm', 'r', 'm', 'r', 'b', 'r', 'm', 'l', 'b', 'm', 'l', 'b', 'm', 'b', 't', 'r', 'r', 'l', 't', 'b', 't', 'b', 't', 'r', 'b', 'm', 't', 't', 'l', 'r', 'n', 'r', 'b', 'r', 'b', 't', 't', 't', 'b', 'm', 'r', 't', 't', 'm', 'r', 't', 'm', 'r', 'm', 'r', 'r', 'r', 'b', 'l', 't', 'l', 't', 'r', 't', 't', 'b', 'r', 'l', 'm', 'b', 'r', 't', 'r', 'b', 'm', 'n', 'm', 'm', 't', 'm', 'b', 'm', 't', 'r', 'l', 'r', 'r', 't', 'r', 'r', 'l', 'm', 'b', 'b', 'm', 'm', 'b', 'r', 't', 't', 'm', 'r', 'l', 'r', 't', 'b', 't', 'b', 'b', 'r', 'b', 't', 'm', 'm', 't', 'n', 'l', 't', 'm', 'r', 't', 'm', 't', 'm', 'b', 'b', 'm', 'm', 'l', 'b', 'r', 'm', 'b', 't', 'l', 't', 'r', 't', 't', 't', 'l', 'r', 'r', 'b', 'b', 'm', 'l', 'm', 'b', 'l', 'm', 'm', 'm', 'l', 't', 'n', 'm', 'l', 't', 'r', 'l', 'm', 't', 'r', 'b', 'm', 'l', 'n', 'm', 'l', 'r', 'b', 'm', 't', 'm', 't', 'm', 'l', 'n']);
        public static EyeMessages West3 = new EyeMessages(['t', 't', 't', 'm', 't', 'l', 'b', 'm', 'l', 'm', 'l', 'l', 'm', 'r', 'b', 't', 'm', 't', 'm', 'b', 'b', 'r', 'b', 'r', 't', 'r', 'm', 't', 't', 'b', 'r', 'l', 'm', 'm', 'b', 'r', 'm', 'r', 'b', 'n', 'l', 'b', 'r', 'm', 'l', 't', 'm', 'm', 'r', 'b', 'l', 'r', 't', 'r', 'm', 'b', 'm', 't', 'l', 'l', 't', 'r', 't', 'r', 'r', 'r', 'r', 'l', 'm', 't', 'l', 'r', 'm', 'r', 't', 't', 't', 'b', 'm', 'n', 'm', 'b', 'b', 'm', 'b', 't', 't', 'b', 'l', 'r', 'r', 'l', 't', 'l', 'l', 't', 't', 't', 'b', 'm', 'b', 'm', 'm', 'b', 't', 'l', 'r', 'r', 'b', 'l', 'm', 'l', 'r', 't', 'b', 't', 't', 't', 'r', 'n', 'l', 'b', 't', 'l', 't', 'b', 'r', 'm', 'm', 'r', 't', 'm', 't', 'l', 't', 'r', 'm', 'r', 't', 't', 'r', 'l', 'b', 't', 'r', 'b', 'm', 'r', 'm', 'b', 't', 't', 't', 'l', 'b', 'm', 'm', 'r', 't', 'n', 't', 'm', 'b', 't', 'b', 'b', 'r', 't', 'l', 'r', 'm', 'm', 'r', 'b', 'm', 'm', 't', 't', 't', 'l', 'b', 'm', 'b', 'l', 't', 'l', 'b', 'm', 'b', 'b', 't', 't', 'm', 't', 'r', 'r', 't', 'r', 'm', 'n', 't', 'm', 't', 't', 'b', 'r', 'r', 't', 't', 't', 'r', 'm', 'l', 'l', 'r', 'b', 't', 'm', 't', 'b', 't', 'b', 'r', 't', 'r', 'b', 't', 'm', 'r', 'm', 'b', 't', 't', 'm', 'r', 'r', 'r', 'm', 'm', 'n', 't', 'r', 'm', 't', 'r', 'm', 't', 'r', 'b', 't', 'b', 'm', 'm', 't', 't', 'm', 'r', 'l', 'm', 't', 'l', 't', 'b', 'b', 'm', 'r', 't', 'm', 'r', 'b', 'm', 'm', 'r', 'r', 'r', 'm', 'm', 'l', 'l', 'n', 'r', 't', 'm', 'b', 't', 'r', 'r', 'r', 'm', 'm', 'm', 't', 'l', 'l', 'm', 't', 'r', 'r', 'm', 'm', 'b', 'r', 'b', 'r', 't', 'l', 'r', 't', 'l', 't', 'b', 'b', 'r', 't', 'b', 't', 'r', 'r', 'm', 'n', 't', 'r', 'm', 'r', 'r', 'l', 'm', 'r', 'r', 'r', 'b', 'l', 'r', 'm', 'b', 'm', 'b', 'b', 't', 'r', 'm', 'r', 'l', 'l', 'm', 'l', 'm', 'r', 'm', 'm', 'n', 'm', 'm', 'r', 't', 'r', 't', 't', 'm', 'm', 't', 'l', 't', 't', 'm', 'r', 'r', 'l', 'r', 't', 'm', 'b', 'l', 'm', 'r', 'l', 't', 't', 'l', 'l', 'r', 'n']);
        public static EyeMessages East4 = new EyeMessages(['t', 'm', 't', 'm', 't', 'l', 'b', 'm', 'l', 'm', 'm', 'm', 't', 'm', 'm', 'm', 'm', 'm', 'm', 't', 'm', 'r', 't', 'b', 'r', 'b', 'b', 't', 'r', 'm', 't', 'l', 'r', 't', 'b', 'b', 'm', 'm', 'b', 'n', 'r', 'b', 'r', 'm', 'l', 't', 'm', 'm', 'r', 'r', 'r', 'r', 'l', 'b', 't', 'r', 't', 'r', 'l', 'b', 'm', 'l', 'b', 'm', 'b', 'm', 'm', 't', 't', 'm', 'r', 'm', 'b', 'l', 'r', 't', 't', 'b', 'm', 'n', 't', 'm', 't', 'm', 'm', 'l', 'r', 'r', 'b', 'r', 't', 'm', 'm', 'b', 'l', 'b', 'm', 'm', 't', 'l', 'l', 'r', 't', 'l', 'r', 'r', 'l', 'm', 'r', 'r', 'r', 'm', 'm', 'b', 'm', 'm', 'm', 'r', 'r', 'n', 'b', 'm', 'b', 'l', 't', 't', 'm', 'r', 'r', 'b', 't', 'b', 'r', 'm', 'r', 'l', 'm', 'b', 'b', 'm', 'r', 'm', 'b', 'm', 'r', 'r', 'r', 'l', 'l', 't', 't', 'l', 'r', 'm', 't', 'm', 't', 'l', 't', 'n', 't', 'l', 'b', 'r', 'b', 'l', 'b', 'm', 'm', 't', 'r', 'm', 'r', 'l', 'r', 'r', 'b', 'm', 't', 't', 'm', 'b', 'm', 't', 'b', 'm', 'r', 'm', 'm', 't', 'm', 'l', 'm', 'm', 'b', 'm', 't', 'b', 'm', 'n', 'm', 't', 'r', 'b', 'b', 'r', 'l', 'm', 't', 'b', 'l', 't', 'b', 'l', 't', 'b', 'm', 'r', 'l', 'l', 't', 'b', 'm', 't', 'l', 't', 'r', 'l', 't', 'r', 'r', 'r', 'r', 'b', 'm', 'b', 'b', 'r', 'r', 'n', 'r', 't', 'r', 'r', 'r', 'r', 't', 'l', 'b', 't', 'b', 'm', 'b', 'm', 'r', 'm', 't', 'b', 't', 'm', 'r', 't', 't', 'b', 't', 'm', 'r', 'r', 'b', 'm', 'm', 'm', 'b', 't', 'm', 'b', 'r', 'b', 'r', 'n', 'l', 'b', 'r', 'b', 'b', 't', 'l', 't', 't', 'm', 'b', 'r', 'l', 'm', 'b', 'r', 'm', 'm', 't', 'r', 'r', 't', 'm', 'b', 't', 't', 'r', 'l', 'b', 't', 'l', 'l', 'm', 't', 'r', 'm', 'r', 'b', 't', 'n', 't', 'r', 'r', 'm', 'r', 'l', 'r', 'b', 'm', 't', 'm', 't', 'b', 't', 't', 'r', 'b', 'r', 'r', 't', 'b', 'm', 'b', 'n', 'b', 'l', 'r', 't', 'r', 't', 'm', 'r', 'r', 'm', 't', 'm', 'm', 'b', 'r', 'b', 'm', 'b', 'l', 'm', 'b', 'l', 'n']);
        public static EyeMessages West4 = new EyeMessages(['b', 'm', 't', 'm', 't', 'l', 'b', 'm', 'l', 'm', 'm', 'm', 't', 'm', 'm', 'm', 'm', 'm', 'm', 't', 'm', 'r', 't', 'b', 'r', 'b', 'b', 't', 'r', 'm', 't', 'l', 'm', 'm', 'l', 'm', 'm', 'm', 'r', 'n', 'r', 'b', 'r', 'm', 'l', 't', 'm', 'm', 'r', 'r', 'r', 'r', 'l', 'b', 't', 'r', 't', 'r', 'l', 'b', 'm', 'l', 'b', 'm', 'b', 'm', 'm', 't', 't', 'm', 'r', 'r', 'r', 't', 't', 'b', 't', 'l', 'r', 'n', 'r', 't', 't', 'b', 't', 'm', 'r', 't', 'l', 'm', 'm', 't', 'm', 'b', 'r', 't', 'r', 'r', 'r', 'l', 't', 't', 'r', 'l', 'b', 'm', 'm', 't', 'm', 'm', 't', 'b', 't', 'r', 'r', 'b', 'b', 't', 'b', 'n', 'm', 'b', 'm', 'r', 'r', 't', 'r', 'b', 'm', 't', 'b', 'r', 'b', 'm', 't', 'l', 'b', 'm', 'l', 't', 'b', 'l', 'r', 'm', 'b', 'm', 'm', 'm', 'b', 'r', 'b', 'b', 'r', 'l', 'r', 't', 't', 'l', 'm', 'n', 'm', 'l', 'm', 'r', 't', 'm', 'r', 'l', 'm', 't', 'm', 'b', 'r', 'm', 'r', 'r', 't', 'm', 'r', 'l', 'b', 'm', 'r', 't', 'm', 't', 'r', 't', 'm', 'b', 'm', 't', 'r', 'm', 'b', 'b', 'r', 'b', 'r', 'n', 'l', 'm', 'r', 'r', 't', 't', 't', 'm', 'b', 't', 'b', 'r', 'l', 't', 'r', 't', 'm', 'r', 't', 'l', 'r', 'l', 'l', 'm', 'b', 't', 't', 't', 'r', 'r', 'm', 'r', 't', 'l', 'b', 't', 't', 'l', 't', 'n', 'r', 'm', 'l', 'r', 'b', 'b', 'r', 'l', 't', 'r', 'm', 'b', 'b', 'm', 'r', 'm', 'r', 'b', 'b', 'm', 't', 'm', 'l', 't', 'r', 'm', 'l', 'r', 'l', 't', 'm', 't', 'r', 'r', 'b', 'r', 't', 'm', 't', 'n', 'b', 't', 't', 't', 'l', 'm', 'b', 't', 't', 'l', 'r', 't', 'r', 'b', 'r', 't', 'r', 'r', 'l', 't', 'm', 'r', 'l', 'm', 't', 'b', 'r', 'l', 'l', 'm', 'm', 'b', 'm', 'r', 'r', 't', 'l', 'l', 'm', 'n', 'r', 'r', 'l', 'b', 't', 'l', 't', 't', 'l', 'm', 'l', 'r', 't', 'r', 't', 't', 't', 'l', 't', 'l', 'm', 't', 'b', 'm', 'n', 'm', 'r', 'm', 'r', 'b', 't', 'm', 'm', 'm', 'm', 'b', 't', 'm', 'm', 'm', 't', 'm', 'r', 't', 'l', 'm', 'm', 't', 't', 'n']);
        public static EyeMessages East5 = new EyeMessages(['t', 't', 't', 'm', 't', 'l', 'b', 'm', 'l', 'm', 'm', 'm', 't', 'm', 'm', 'm', 'm', 'm', 'm', 't', 'm', 'r', 't', 'b', 'r', 'b', 'b', 't', 'r', 'm', 't', 'l', 'b', 'm', 'l', 'l', 't', 'b', 'b', 'n', 'b', 'b', 'r', 'm', 'l', 't', 'm', 'm', 'r', 'r', 'r', 'r', 'l', 'b', 't', 'r', 't', 'r', 'l', 'b', 'm', 'l', 'b', 'm', 'b', 'm', 'm', 't', 't', 'm', 'r', 't', 't', 't', 't', 'r', 'l', 'b', 'm', 'n', 't', 'm', 't', 'r', 't', 'l', 'r', 'r', 'b', 'b', 'm', 'r', 'm', 'r', 'l', 'm', 't', 'l', 't', 'l', 'l', 'r', 't', 'r', 'r', 'r', 'r', 'r', 'b', 'm', 'r', 't', 'r', 'r', 't', 'b', 'r', 'b', 'b', 'n', 'b', 'm', 'b', 'l', 't', 't', 'm', 'r', 'r', 'l', 'm', 't', 'r', 'm', 'r', 'm', 'l', 't', 'b', 'm', 'r', 'm', 'm', 'r', 'r', 'l', 'r', 'l', 'r', 'm', 'r', 'l', 'm', 'b', 'l', 't', 'r', 'm', 'r', 'n', 'm', 't', 'l', 't', 't', 'm', 't', 't', 'l', 't', 'm', 'b', 't', 't', 't', 'm', 't', 'm', 'r', 'l', 'm', 't', 't', 'm', 'r', 'm', 'l', 'm', 't', 'm', 'm', 't', 'b', 't', 'm', 'm', 't', 'b', 'm', 'n', 'r', 't', 't', 'r', 't', 't', 't', 'b', 'm', 't', 't', 'm', 'l', 'l', 't', 'r', 't', 't', 't', 't', 'r', 'l', 'm', 'b', 'l', 't', 'm', 't', 'r', 'r', 'm', 'l', 'm', 'm', 'l', 't', 'r', 't', 'b', 'n', 't', 'm', 'r', 'm', 'l', 't', 'm', 'l', 't', 'r', 'r', 't', 't', 'b', 'l', 't', 'b', 'm', 't', 'b', 'b', 'm', 't', 'b', 'r', 'l', 'b', 'm', 't', 't', 'm', 'l', 'r', 'm', 't', 'm', 'r', 'r', 't', 'n', 'm', 'r', 'm', 'r', 'm', 'b', 'm', 'm', 'r', 'r', 'l', 'm', 'm', 't', 'm', 't', 'r', 'm', 'l', 'l', 'r', 'b', 't', 't', 'm', 'l', 'r', 't', 't', 't', 't', 'l', 'r', 'm', 'b', 't', 't', 'm', 'r', 'n', 't', 'b', 't', 'r', 'r', 'l', 'r', 'r', 'm', 'r', 'r', 'r', 'm', 'l', 't', 'n', 'r', 'b', 'r', 'l', 'l', 'r', 't', 'm', 't', 'b', 'b', 't', 'l', 'b', 't', 'n']);

        public static EyeMessages[] Combined = [East1, West1, East2, West2, East3, West3, East4, West4, East5];
    }
}
