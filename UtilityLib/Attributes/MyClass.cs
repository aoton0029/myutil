using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.Attributes
{
    public class Person(string firstName, string lastName)
    {
        public string FirstName => firstName;
        public string LastName => lastName;

        [Experimental("THOMAS001")]
        public string FullName => $"{FirstName} {LastName}";

    }
    
    public class ext
    {
        public void TExt()
        {
            var person = new Person("Thomas", "Huber");

#pragma warning disable THOMAS001
            Console.WriteLine(person.FullName);
#pragma warning restore THOMAS001

            /*
             <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net9.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
                <NoWarn>THOMAS001</NoWarn>
            </PropertyGroup>    
            <NoWarn>THOMAS001,THOMAS002,THOMAS003</NoWarn>
             */
        }
    }
}
