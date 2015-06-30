using System;
using System.Net;
using System.Collections.Generic;
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

        public void Write(List<ISeriesPoint> data)
        {
            string s = "";
            foreach (var d in data)
            {
                s += SerializeWriteData(d) + "\n";
            }
            Write(s);
        }

        public void Write(ISeriesPoint data)
        {
            Write(SerializeWriteData(data));
        }

        private void Write(string data)
        {
            var url = _url + "write" + _credentials;
            var req = WebRequest.Create(url);

            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";
            var bytes = System.Text.Encoding.ASCII.GetBytes(data);
            req.ContentLength = bytes.Length;
            var os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length);
            os.Close();
            var response = req.GetResponse();
            if (response == null)
            {
                throw new Exception("unable to write series point data.");
            }
        }

        public class QuerySeries
        {
            public string Name;
            public List<string> Columns;
            public List<List<object>> Values;
        }

        public List<List<object>> QuerySingleSeries(string query)
        {
            var queryString = System.Uri.EscapeDataString(query);
            var url = _url + "query" + _credentials + "&q=" + queryString;

            var req = WebRequest.Create(url);
            var res = req.GetResponse();
            using (var sr = new StreamReader(res.GetResponseStream()))
            {
                var result = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, List<QuerySeries>>>>>(sr.ReadToEnd());
                return result["results"][0]["series"][0].Values;
            }
        }

    }

}