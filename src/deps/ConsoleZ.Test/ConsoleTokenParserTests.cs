using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ConsoleZ.Internal;
using Xunit;
using Xunit.Abstractions;

namespace ConsoleZ.Test
{
    public class ConsoleTokenParserTests
    {
        private ITestOutputHelper outp;

        public ConsoleTokenParserTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void CanTokenize()
        {
            var parser = new ConsoleTokenParser();
            parser.Scan( "Hello ^red;World!^;");

            Assert.Equal(
                new string[]
                {
                    "Hello ",
                    "red",
                    "World!",
                    ""
                }, 
                parser.Tokens.Select(x=>x.Text).ToArray()
            );
        }
        
        [Fact]
        public void CanRenderRaw()
        {
            var parser = new ConsoleTokenParser();

            var imp = "Hello ^red;World!^;";
            parser.Scan(imp);

            var raw = parser.Render((i, t) => t.RawText);
            Assert.Equal(imp, raw);
        }
        
        [Fact]
        public void CanRenderBasic()
        {
            var parser = new ConsoleTokenParser();

            var imp = "Hello ^red;World!^;";
            parser.Scan(imp);


            var raw = parser.Render((i, t) => t.Text);
            Assert.Equal("Hello redWorld!", raw);
        }

        [Fact]
        public void Empty()
        {
            var parser = new ConsoleTokenParser();

            var imp = "No tokens at all";
            parser.Scan(imp);


            var raw = parser.Render((i, x) => x.IsLiteral ? x.Text : x.Text.Trim('^', ';'));
            Assert.Equal("No tokens at all", raw);
        }


        
    }
}