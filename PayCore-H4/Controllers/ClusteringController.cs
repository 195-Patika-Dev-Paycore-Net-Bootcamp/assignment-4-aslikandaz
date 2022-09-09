using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PayCore_H4.Context;
using PayCore_H4.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace PayCore_H4.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClusteringController : ControllerBase
    {
        private readonly IMapperSession<Container> _session;

        public ClusteringController(IMapperSession<Container> session)
        {
            _session = session;
        }

        [HttpPost("Cluster")]

        public IActionResult Cluster(long vehicleId, int clusterAmount)
        {
            var ContanerList = _session.Entites.Where(x => x.VehicleId == vehicleId).ToArray(); // gelen vehicleId ye ait containerların listelenmesi
            var dataArray = ContanerList.Select((x) => (X: x.Latitude, Y: x.Longitude)).ToArray(); // veritabanından çekilen containerların lat ve long değerlerinin çekilmesi
            var Containers = dataArray.Select(n => new double[] { (double)n.X, (double)n.Y }).ToArray(); // array haline getirilen verilerden bir dizi oluşturulması


            var random = new Random(5555); // rastgele sayı üreteci
            
            var responseList = Enumerable
                                    .Range(0, Containers.Length) //kaç satır verimiz varsa o kadar sayı üretiyotuz
                                    .Select(index => (AssignedCluster: random.Next(0, clusterAmount),//her satıra önce rastgele bir küme numarası ardından değerler atıyoruz
                                                  Values: Containers[index]))
                                    .ToList();

            var DimensionNumber = Containers[0].Length;
            var limit = 10000; //algoritmaynın eşik(max iterasyon) değeri
            var Update = true; // döngüyü sonlandıracak değişken

            while (--limit > 0) // döngü limit 0'a eşit olana kadar devam edecek
            {
                var CentreSpots = Enumerable.Range(0, clusterAmount)//arzu edilen küme sayısı kadar sayı üretiyoruz
                        .AsParallel() //gelen döngünün multithread çalışmasını sağlıyoruz
                        .Select(numberOfCluster =>// her küme için merkez nokta hesaplıyoruz
                            (
                            cluster: numberOfCluster,
                                centreSpot: Enumerable.Range(0, DimensionNumber) //verideki boyut sayısı kadar sayı oluşturuyoruz
                                    .Select(pivot => responseList.Where(s => s.AssignedCluster == numberOfCluster)
                                        .Average(s => s.Values[pivot])) // ilgili kümeye atanmış verilerin ilgili eksenindeki değerlerin ortalamasını alıyoruz
                                    .ToArray())
                        ).ToArray();


                    Update = false;
                    
                    // en yakın merkez noktaya göre yeniden atama yapılıyor
                    Parallel.For(0, responseList.Count, i => ////for (int i = 0; i < responseList.Count; i++)
                    {
                        var Line = responseList[i]; // mevcut veri tutuluyor
                        var FormerCluster = Line.AssignedCluster; // bu satır için bir önceki atanan küme tutuluyor

                        var RecentCluster = CentreSpots.Select(n => (clusterNumber:n.cluster, // her merkez noktayı tek tek dönüp line değerine uzaklığı en az olanı seçiyoruz
                                Distance: DistanceCalculation(Line.Values, n.centreSpot)))
                            .OrderBy(x => x.Distance)
                            .First()
                            .clusterNumber;

                        if (RecentCluster != FormerCluster) // yeni atanacak küme eskisiyle aynı değilse güncelliyoruz ve güncellemeyi true yaparal döngüyü sonlandırıyoruz
                        {
                            responseList[i] = (AssignedCluster: RecentCluster, Values: Line.Values);
                            Update = true;
                        }
                    });

                    if (!Update) // hiçbir güncelleme olmadıysa büyük döngüyü sonlandırıyoruz
                    {
                        break;
                    }

                    
            }

            var response =
                (responseList.Select((assignedCluster, values) => new
                        {assignedCluster = assignedCluster.AssignedCluster, value = assignedCluster.Values})
                    .GroupBy(i => i.assignedCluster, g => g.value).ToList()).ToList();
           
            return Ok(response);
        }

        private double DistanceCalculation(double[] FirstPoint, double[] SecondPoint) //iki adet n elemanlı vektör parametre alıyor ve öklid formülü uygulanıyor
        {
            var SquaredDistance = FirstPoint
                .Zip(SecondPoint, // iki adet dizinin sırayla elemanları dönülüyor. değer çiftleri olarak dönüş yapılıyor
                    (p1, p2) => Math.Pow(p1 - p2, 2)).Sum(); // her bir eksendeki değerleri sırasıyla alıp birbirinden çıkarıp sonucun karesini alıyoruz daha sonra tüm bu farkların genel toplamını alıyoruz
            return Math.Sqrt(SquaredDistance); //kareli toplamın karekökünü dönüyoruz
        }
    }
}


    

