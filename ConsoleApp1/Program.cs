using UBViews.LexParser;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string qry = "mind and ship filterby parid";
            string message = $"Query String {qry}";
            Console.WriteLine(message);

            ParserService parserService = new ParserService();
            string expr = parserService.ParseQuery(qry).ToString();

            message = $"Query expression {expr}";
            Console.WriteLine(message);
        }
    }
}