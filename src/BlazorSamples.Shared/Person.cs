using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSamples.Shared
{
    public sealed class Person
    {
        public int Id { get; set; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required int Age { get; init; }
    }
}
