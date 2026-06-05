namespace POS36.Api.DTOs
{
    public class SampleDataRequest
    {
        public string ModelType { get; set; } = "cafe"; // cafe, restaurant, pub, fastfood
        public bool HasTables { get; set; } = true;
        public int TableCount { get; set; } = 6;
        
        public bool ImportProducts { get; set; } = true;
        public int ProductCount { get; set; } = 10;
        
        public bool ImportTables { get; set; } = true;
        
        public bool ImportTransactions { get; set; } = true;
        public int TransactionCount { get; set; } = 10;
    }
}
