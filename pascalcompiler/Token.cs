using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pascalcompiler
{
    public enum Type
    {
        NONE,
        DOUBLE,
        INTEGER,
        STRING,
        IDENTIFIER,
        KEYWORD,
        OPERATOR,
        EOF,
        SEPARATORS,
        SPECIALSYMBOLS,
        COMMENT
    }
    class Token
    {
        public int row;
        public int pos;
        public Type type;
        public object value;
        public string source;

        public override string ToString()
        {
            return "" + row + " " + pos + " " + type + " " + value + " " + source;
        }
    }

    
}
