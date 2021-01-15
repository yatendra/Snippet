using System;

namespace Snippet
{
    public class SnippetResponse
    {
        public string url { get; set; }
        
        public string name { get; set; }

        public DateTime expires_at { get; set; }

        public string snippet { get; set; }
    }
}
