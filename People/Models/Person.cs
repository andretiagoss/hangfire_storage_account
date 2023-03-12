using System;
using System.ComponentModel;

namespace Hangfire.API.People.Models
{
    public class Person
    {
        public Person(string code, string nome, string email)
        {
            Id = Guid.NewGuid();
            Code = code;
            Name = nome;
            Email = email;
        }
        
        public Guid Id { get; init; }
        [Description("Codigo")]
        public string Code { get; set; }
        [Description("Nome")]
        public string Name { get; init; }
        [Description("E-mail")]
        public string Email { get; init; }
    }
}
