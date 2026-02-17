using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleZ;

namespace ConsoleZ.Playground.Console
{
    public class TestHelper
    {
        public static char GetCharOffset(char start, int index) => (char) (start + index);

    }
}
