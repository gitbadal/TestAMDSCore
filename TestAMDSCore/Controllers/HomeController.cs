using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using TestAMDSCore.Models;

namespace TestAMDSCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public TagValueResponse[] GetPLCData([FromBody]RquestObj req)
        {
            TagValueResponse[] resp = null;
            try
            {
                //DateTime chrtdate_from, chrtdate_to;
                //GraphTypeDC[] objOutPutDataDCarray = null;
                //chrtdate_from = Convert.ToDateTime(FromDate);
                //chrtdate_to = Convert.ToDateTime(ToDate);
                //objOutPutDataDCarray = WcfService.objHMI2.STUB_GetAIEQuipFlag(chrtdate_from, chrtdate_to, EqpNO, Scenarios);
                //return objOutPutDataDCarray;
                string projectId = "projamdstrial";
                DateTime chrtdate_from, chrtdate_to;

                chrtdate_from = Convert.ToDateTime(req.FromDate);
                chrtdate_to = Convert.ToDateTime(req.ToDate);

                // Instantiates a client.
                using (StorageClient storageClient = StorageClient.Create())
                {
                    // The name for the new bucket.
                    string bucketName = "TestAMDSData/BFProcess_FBF_DATA_HEARTH_2020_1_28_20200128073415.csv";
                    try
                    {
                        // Creates the new bucket.
                        var bucket = storageClient.GetBucket("amds_bucket");
                        using (MemoryStream mem = new MemoryStream())
                        {
                            storageClient.DownloadObject("amds_bucket", bucketName, mem);
                            //StreamReader reader = new StreamReader(mem);
                            //string content = reader.ReadToEnd();
                            string content = Encoding.ASCII.GetString(mem.ToArray());
                            string[] sep = { "\n" };
                            List<string> lines = content.Split(sep, StringSplitOptions.RemoveEmptyEntries).ToList();
                            lines.RemoveAt(0);
                            List<TagValueResponse> dataList = new List<TagValueResponse>();
                            lines.ForEach(x => dataList.Add(new TagValueResponse()
                            {
                                READTIME = Convert.ToDateTime(x.Split(',')[0]),
                                TAGID = Convert.ToInt32(x.Split(',')[1]),
                                VALUE = Convert.ToDecimal(x.Split(',')[2])
                            }));
                            resp = dataList.Where(x => x.READTIME >= chrtdate_from && x.READTIME <= chrtdate_to && x.TAGID == req.TAGID).ToArray();

                        }
                        Console.WriteLine($"Bucket {bucketName} created.");
                    }
                    catch (Google.GoogleApiException e)
                    when (e.Error.Code == 409)
                    {
                        // The bucket already exists.  That's fine.
                        Console.WriteLine(e.Error.Message);
                    }
                }
                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }

    public class RquestObj
    {
        public DateTime FromDate { get; set; }
        public int TAGID { get; set; }
        public DateTime ToDate{ get; set; }
    }

    public class TagValueResponse
    {

        public DateTime READTIME { get; set; }
        public int TAGID { get; set; }
        public decimal? VALUE { get; set; }

    }

}
