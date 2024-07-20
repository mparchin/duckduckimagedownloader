namespace mparchin.Client
{
    public class ResponseWrapper
    {
        public int StatusCode { get; set; }
        public TimeSpan Time { get; set; }
        public string? Error { get; set; }
        public virtual bool HasError => !string.IsNullOrEmpty(Error);
    }

    public class ResponseWrapper<TEntity> : ResponseWrapper where TEntity : class
    {
        public TEntity? Response { get; set; }
        public override bool HasError => base.HasError || Response is null;
    }
}