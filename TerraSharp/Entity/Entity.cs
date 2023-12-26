using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerraSharp.Record;

namespace TerraSharp.Entity
{
    public struct EntityRegistration
    {
        public string uuid;
        public EntityType type;
        public string name;
        //public AABB boundingBox;

        public static EntityRegistration Register(String name, EntityType type)//, AABB boundingBox)
        {
            EntityRegistration registration = new EntityRegistration();

            registration.name = name;
            registration.type = type;
            if (name == "")
                registration.uuid = Hash.GenerateMD5Hash(MathNet.Numerics.Random.RandomSeed.Robust().ToString());
            else
                registration.uuid = Hash.GenerateMD5Hash(name);

            //registration.boundingBox = boundingBox;

            return registration;
        }
    }

    public class Entity
    {

    }
}
