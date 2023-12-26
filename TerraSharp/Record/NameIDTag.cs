using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Record;

namespace TerraSharp.Record
{
    public class NameIDTag
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string hash { get; private set; }
        public object Reference { get; set; }

        private NameIDTag(string name, string description, object reference)
        {
            Name = name;
            Description = description;
            this.hash = Hash.GenerateMD5Hash(name);
            Reference = reference;
        }

        public static NameIDTag Register(string name, string description, object reference)
        {
            return new NameIDTag(name, description, reference);
        }

        public static NameIDTag Register(string name, object reference)
        {
            return new NameIDTag(name, "<any>", reference);
        }

        public static NameIDTag Register(object reference)
        {
            return new NameIDTag("<any>", "<any>", reference);
        }

        public static bool IsMatch(NameIDTag tag, NameIDTag other)
        {
            return tag != null && other != null &&
                   (tag.Name == other.Name || tag.Reference == other.Reference);
        }
    }
}