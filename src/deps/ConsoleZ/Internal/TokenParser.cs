using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleZ.Internal
{
    public struct Token<T>
    {
        public int Index { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public bool IsLiteral { get; set; }

        public string RawText { get; set; }
        public string Text { get; set; }
        public T Data { get; set; }

        public override string ToString()
        {
            return $"{nameof(Start)}: {Start}, {nameof(End)}: {End}, {nameof(Text)}: {Text}";
        }
    }

    public abstract class TokenParser<T>
    {
        public List<Token<T>> Tokens { get; set; } = new List<Token<T>>();
        
        public void Scan(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Tokens.Clear();

            var c = 0; // cursor
            while (c < text.Length)
            {
                if (IsStart(text, c, text[c]))
                {
                    var last = Tokens.Any() 
                        ? Tokens.LastOrDefault().End + 1
                        : 0;
                    if (c != last)
                    {
                        var t = CreateToken(text, last, c - 1, true);
                        t.Index = Tokens.Count - 1;
                        Tokens.Add(t);
                    }

                    var e = c + 1;
                    while (e < text.Length)
                    {
                        if (IsEnd(text, c, e, text[e]))
                        {
                            var t = CreateToken(text, c, e, false);
                            t.Index = Tokens.Count - 1;
                            Tokens.Add(t);
                            break;
                        }

                        e++;
                    }
                }

                c++;
            }
            var llast = Tokens.Any() 
                ? Tokens.LastOrDefault().End + 1
                : 0;
            if (c > 0 && c != llast)
            {
                var t = CreateToken(text, llast, c - 1, true);
                t.Index = Tokens.Count - 1;
                Tokens.Add(t);
            }
        }

        public string Render(Func<int, Token<T>, string> renderToken)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < Tokens.Count; i++)
            {
                sb.Append(renderToken(i, Tokens[i]));
            }
            
            return sb.ToString();
        }

        protected abstract bool IsStart(string text, int index, char c);

        protected abstract bool IsEnd(string text, int start, int index, char endC);

        protected abstract Token<T> CreateToken(string text, int start, int end, bool isLiteral);


        public bool TryGetPreviousNonLiteral(Token<T> token, out Token<T> prev)
        {
            var i = token.Index - 1;
            while (i > 0)
            {
                if (!Tokens[i].IsLiteral)
                {
                    prev = Tokens[i];
                    return true;
                }
                i--;
            }

            prev = default(Token<T>);
            return false;
        }

        public bool TryGetLastNonLiteral(out Token<T> prev)
        {
            var i = Tokens.Count - 1;
            while (i > 0)
            {
                if (!Tokens[i].IsLiteral)
                {
                    prev = Tokens[i];
                    return true;
                }
                i--;
            }

            prev = default(Token<T>);
            return false;
        }
    }

    public class ConsoleTokenParser : TokenParser<string>
    {
        protected override Token<string> CreateToken(string text,int start, int end, bool isLiteral)
        {
            var raw = text.Substring(start, end - start + 1);
            return new Token<string>()
            {
                Start = start,
                End = end,
                RawText = raw,
                Text = raw.Trim('^', ';'),
                IsLiteral = isLiteral
            };
        }

        protected override bool IsStart(string text, int index, char c)
        {
            return c == '^';
        }

        protected override bool IsEnd(string text, int start, int index, char endC)
        {
            return endC == ';';
        }


        
    }
}
