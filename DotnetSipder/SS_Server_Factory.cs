using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetSpider.Core;
using DotnetSpider.Core.Downloader;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using Newtonsoft.Json.Linq;

namespace MyDotnetSipder
{
    /// <summary>
    /// SS处理器工厂
    /// </summary>
    public class SS_Server_Factory
    {
        public static List<SS_Server_Setting_Model> SrvModels = new List<SS_Server_Setting_Model>();
        /// <summary>
        /// 创建爬虫
        /// </summary>
        /// <param name="url">网站网址</param>
        /// <param name="precessor">页面处理器</param>
        public static bool Find_SS_Processor_Pipline(string url, IPageProcessor precessor,bool isKeepRunning)
        {
            if (isKeepRunning)
            {
                var site = new Site { EncodingName = "utf-8", RemoveOutboundLinks = true };
                site.AddStartUrl(url);
                site.Timeout = 10000;

                Spider spider = Spider.Create(
                    site,
                    new QueueDuplicateRemovedScheduler(),
                    precessor
                    ).AddPipeline(new SS_Server_Model_Pipeline())
                    .SetDownloader(new HttpClientDownloader())
                    .SetThreadNum(1);

                spider.Deep = 1;
                spider.EmptySleepTime = 30;
                spider.Run();
            }
            return SS_Server_Model_Pipeline.SrvrResult.Count > 0;
        }

        /// <summary>
        /// model格式转换为json格式字符串
        /// </summary>
        /// <returns></returns>
        public static void SS_Save_Server_Result_Json(string path)
        {
            JObject jobj = new JObject();
            jobj["strategy"] = null;
            jobj["index"] = 2;
            jobj["global"] = false;
            jobj["enabled"] = true;
            jobj["shareOverLan"] = false;
            jobj["isDefault"] = false;
            jobj["localPort"] = 1080;
            jobj["pacUrl"] = null;
            jobj["useOnlinePac"] = false;
            jobj["availabilityStatistics"] = false;


            JArray jconfig = new JArray();
            jobj.Add("config", jconfig);
            foreach (SS_Server_Model model in SS_Server_Model_Pipeline.SrvrResult)
            {
                JObject server = new JObject();

                server["server"] = model.SrvAddr;
                server["server_port"] = model.SrvPort;
                server["password"] = model.SrvPass;
                server["method"] = model.SrvMethod;
                server["remarks"] = model.SrvRemark;

                jconfig.Add(server);
            }

            System.IO.File.WriteAllText(path,jobj.ToString());
        }

        public static void SS_Load_Server_Setting_Json(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Console.WriteLine("输入配置文件不存在或为空，请处理");
                Console.ReadKey();
            }
            else
            {
                string ServerSetting = System.IO.File.ReadAllText(path);

                JObject jobj = JObject.Parse(ServerSetting);
                JArray arr = (JArray)jobj["SrvList"];

                foreach (JObject item in arr)
                {
                    SrvModels.Add(new SS_Server_Setting_Model()
                    {
                        SrvAddr = item["SrvAddr"].ToString(),
                        RegexPattern = item["RegexPattern"].ToString(),
                    });
                }
            }
        }
    }

    /// <summary>
    /// 用于存放处理过后的SS服务器结果
    /// </summary>
    public class SS_Server_Model_Pipeline : BasePipeline
    {
        static public List<SS_Server_Model> SrvrResult = new List<SS_Server_Model>();

        public override void Process(ResultItems resultItems)
        {
            foreach (SS_Server_Model item in resultItems.GetResultItem("results"))
            {
                if (!string.IsNullOrWhiteSpace(item.SrvAddr) && !string.IsNullOrWhiteSpace(item.SrvPort) && !string.IsNullOrWhiteSpace(item.SrvPass))
                {
                    Console.WriteLine("SrvName:{0} , SrvPort:{1} , SrvPass{2}", item.SrvAddr, item.SrvPort, item.SrvPass);
                    SrvrResult.Add(item);
                }
            }
        }
    }

    /// <summary>
    /// 正则表达式网页处理器
    /// </summary>
    public class SS_Server_Regex_Processor : BasePageProcessor
    {
        public string RegexPattern { get; set; }

        public SS_Server_Regex_Processor(string regexString)
        {
            RegexPattern = regexString;
        }

        protected override void Handle(Page page)
        {
            try
            {
                RegexOptions options = RegexOptions.Singleline;
                MatchCollection regexMatches = Regex.Matches(page.Content, RegexPattern, options);
                Console.WriteLine(regexMatches.Count.ToString());


                List<SS_Server_Model> results = new List<SS_Server_Model>();
                
                foreach (Match match in regexMatches)
                {
                    results.Add(new SS_Server_Model()
                    {
                        SrvAddr = match.Groups["SrvAddr"].Value,
                        SrvPort = match.Groups["SrvPort"].Value,
                        SrvPass = match.Groups["SrvPass"].Value,
                        SrvMethod = match.Groups["SrvMethod"].Value,
                        SrvRemark = match.Groups["SrvRemark"].Value,
                    });
                }

                page.AddResultItem("results", results);


                //后续采集网页
                //page.AddTargetRequest(new Request("",null));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }

    /// <summary>
    /// ss服务器节点
    /// </summary>
    public class SS_Server_Model
    {
        public string SrvAddr { get; set; }
        public string SrvPort { get; set; }
        public string SrvPass { get; set; }
        public string SrvMethod { get; set; }
        public string SrvRemark { get; set; }
    }

    public class SS_Server_Setting_Model
    {
        public string SrvAddr { get; set; }
        public string RegexPattern { get; set; }
    }
}
