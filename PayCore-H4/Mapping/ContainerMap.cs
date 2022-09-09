using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using PayCore_H4.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PayCore_H4.Mapping
{
    public class ContainerMap : ClassMapping<Container>
    {
        // veritabanındaki container tablosu ile proje içerisindeki container nesnesinin eşlenmesi
        public ContainerMap()
        {

            Id(x => x.Id, x =>
            {
                x.Type(NHibernateUtil.Int64);
                x.Column("id");
                x.UnsavedValue(0);
                x.Generator(Generators.Increment);
            });

            Property(b => b.ContainerName, x =>
            {
                x.Length(50);
                x.Type(NHibernateUtil.String);
                x.NotNullable(true);
            });
            Property(x => x.Latitude, x =>
                {
                    x.Type(NHibernateUtil.Currency);
                    x.Precision(10);
                    x.Scale(6);
                });
            Property(x => x.Longitude, x =>
                {
                    x.Type(NHibernateUtil.Currency);
                    x.Precision(10);
                    x.Scale(6);
                });

            Property(x => x.VehicleId, x =>
            {
                x.Type(NHibernateUtil.Int64);
                x.Column("vehicleid");

            });

            Table("container");
        }
    }
}
