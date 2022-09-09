using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayCore_H4.Model
{
    public class Container
    {

        // veritabanındaki container tablosune eşlenecek olan container nesnesinin oluşturulması
        public virtual long Id { get; set; }
        public virtual string ContainerName { get; set; }
        public virtual decimal Latitude { get; set; }
        public virtual decimal Longitude { get; set; }
        public virtual long VehicleId { get; set; }

    }
}
