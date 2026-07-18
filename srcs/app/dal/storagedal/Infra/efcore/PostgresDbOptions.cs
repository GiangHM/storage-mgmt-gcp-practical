namespace storagedal.Infra.efcore
{
    public class PostgresDbOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public bool EnableDetailedErrors { get; set; }
    }
}
