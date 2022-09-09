using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayCore_H4.Model
{
    public class Vehicle
    {

        // veritabanındaki vehicle tablosune eşlenecek olan vehicle nesnesinin oluşturulması
        public virtual long Id { get; set; }
        public virtual string VehicleName { get; set; }
        public virtual string VehiclePlate { get; set; }
    }
}
