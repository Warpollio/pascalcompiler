using System;

namespace pascalcompiler
{
    /*
        expression ::= term { '+' term | '-' term }
        term ::= factor { '*' factor | '/' factor }
        factor ::= number | identifier | '(' expression ')'
     */
    class Parser
    {
        private Lexer lexer;
        private Token currentToken;

        public Parser(Lexer lexer)
        {
            this.lexer = lexer;
            currentToken = lexer.GetNextToken();
        }

        public Node ParseExpression()
        {
            Node node = ParseTerm();

            while ((Lexer.Symbols)currentToken.value == Lexer.Symbols.Plus || (Lexer.Symbols)currentToken.value == Lexer.Symbols.Minus)
            {
                Token op = currentToken;
                Eat(currentToken.value);
                Node right = ParseTerm();
                node = new BinOpNode(node, op, right);
            }

            return node;
        }

        public Node ParseTerm()
        {
            Node node = ParseFactor();

            while ((Lexer.Symbols)currentToken.value == Lexer.Symbols.Multiply || (Lexer.Symbols)currentToken.value == Lexer.Symbols.Divide)
            {
                Token op = currentToken;
                Eat(currentToken.value);
                Node right = ParseFactor();
                node = new BinOpNode(node, op, right);
            }

            return node;
        }

        public Node ParseFactor()
        {
            if (currentToken.type == Type.INTEGER || currentToken.type == Type.DOUBLE)
            {
                Node node = new NumNode(currentToken);
                Eat(currentToken.type);
                return node;
            }
            else if (currentToken.type == Type.IDENTIFIER)
            {
                Node node = new IdNode(currentToken);
                Eat(Type.IDENTIFIER);
                return node;
            }
            else if ((Lexer.Symbols)currentToken.value == Lexer.Symbols.LeftParen)
            {
                Eat(Lexer.Symbols.LeftParen);
                Node node = ParseExpression();
                Eat(Lexer.Symbols.RightParen);
                return node;
            }
            else
            {
                throw new Exception($"Unexpected token {currentToken.type} at position {currentToken.row} {currentToken.pos}");
            }
        }

        private void Eat(Object type)
        {
            if ((Lexer.Symbols)currentToken.value == (Lexer.Symbols)type)
            {
                currentToken = lexer.GetNextToken();
            }
            else
            {
                throw new Exception($"Expected {type} but found {(Lexer.Symbols)currentToken.value} at position {currentToken.row} {currentToken.pos}");
            }
        }
        private void Eat(Type type)
        {
            if (currentToken.type == type)
            {
                currentToken = lexer.GetNextToken();
            }
            else
            {
                throw new Exception($"Expected {type} but found {(Lexer.Symbols)currentToken.value} at position {currentToken.row} {currentToken.pos}");
            }
        }
    }

    class Node
    {
        public override string ToString()
        {
            return Print("", true);
        }

        private string Print(string prefix, bool isTail)
        {
            var result = new System.Text.StringBuilder();

            result.Append(prefix);
            result.Append(isTail ? "└── " : "├── ");

            if (this is NumNode numNode)
            {
                result.Append(numNode.Token.source);
            }
            else if (this is IdNode idNode)
            {
                result.Append(idNode.Token.source);
            }
            else if (this is BinOpNode binOpNode)
            {
                result.Append(binOpNode.Op.source);
                result.Append(Environment.NewLine);

                result.Append(binOpNode.Left.Print(prefix + (isTail ? "    " : "│   "), false));
                result.Append(binOpNode.Right.Print(prefix + (isTail ? "    " : "│   "), true));
            }

            result.Append(Environment.NewLine);

            return result.ToString();
        }
    }

    class BinOpNode : Node
    {
        public Node Left { get; set; }
        public Token Op { get; set; }
        public Node Right { get; set; }

        public BinOpNode(Node left, Token op, Node right)
        {
            Left = left;
            Op = op;
            Right = right;
        }
    }

    class NumNode : Node
    {
        public Token Token { get; set; }

        public NumNode(Token token)
        {
            Token = token;
        }
    }

    class IdNode : Node
    {
        public Token Token { get; set; }

        public IdNode(Token token)
        {
            Token = token;
        }
    }
}
