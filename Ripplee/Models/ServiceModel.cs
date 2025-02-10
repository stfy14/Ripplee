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
        public required string Gender { get; set; }
        public required string City { get; set; }
        public required string Topic { get; set; }
        public required string Chat {  get; set; }
        public required int Age { get; set; }
    }
    //
    public class CompanionResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public CompanionData Data { get; set; } //втф че? зачем? 
    }

    public class CompanionData
    {
        public required string CompanionName { get; set; }
        public required string CompanionGender { get; set; }
        public required string CompanionCity { get; set; }
        public required string CompanionTopic { get; set; }
        public required string CompanionChat { get; set; }
    }
}