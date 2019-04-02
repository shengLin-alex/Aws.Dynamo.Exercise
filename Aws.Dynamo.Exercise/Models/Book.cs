using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace Aws.Dynamo.Exercise.Models
{
    [DynamoDBTable("Book")]
    public class Book
    {
        [DynamoDBHashKey]
        public int Id { get; set; }
        
        [DynamoDBProperty]
        public string Title { get; set; }
        
        [DynamoDBProperty]
        public string ISBN { get; set; }
        
        [DynamoDBProperty]
        public List<string> BookAuthors { get; set; }
    }
}