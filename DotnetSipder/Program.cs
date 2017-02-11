using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;


namespace MyDotnetSipder
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SS_Server_Factory.SS_Load_Server_Setting_Json(@"spider-config.json");

                bool flagRuning = true;
                foreach (SS_Server_Setting_Model item in SS_Server_Factory.SrvModels)
                {
                    flagRuning = !SS_Server_Factory.Find_SS_Processor_Pipline(item.SrvAddr, new SS_Server_Regex_Processor(item.RegexPattern), flagRuning);
                }

                SS_Server_Factory.SS_Save_Server_Result_Json(@"gui-config.json");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

       
    }

    
}
