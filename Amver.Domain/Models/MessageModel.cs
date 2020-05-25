namespace Amver.Domain.Models
{
    public class MessageModel
    {
        public int UserId { get; set; }
        public string Message { get; set; }
        public bool IsOwnMessage { get; set; }
        public bool IsSystemMessage { get; set; }
    }
}