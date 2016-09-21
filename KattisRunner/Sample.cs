namespace KattisRunner
{
    public class Sample
    {
        public string Input { get; private set; }
        public string Output { get; private set; }
        public string Name { get; private set; }

        public Sample(string name, string input, string output)
        {
            Name = name;
            Input = input;
            Output = output;
        }
    }
}
