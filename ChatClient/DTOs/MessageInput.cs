using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace ChatClient.DTOs
{
    [MessagePackObject]
    public class MessageInputDto
    {
        [Key(0)]
        public string Text { get; init; } = string.Empty;
    }
}
