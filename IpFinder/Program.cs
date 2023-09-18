using System.Net;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;

namespace IpFinder
{
    public class IpData
    {
        public string ip, sign,address;
    }
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Console.Title = "IpFinder";
            Console.WriteLine("Write your domain name here:");
            string domain = Console.ReadLine();
            var local = await Dns.GetHostAddressesAsync(domain);
            foreach (var item in local)
                Console.WriteLine("Local DNS record:" + item);
            Console.WriteLine();
            Console.WriteLine("FINDING GLOBAL ADDRESSES....");
            var list = await GetIpAddressByDomain(domain);
            foreach (var item in list)
            {
                Console.Write(item.ip + "  ");
                var add = await FindIpAddress(item);
                item.address = add;
                Console.WriteLine(add);
            }
            Console.WriteLine("Enter to start to ping..");
            Console.ReadLine();
            Ping ping = new();
            var AvaliableList = new List<IpData>();
            foreach (var item in list) {
                if (item.address.Contains("Facebook") || item.address.Contains("Twitter")) continue;
                Console.WriteLine("Trying to connect " + item.ip + item.address);
                var reply = ping.Send(item.ip);
                if (reply.Status == IPStatus.Success)
                {
                    AvaliableList.Add(item);
                    Console.WriteLine("Successfully connected!");
                }
                else Console.WriteLine("Failed");
            }
            if (AvaliableList.Count > 0)
            {
                Console.WriteLine(AvaliableList.Count + " of " + list.Count + " is avaliable");
                Console.WriteLine("Enter to write them to hosts");
                Console.ReadLine();
                Console.WriteLine("Appending...");
                var str = "";
                foreach (var item in AvaliableList)
                    str += "\r\n"+item.ip + " " + domain;
                Console.WriteLine(str);
                await File.AppendAllTextAsync("C:\\Windows\\System32\\drivers\\etc\\hosts",str);
                Console.WriteLine("Done.");
                Console.ReadLine();
            }
            else Console.WriteLine("None is avaliable...");
        }
        static async Task<List<IpData>> GetIpAddressByDomain(string domain)
        {
            using var hp = new HttpClient(new SocketsHttpHandler()
            {
                KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                AutomaticDecompression = DecompressionMethods.GZip
            });
            hp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            hp.DefaultRequestHeaders.Host = "site.ip138.com";
            hp.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36 Edg/117.0.0.0");
            string data= await hp.GetStringAsync("https://site.ip138.com/domain/read.do?domain=" + domain);
            var obj=JObject.Parse(data)["data"];
            var list = new List<IpData>();
            foreach (var item in obj) {
                list.Add(new IpData()
                {
                    ip = item["ip"].ToString(),
                    sign = item["sign"].ToString()
                });
            }
            return list;
        }
        static async Task<string> FindIpAddress(IpData d) {
            using var hp = new HttpClient(new SocketsHttpHandler()
            {
                KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                AutomaticDecompression = DecompressionMethods.GZip
            });
            hp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            hp.DefaultRequestHeaders.Host = "api.ip138.com";
            hp.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36 Edg/117.0.0.0");
            string data = await hp.GetStringAsync($"https://api.ip138.com/query/?ip={d.ip}&oid=5&mid=5&from=siteFront&datatype=json&sign="+d.sign);
            string address = "";
            var obj = JObject.Parse(data)["data"];
            foreach (var item in obj)
                address += item + " ";
            return address;
        }
    }
}