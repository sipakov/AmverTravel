using System.Collections.Generic;
using Amver.Domain.Dto;

namespace Amver.Domain.Models
{
    public class ConversationResponse
    {
        public IEnumerable<ConversationForListDto> Conversations { get; set; }
        public int Count { get; set; }
    }
}