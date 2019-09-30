using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokoSolve.Core.Debugger
{
    public static class DebugHelper
    {

        /// <summary>
        /// CPU Name/Identifier
        /// </summary>
        public static string GetCPUDescription() => System.Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");

        // Not WIN32
        //public static string GetCPUDescription()
        //{
        //    try
        //    {
        //        var sb = new StringBuilder();
        //        var searcher = new ManagementObjectSearcher("Select * from Win32_Processor");
        //        var results = searcher.Get();
        //        if (results != null)
        //        {
        //            //http://msdn2.microsoft.com/en-us/library/aa394373(VS.85).aspx
        //            foreach (var result in results)
        //            {
        //                sb.AppendFormat("{0} with {1} cores. ", result["Name"], result["NumberOfCores"]);
        //            }
        //        }
        //        return sb.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        return "Cannot get CPU Type, error:" + ex.Message;
        //    }
        //}
    }


}
