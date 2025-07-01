using System;

namespace RemindMe.Models
{
    public class TodoTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public bool Flagged { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public DateTime? AlarmAt { get; set; }
    }
}
