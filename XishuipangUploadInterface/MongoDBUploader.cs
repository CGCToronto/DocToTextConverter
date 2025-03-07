using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XishuipangUploadInterface
{
    class MongoDBUploader
    {
        // Pass in the logger to print information

        public void Upload(string folderPath)
        {
            // Check if folder exists
            if (!Directory.Exists(folderPath))
            {
                // log error
                Logger.Instance.WriteLine("MongoDBUploader: ERROR: Directory not exist.");

                return;
            }

            // Establish connection to MongoDB
            var client = new MongoClient("mongodb+srv://sx2gavin:glXishuipang@xishuipang-db.qo1sq.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
            var database = client.GetDatabase("Xishuipang");


            // Cache table of content collection and article collection
            IMongoCollection<BsonDocument> tableOfContentCollection = database.GetCollection<BsonDocument>("TableOfContents");
            IMongoCollection<BsonDocument> articleCollection = database.GetCollection<BsonDocument>("Articles");

            // Loop through all JSON files within this folder
            string[] files = Directory.GetFiles(folderPath);
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                string fileExtension = Path.GetExtension(filePath).ToLower();

                if (fileExtension != ".json")
                {
                    continue;
                }

                // if the file is a table of content file
                if (fileName == "table_of_content_s.json" || fileName == "table_of_content_t.json")
                {
                    // Upload file to table of content collection
                    AddJsonFileToCollection(filePath, tableOfContentCollection, true);
                }
                else
                {
                    // upload to article collection
                    AddJsonFileToCollection(filePath, articleCollection);
                }
            }

        }

        // Convert JSON file to BSONDocument
        private async void AddJsonFileToCollection(string filePath, IMongoCollection<BsonDocument> collection, bool isTOC = false)
        {
            // Check if file exists
            if (!File.Exists(filePath))
            {
                return;
            }

            if (Path.GetExtension(filePath).ToLower() != ".json")
            {
                return;
            }

            // Check if collection is null
            if (collection == null)
            {
                return;
            }

            // Use StreamReader to read file content
            using (StreamReader sr = new StreamReader(filePath))
            {
                string content = sr.ReadToEnd();
                using (var jsonReader = new JsonReader(content))
                {
                    var context = BsonDeserializationContext.CreateRoot(jsonReader);
                    var document = collection.DocumentSerializer.Deserialize(context);

                    FilterDefinition<BsonDocument> filter;

                    // If it's not table of content, use id and volume
                    if (!isTOC)
                    {
                        // get volume number and article id from document
                        string articleId = document.GetValue("id").AsString;
                        string volumeNum = document.GetValue("volume").AsString;

                        // create filter with volume number and article id
                        var builder = Builders<BsonDocument>.Filter;
                        filter = builder.Eq("id", articleId) & builder.Eq("volume", volumeNum);
                    }
                    else
                    {
                        // else, use volume and character version
                        string character = document.GetValue("character").AsString;
                        string volumeNum = document.GetValue("volume").AsString;

                        // create filter with volume number and article id
                        var builder = Builders<BsonDocument>.Filter;
                        filter = builder.Eq("character", character) & builder.Eq("volume", volumeNum);
                    }

                    // update the document within the collection with the document.
                    var options = new ReplaceOptions { IsUpsert = true };
                    var result =  await collection.ReplaceOneAsync(filter, document, options);

                    // log result to logger
                    if (result.IsAcknowledged)
                    {
                        Logger.Instance.WriteLine("MongoDBUploader: " + Path.GetFileName(filePath) + " pushed to MongoDB successfully.");
                    }
                    else
                    {
                        Logger.Instance.WriteLine("MongoDBUploader: ERROR: " + Path.GetFileName(filePath) + " failed to push to MongoDB.");
                    }
                }
            }

        }

    }
}
