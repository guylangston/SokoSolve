using Xunit;

namespace ConsoleZ.Test
{
    public class HtmlConsoleRendererTests
    {
        [Fact]
        public void Spans_Colours()
        {
            var cons = new HtmlConsoleRenderer();
            var inp = "Hello ^red;World!^;";
            var res = cons.RenderLine(null, 0, inp);

            Assert.Equal("Hello <span style=\"color:red;\">World!</span>", res);
        }


        [Fact]
        public void Spans_Bold()
        {
            var cons = new HtmlConsoleRenderer();
            var inp = "Hello **World!**";
            var res = cons.RenderLine(null, 0, inp);

            Assert.Equal("Hello <b>World!</b>", res);
        }


        [Fact]
        public void Headers()
        {
            var cons = new HtmlConsoleRenderer();
            var inp = "Hello ^red;World!^;";
            var res = cons.RenderLine(null, 0, inp);

            Assert.Equal("Hello <span style=\"color:red;\">World!</span>", res);
        }
    }
}