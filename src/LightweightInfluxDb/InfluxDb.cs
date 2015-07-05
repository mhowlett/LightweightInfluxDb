using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace LightweightInfluxDb
{
    public class InfluxDb
    {
        private string _url;
        private string _credentials;

        public InfluxDb(string url, string db, string user, string password)
        {
            _url = url;
            if (!_url.EndsWith("/")) { _url = _url + "/"; }
            _credentials = "?db=" + db + "&u=" + user + "&p=" + password;
        }

        private static string SerializeWriteData(ISeriesPoint data)
        {
            if (data.Fields.Count != data.Values.Count)
            {
                throw new Exception("Invalid field series point - number of fields does not match number of values.");
            }
            var result = data.Name;
            if (data.Tags.Count > 0)
            {
                foreach (var kvp in data.Tags)
                {
                    result += "," + kvp.Key + "=" + kvp.Value;
                }
            }
            result += " ";
            for (int i = 0; i < data.Fields.Count; ++i)
            {
                if (i != 0) { result += ","; }
                string strRep;
                if (data.Values[i].GetType() == typeof(double))
                {
                    strRep = ((double)data.Values[i]).ToString(".0##############");
                }
                else if (data.Values[i].GetType() == typeof(float))
                {
                    strRep = ((float)data.Values[i]).ToString(".0######");
                }
                else
                {
                    strRep = "" + data.Values[i];
                }
                result += data.Fields[i] + "=" + strRep;
            }
            return result;
        }

        public async Task WriteAsync(List<ISeriesPoint> data)
        {
            string s = "";
            foreach (var d in data)
            {
                s += SerializeWriteData(d) + "\n";
            }
            await WriteAsync(s);
        }
        
        public async Task WriteAsync(ISeriesPoint data)
        {
            await WriteAsync(SerializeWriteData(data));
        }

        private async Task WriteAsync(string data)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Accept.Clear();
                HttpResponseMessage response = await client.PostAsync("write" + _credentials, new StringContent(data));
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("unable to write series point data.");
                }
            }
        }

        public class QuerySeries
        {
            public string Name;
            public List<string> Columns;
            public List<List<object>> Values;
        }

        public async Task<List<List<object>>> QuerySingleSeriesAsync(string query)
        {
            var queryString = Uri.EscapeDataString(query);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var url = "query" + _credentials + "&q=" + queryString;
                HttpResponseMessage response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("unable to read series point data.");
                }
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var sr = new StreamReader(stream))
                {
                    var result = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, List<QuerySeries>>>>>(sr.ReadToEnd());
                    return result["results"][0]["series"][0].Values;
                }
            }
        }

    }

}