using System.Collections.Generic;
using System.Linq;

public static class Tokenizer
{
    public enum TokenType
    {
        TokenWord,
        TokenQuoted1String,
        TokenQuoted2String,
        TokenRoundBracketOpen,
        TokenRoundBracketClose,
        TokenSquareBracketOpen,
        TokenSquareBracketClose,
        TokenEnumStart,
        TokenEnumEnd
    }

    public class Token
    {
        public TokenType tokenType;

        public Token(TokenType tTokenType)
        {
            tokenType = tTokenType;
        }

        public override string ToString()
        {
            switch (tokenType)
            {
                case TokenType.TokenWord:
                    {
                        return "<WORD>";
                    }

                case TokenType.TokenQuoted1String:
                    {
                        return "\"<TEXT>\"";
                    }

                case TokenType.TokenQuoted2String:
                    {
                        return "'<TEXT>'";
                    }

                case TokenType.TokenRoundBracketOpen:
                    {
                        return "(";
                    }

                case TokenType.TokenRoundBracketClose:
                    {
                        return ")";
                    }

                case TokenType.TokenSquareBracketOpen:
                    {
                        return "[";
                    }

                case TokenType.TokenSquareBracketClose:
                    {
                        return "]";
                    }

                case TokenType.TokenEnumStart:
                    {
                        return "{-";
                    }

                case TokenType.TokenEnumEnd:
                    {
                        return "-}";
                    }

                default:
                    {
                        return "<???>";
                    }
            }
        }
    }

    public class TokenStr : Token
    {
        public string item = "";

        public TokenStr(TokenType tTokenType) : base(tTokenType)
        {
        }

        public TokenStr(TokenType tTokenType, string tStr) : base(tTokenType)
        {
            item = tStr;
        }

        public override string ToString()
        {
            switch (tokenType)
            {
                case TokenType.TokenWord:
                    {
                        return item;
                    }

                case TokenType.TokenQuoted1String:
                    {
                        return "\"" + item + "\"";
                    }

                case TokenType.TokenQuoted2String:
                    {
                        return "'" + item + "'";
                    }

                default:
                    {
                        return base.ToString();
                    }
            }
        }
    }

    public static List<Token> Tokenize(string text)
    {
        var tokens = new List<Token>();
        bool flagQuotes1 = false;
        bool flagQuotes2 = false;
        Stack<bool> flagsEnum = new Stack<bool>();
        bool flagEnum = false;
        flagsEnum.Push(false);
        bool flagWord = false;

        char p = '\0';
        bool cancel = false;

        for (var i = 0; i <= text.Length - 1; i++)
        {
            if (flagQuotes1 || flagQuotes2)
            {
                cancel = !cancel && p == '\\';
                p = text[i];

                if (p == '\\' && !cancel)
                {
                    continue;
                }
            }

            if (text[i] == '\"' && !flagQuotes2 && !cancel)
            {
                flagQuotes1 = !flagQuotes1;
                p = '\0';

                if (flagQuotes1)
                {
                    flagWord = true;

                    if (!flagEnum && flagsEnum.Peek())
                    {
                        flagsEnum.Pop();
                        flagsEnum.Push(false);
                        tokens.Insert(tokens.Count - 1, new Token(TokenType.TokenEnumEnd));
                    }

                    tokens.Add(new TokenStr(TokenType.TokenQuoted1String));
                }

                continue;
            }
            else if (text[i] == '\'' && !flagQuotes1 && !cancel)
            {
                flagQuotes2 = !flagQuotes2;
                p = '\0';

                if (flagQuotes2)
                {
                    flagWord = true;

                    if (!flagEnum & flagsEnum.Peek())
                    {
                        flagsEnum.Pop();
                        flagsEnum.Push(false);
                        tokens.Insert(tokens.Count - 1, new Token(TokenType.TokenEnumEnd));
                    }

                    tokens.Add(new TokenStr(TokenType.TokenQuoted2String));
                }

                continue;
            }

            if (flagQuotes1 || flagQuotes2)
            {
                if ((flagQuotes1 && (p != '\"' || cancel)) || (flagQuotes2 && (p != '\'' || cancel)))
                {
                    ((TokenStr)tokens.Last()).item += text[i];
                }
            }
            else
            {
                switch (text[i].ToString())
                {
                    case "(":
                        {
                            flagWord = false;
                            flagsEnum.Push(false);
                            tokens.Add(new Token(TokenType.TokenRoundBracketOpen));
                            break;
                        }

                    case ")":
                        {
                            flagWord = false;

                            if (flagsEnum.Pop())
                            {
                                tokens.Add(new Token(TokenType.TokenEnumEnd));
                            }

                            tokens.Add(new Token(TokenType.TokenRoundBracketClose));
                            break;
                        }

                    case "[":
                        {
                            flagWord = false;
                            flagsEnum.Push(false);
                            tokens.Add(new Token(TokenType.TokenSquareBracketOpen));
                            break;
                        }

                    case "]":
                        {
                            flagWord = false;

                            if (flagsEnum.Pop())
                            {
                                tokens.Add(new Token(TokenType.TokenEnumEnd));
                            }

                            tokens.Add(new Token(TokenType.TokenSquareBracketClose));
                            break;
                        }

                    case " ":
                        {
                            if (flagWord)
                            {
                                flagWord = false;
                                flagEnum = false;
                            }

                            break;
                        }

                    case ",":
                        {
                            flagWord = false;
                            flagEnum = true;

                            if (!flagsEnum.Peek())
                            {
                                flagsEnum.Pop();
                                flagsEnum.Push(true);
                                tokens.Insert(tokens.Count - 1, new Token(TokenType.TokenEnumStart));
                            }

                            break;
                        }

                    default:
                        {
                            if (flagWord)
                            {
                                ((TokenStr)tokens.Last()).item += text[i];
                            }
                            else
                            {
                                flagWord = true;

                                if (!flagEnum && flagsEnum.Peek())
                                {
                                    flagsEnum.Pop();
                                    flagsEnum.Push(false);
                                    tokens.Insert(tokens.Count - 1, new Token(TokenType.TokenEnumEnd));
                                }

                                tokens.Add(new TokenStr(TokenType.TokenWord, text[i].ToString()));
                            }

                            break;
                        }
                }
            }
        }
        return tokens;
    }

}