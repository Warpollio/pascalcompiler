using System;
using System.IO;

namespace pascalcompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            

            

            Console.WriteLine("lex/parse");
            string a;

            a = Console.ReadLine();
            if (a == "lex")
            {
                string filename;

                Console.WriteLine("Enter filename");
                filename = Console.ReadLine();
                StreamReader reader = new StreamReader(@"..\..\..\LexerTests\" + filename);
                Lexer lexer = new Lexer(reader);
                Token next;
                Console.WriteLine("0 to stop, enter for next token");
                a = Console.ReadLine();

                while (a != "0")
                {
                    next = lexer.GetNextToken();
                    if (next.type != Type.COMMENT)
                    {
                        Console.WriteLine(next);
                        a = Console.ReadLine();
                    }

                }
            } 
            else if (a == "parse")
            {
                string filename;
                Console.WriteLine("Enter filename");
                filename = Console.ReadLine();
                StreamReader reader = new StreamReader(@"..\..\..\ParserTests\" + filename);
                Lexer lexer = new Lexer(reader);
                Console.WriteLine("0 to stop, enter for next token");
                Parser parser = new Parser(lexer);
                try
                {
                    Node root = parser.ParseExpression();
                    Console.WriteLine(root);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
