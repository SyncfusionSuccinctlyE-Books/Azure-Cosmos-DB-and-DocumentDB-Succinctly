using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocDbClientCode
{
    public sealed class CR7DocType
    {
        public string id { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public string Birthplace { get; set; }
    }

    public sealed class Tags
    {
        public string name { get; set; }
    }

    public sealed class Servings
    {
        public int amount { get; set; }
        public string description { get; set; }
        public int weightInGrams { get; set; }
    }

    public sealed class FoodDocType
    {
        public string id { get; set; }
        public string description { get; set; }
        public int version { get; set; }
        public bool isFromSurvey { get; set; }
        public string foodGroup { get; set; }
        public Tags[] tags { get; set; }
        public Servings[] servings { get; set; }
    }

    class Program
    {
        public const string cStrEndPoint = "<< DocumentDB Endpoint >>";
        public const string cStrKey = "<< DocumentDB Primary Key >>";

        static void Main(string[] args)
        {
            //CreateDoc("ToDoList", "Items");
            //TestTrigger("ToDoList", "Items");
            TestUdf("ToDoList", "Items");

            //ListDbs();
            Console.ReadLine();
        }

        public static void TestUdf(string dbName, string collName)
        {
            using (var client = new DocumentClient(new Uri(cStrEndPoint), cStrKey))
            {
                string query = "SELECT c.id FROM c WHERE udf.udfCheckRegEx(c.id, 'Messi') != null";
                string url = "dbs" + "/" + dbName + "/colls/" + collName;

                Console.WriteLine("Querying for Messi documents");
                var docs = client.CreateDocumentQuery(url, query).ToList();

                Console.WriteLine("{0} docs found", docs.Count);
                foreach (var d in docs)
                {
                    Console.WriteLine("{0}", d.id);
                }
            }
        }

        public static async void TestTrigger(string dbName, string collName)
        {
            await Task.Run(
            async () =>
            {
                using (var client = new DocumentClient(new Uri(cStrEndPoint), cStrKey))
                {
                    FoodDocType fd = new FoodDocType { id = "TestFoodDoc", description = "Organic food", isFromSurvey = false, foodGroup = "Organic", version = 1 };
                    string url = "dbs" + "/" + dbName + "/colls/" + collName;

                    try
                    {
                        Document issue = await client.CreateDocumentAsync(url, fd, new RequestOptions { PreTriggerInclude = new[] { "preTrigValidateIdExists" } });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception: "  + ex.ToString());
                    }
                }
            });
        }

        public static async void CreateDoc(string dbName, string collName)
        {
            await Task.Run(
            async () =>
            {
                using (var client = new DocumentClient(new Uri(cStrEndPoint), cStrKey))
                {
                    FoodDocType fd = new FoodDocType { id = "TestFoodDoc", description = "Organic food", isFromSurvey = false, foodGroup = "Organic", version = 1 };
                    Document issue = await CreateDocType(fd, client, dbName, collName);
                }
            });
        }

        public static async Task<Document> CreateDocType(FoodDocType fd, DocumentClient client, string dbName, string collName)
        {
            if (client != null)
            {
                string url = "dbs" + "/" + dbName + "/colls/" + collName;
                Document id = await client.CreateDocumentAsync(url, fd);

                return (id != null) ? client.CreateDocumentQuery(url).
                    Where(d => d.Id == id.Id).AsEnumerable().FirstOrDefault() : null;
            }
            else
                return null;
        }

        public static void ListDbs()
        {
            using (var client = new DocumentClient(new Uri(cStrEndPoint), cStrKey))
            {
                var dbs = client.CreateDatabaseQuery();
                foreach (var db in dbs)
                {
                    Console.WriteLine("Database Id: {0}; Rid {1}", db.Id, db.ResourceId);
                    ListCollections(client, db, db.Id);
                }
            }
        }

        public static void ListCollections(DocumentClient client, Database db, string dbname)
        {
            if (client != null && db != null)
            {
                List<DocumentCollection> collections = client.CreateDocumentCollectionQuery(db.SelfLink).ToList();

                Console.WriteLine("{0} collections for database: {1}", collections.Count.ToString(), dbname);
                foreach (DocumentCollection col in collections)
                {
                    Console.WriteLine("Collection Id: {0}; Rid {1}", col.Id, col.ResourceId);
                    //ListDocuments(client, dbname, col.Id);
                    //ListCR7DocType(new CR7DocType { Name = "Ronaldo", LastName = "Aveiro"}, client, dbname, col.Id);
                    //ListFoodDocType(new FoodDocType { description = "Snacks" }, client, dbname, col.Id);
                }
            }
        }

        public static void ListDocuments(DocumentClient client, string dbName, string collName)
        {
            if (client != null)
            {
                IEnumerable<Document> docs =
                    from c in client.CreateDocumentQuery("dbs" + "/" + dbName + "/colls/" + collName)
                    select c;

                if (docs != null)
                {
                    Console.WriteLine("Documents for collection {0}", collName);
                    foreach (var doc in docs)
                    {
                        Console.WriteLine("Document Id: {0}; Rid {1} ", doc.Id, doc.ResourceId);
                    }
                }
            }
        }

        public static void ListFoodDocType(FoodDocType fd, DocumentClient client, string dbName, string collName)
        {
            if (client != null)
            {
                IEnumerable<FoodDocType> docs =
                    from c in client.CreateDocumentQuery<FoodDocType>("dbs" + "/" + dbName + "/colls/" + collName)
                    where c.description.ToUpper().
                        Contains(fd.description.ToUpper())
                    select c;

                if (docs != null)
                {
                    foreach (var doc in docs)
                    {
                        Console.WriteLine("id: {0}", doc.id);
                        Console.WriteLine("description: {0}", doc.description);
                        Console.WriteLine("Version: {0}", doc.version);
                        Console.WriteLine("isFromSurvey: {0}", doc.isFromSurvey.ToString());
                        Console.WriteLine("foodGroup: {0}", doc.foodGroup);
                    }
                }
            }
        }

        public static void ListCR7DocType(CR7DocType cr7, DocumentClient client, string dbName, string collName)
        {
            if (client != null)
            {
                IEnumerable<CR7DocType> docs =
                    from c in client.CreateDocumentQuery<CR7DocType>("dbs" + "/" + dbName + "/colls/" + collName)
                    where c.Name.ToUpper().
                        Contains(cr7.Name.ToUpper())
                    where c.LastName.ToUpper().
                        Contains(cr7.LastName.ToUpper())
                    select c;

                if (docs != null)
                {
                    foreach (var doc in docs)
                    {
                        Console.WriteLine("id: {0}", doc.id);
                        Console.WriteLine("Name: {0}", doc.Name);
                        Console.WriteLine("LastName: {0}", doc.LastName);
                        Console.WriteLine("Nationality: {0}", doc.Nationality);
                        Console.WriteLine("Birthplace: {0}", doc.Birthplace);
                    }
                }
            }
        }
    }
}
