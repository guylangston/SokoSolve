using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SokoSolve.Client.Web.Logic
{
    public class GraphVisWrapper
    {
        private string dotExec = "/usr/bin/dot";
        private string argTemplate = "-o{1} -Tsvg {0}";
        
        
        public async Task<string> GenerateGraph(string inputText)
        {
            var inFile = Path.GetTempFileName();
            await System.IO.File.WriteAllTextAsync(inFile, inputText);
            
            var outFile = Path.GetTempFileName();
            var args    = string.Format(argTemplate, inFile, outFile);
            var proc = Process.Start(new ProcessStartInfo()
            {
                FileName  = dotExec,
                Arguments = args
            });
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
            {
                throw new Exception($"Exit Code = {proc.ExitCode} `{dotExec} {args}`");
            }
            return outFile;
        }
        public async Task<IActionResult> GetActionResult(string inputText) 
            => new FileStreamResult(System.IO.File.OpenRead(await GenerateGraph(inputText)), "image/svg+xml");
    }
}