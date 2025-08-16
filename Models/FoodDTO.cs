using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Exam.Models
{
    internal class FoodDTO
    {
        [JsonPropertyName("name")]
        public string? Msg { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
       
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
