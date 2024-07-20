namespace imageDownloader
{
    public static class ENV
    {
        public static readonly Lazy<string> DbPath = new(() => Environment.GetEnvironmentVariable("DB_PATH") ?? "data/");
        public static readonly Lazy<int> ConcurrentRequests = new(() => Convert.ToInt32(Environment.GetEnvironmentVariable("CONCURRENT_REQUESTS") ?? "2"));


        public static readonly Lazy<string> ClientId = new(() => Environment.GetEnvironmentVariable("CLIENT_ID") ?? "9c70bd50-3a60-4522-9d8b-19c0352ce8b1");
        public static readonly Lazy<string> ClientSecret = new(() => Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "gQ4xYozlhqcbYOC3WpAPKI9OyD88KJMRrh4TrvfW");


        public static readonly Lazy<int> MockUsers = new(() => Convert.ToInt32(Environment.GetEnvironmentVariable("MOCK_USERS") ?? "4"));


        public static readonly Lazy<Uri> CDNURL = new(() => new(Environment.GetEnvironmentVariable("CDN_URL") ?? "https://media.idoonya.com/"));
        public static readonly Lazy<Uri> AuthorityURL = new(() => new(Environment.GetEnvironmentVariable("AUTHORITY_URL") ?? "https://api.idoonya.com/v1/authority/"));
        public static readonly Lazy<Uri> MediaURL = new(() => new(Environment.GetEnvironmentVariable("MEDIA_URL") ?? "https://api.idoonya.com/v1/media/"));
        public static readonly Lazy<Uri> ProfileURL = new(() => new(Environment.GetEnvironmentVariable("PROFILE_URL") ?? "https://api.idoonya.com/v1/profile/"));
        public static readonly Lazy<Uri> FollowURL = new(() => new(Environment.GetEnvironmentVariable("FOLLOW_URL") ?? "https://api.idoonya.com/v1/follow/"));
        public static readonly Lazy<Uri> LineURL = new(() => new(Environment.GetEnvironmentVariable("LINE_URL") ?? "https://api.idoonya.com/v1/line/"));
        public static readonly Lazy<Uri> PostURL = new(() => new(Environment.GetEnvironmentVariable("POST_URL") ?? "https://api.idoonya.com/v1/post/"));
        public static readonly Lazy<Uri> HomeURL = new(() => new(Environment.GetEnvironmentVariable("HOME_URL") ?? "https://api.idoonya.com/v1/gateway/"));
        public static readonly Lazy<Uri> PostCommentURL = new(() => new(Environment.GetEnvironmentVariable("POST_COMMENT_URL") ?? "https://api.idoonya.com/v1/post-comment/"));
        public static readonly Lazy<Uri> CommentLikeURL = new(() => new(Environment.GetEnvironmentVariable("COMMENT_LIKE_URL") ?? "https://api.idoonya.com/v1/comment-like/"));
        public static readonly Lazy<Uri> PostLikeURL = new(() => new(Environment.GetEnvironmentVariable("POST_LIKE_URL") ?? "https://api.idoonya.com/v1/post-like/"));
    }
}