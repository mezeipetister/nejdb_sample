using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using fastJSON;
using Ejdb.DB;
using Ejdb.BSON;

namespace EJDBApplication
{
    class Program
    {
        static void Main()
        {

            var httpWReq = (HttpWebRequest) WebRequest.Create("http://www.slamby.com/test/json/get/basic.json");
            var response = (HttpWebResponse) httpWReq.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var jsonContent = new StreamReader(stream: response.GetResponseStream()).ReadToEnd();

                #region WithJSON2BSON

                var dbtest = new EJDB("dbb") { ThrowExceptionOnFail = true };
                dbtest.Save("doctest", dbtest.Json2Bson(jsonContent));
                var queryTest = dbtest.CreateQuery(new
                {
                    sex = "male"
                }, "doctest");
                using (var cursor = queryTest.Find())
                {
                    Console.WriteLine("Found" + cursor.Length + "names");
                }
                #endregion
                
                
                
                var dictionary = JSON.Instance.Parse(jsonContent) as Dictionary<string, object>;
                if (dictionary != null)
                {
                    var dict = new Dictionary<string, object>(dictionary);

                    var db = new EJDB("db") {ThrowExceptionOnFail = true};
                    var bsonDocument = new BSONDocument();
                    foreach (var keyValuePair in dict)
                    {
                        if (keyValuePair.Value.GetType() != dict["dogs"].GetType())
                        {
                            bsonDocument.SetString(keyValuePair.Key, keyValuePair.Value.ToString());
                        }
                        else
                        {
                            if (dict["dogs"] as IEnumerable == null) continue;
                            var dogsIndex = 0;
                            var dogsBsonArray = new BSONArray();
                            foreach (var dog in dict["dogs"] as IEnumerable)
                            {
                                var dogsAttributes = JSON.Instance.Parse(dog as string) as Dictionary<string, string>;
                                if (dogsAttributes != null)
                                {
                                    var dogBsonValue = new BSONDocument();
                                    foreach (var dogAttribute in dogsAttributes)
                                    {
                                        dogBsonValue.SetString(dogAttribute.Key, dogAttribute.Value);
                                    }
                                    dogsBsonArray.SetObject(dogsIndex, dogBsonValue);
                                    dogsIndex++;
                                }
                            }
                            bsonDocument.SetArray("dogs", dogsBsonArray);
                        }
                    }
                
                    db.Save("doc", bsonDocument);

                    var query = db.CreateQuery(new {
                        sex = "male"
                        },"doc");
                    /*using (var cursor = query.Find())
                    {
                        Console.WriteLine("Found" + cursor.Length + "names");
                    }*/

                    Console.ReadKey();
                    query.Dispose();
                    db.Dispose();
                }
            }
        }
    }
}