using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Ejdb.BSON;
using Ejdb.DB;

namespace Slamby
{
    public class Database
    {
        public EJDB _ejdbDatabase { get; set; }

        public Database(string connectionInfo)
        {
            _ejdbDatabase = new EJDB(connectionInfo);
        }

        public Database(string connectionInfo, int openMode)
        {
            _ejdbDatabase = new EJDB(connectionInfo, openMode);
        }

        public bool GetContentFromWeb(string url)
        {
            var httpWReq = (HttpWebRequest)WebRequest.Create(url);
            var response = (HttpWebResponse)httpWReq.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var encoding = new ASCIIEncoding();
                var content = encoding.GetBytes(new StreamReader(response.GetResponseStream()).ReadToEnd());
                var doc = new BSONDocument(content);
                return _ejdbDatabase.Save("doc", doc);
            }
            return false;
        }

        /*public string Select()
        {
            var query = _ejdbDatabase.CreateQuery();
            query.Find()
        }*/
    }
}
