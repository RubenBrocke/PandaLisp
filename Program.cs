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
            Primary result = interpreter.VisitRoot(parser.Root);
            System.Console.WriteLine("Result: " + result);
            System.Console.WriteLine("EXECUTION ENDED");
            Console.ReadLine();
        }
    }
}
