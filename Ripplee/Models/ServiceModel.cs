using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//выбоор городов обьяв 
//тут ещё обьвим что нибудь пока хз 
namespace Ripplee.Models
{
    public class CompanionRequest
    {
        public string Gender { get; set; }
        public string City { get; set; }
        public string Topic { get; set; }
        public int Age { get; set; }
    }

    public class CompanionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public CompanionData Data { get; set; }
    }

    public class CompanionData
    {
        public string CompanionName { get; set; }
        public string CompanionGender { get; set; }
        public string CompanionCity { get; set; }
        public string CompanionTopic { get; set; }
    }
}