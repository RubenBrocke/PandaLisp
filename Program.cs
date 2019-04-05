using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PandaLisp
{
    class Program
    {
        static void Main(string[] args)
        {
            Lexer lexer = new Lexer(File.ReadAllText("input.txt"));
            List<Token> tokens = lexer.Scan();
            lexer.PrintTokens();
            Parser parser = new Parser(tokens);
            parser.Parse();
            AstPrinter astPrinter = new AstPrinter();
            string ast = astPrinter.VisitRoot(parser.Root);
            System.Console.WriteLine(ast);
            Interpreter interpreter = new Interpreter(); 
            List<object> result = (List<object>)interpreter.VisitRoot(parser.Root);
            foreach (object o1 in result)
            {
                foreach (object o2 in (List<object>)o1)
                {
                    System.Console.WriteLine(o2);
                }
            }
            Console.ReadLine();
        }
    }
}
