using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VzaarApi;

namespace VzaarConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                Client.client_id = "fluff-idea-vale";
                Client.auth_token = "UGuApKT_bWUiUsyrx5ks";
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                Client.url = "https://api.vzaar.com/api/";
                //Client.version = "v2";
                //Client.urlAuth = true;
                //Client client = new Client() { CfgClientId = "fluff-idea-vale", CfgAuthToken = "UGuApKT_bWUiUsyrx5ks" };
                Dictionary<string, string> query = new Dictionary<string, string>() {
                    {"sort","title"},
                    {"order", "asc"},
                    {"per_page", "100"},
                    {"q", "title:room" }
                };

                //Client.url = Client.url + "?q=title:4th";

                //foreach (Video item in VideosList.EachItem(query, client))
                //{
                //    Console.WriteLine("Id: " + item["id"] + " Title: " + item["title"]);
                //}

                foreach (var item in VideosList.EachItem(query))
                {
                    Console.WriteLine("Id: " + item["id"] + " Title: " + item["title"]);
                }

                //var api = new VzaarApi(secret, token);
                //var query = new VideoListQuery();
                //query.count = 100;
                //query.page = 1;
                //query.sort = VideoListSorting.ASCENDING;
                //query.title = searchString;
                //var list = api.getVideoList(query);
                //return Mapper.Map<List<OutputDto_DropdownData_VzaarVideo>>(list);
            }
            catch (VzaarApiException ve)
            {

                Console.Write("!!!!!!!!! EXCEPTION !!!!!!!!!");
                Console.WriteLine(ve.Message);

            }
            catch (Exception e)
            {

                Console.Write("!!!!!!!!! EXCEPTION !!!!!!!!!");
                Console.WriteLine(e.Message);

                if (e is AggregateException)
                {
                    AggregateException ae = (AggregateException)e;

                    var flatten = ae.Flatten();

                    foreach (var fe in flatten.InnerExceptions)
                    {
                        if (fe is VzaarApiException)
                            Console.WriteLine(fe.Message);
                    }
                }

            }
        }
    }
}
