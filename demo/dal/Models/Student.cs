using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace dal.Models
{
    public class Student
    {
        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstMidName { get; set; }
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime EnrollmentDate { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
    }
}
