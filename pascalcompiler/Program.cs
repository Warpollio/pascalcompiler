using System;
using System.IO;

namespace pascalcompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args[1] != "-lex")
            {
                Console.WriteLine("Usage: PascalLexer.exe <filename> -lex");
                return;
            }
            string filename = args[0];
            StreamReader reader = new StreamReader(@"..\..\..\LexerTests\" + filename);

            Lexer lexer = new Lexer(reader);
            Token next;

            string a = "1";
            while (a != "0")
            {
                next = lexer.GetNextToken();
                //if (next.type != Type.COMMENT)
                {
                    Console.WriteLine(next);
                }
                if (next.type == Type.EOF) break;
                a = Console.ReadLine();

            }
        }
    }
}
