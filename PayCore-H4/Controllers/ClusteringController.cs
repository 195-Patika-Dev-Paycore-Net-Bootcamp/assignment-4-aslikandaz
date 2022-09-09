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

        public IActionResult Cluster(long vehicleId, int numberOfClusters)
        {
            var ContanerList = _session.Entites.Where(x => x.VehicleId == vehicleId).ToArray();
            var dataArray = ContanerList.Select((x) => (X: x.Latitude, Y: x.Longitude)).ToArray();
            var Containers = dataArray.Select(n => new double[] { (double)n.X, (double)n.Y }).ToArray();


            var random = new Random(5555);
            
            var responseList = Enumerable
                                    .Range(0, Containers.Length)
                                    .Select(index => (AssignedCluster: random.Next(0, numberOfClusters),
                                                  Values: Containers[index]))
                                    .ToList();

            var DimensionNumber = Containers[0].Length;
            var limit = 10000;
            var Update = true;

            while (--limit > 0)
            {
                 
                    var CentreSpots = Enumerable.Range(0, numberOfClusters)
                        .AsParallel()
                        .Select(kumeNumarasi =>
                            (
                                kume: kumeNumarasi,
                                merkezNokta: Enumerable.Range(0, DimensionNumber)
                                    .Select(pivot => responseList.Where(s => s.AssignedCluster == kumeNumarasi)
                                        .Average(s => s.Values[pivot]))
                                    .ToArray())
                        ).ToArray();

                    Update = false;
                    
                    Parallel.For(0, responseList.Count, i =>
                    {
                        var Line = responseList[i];
                        var FormerCluster = Line.AssignedCluster;

                        var RecentCluster = CentreSpots.Select(n => (KumeNumarasi: n.kume,
                                Uzaklik: DistanceCalculation(Line.Values, n.merkezNokta)))
                            .OrderBy(x => x.Uzaklik)
                            .First()
                            .KumeNumarasi;

                        if (RecentCluster != FormerCluster)
                        {
                            responseList[i] = (AssignedCluster: RecentCluster, Values: Line.Values);
                            Update = true;
                        }
                    });

                    if (!Update)
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

        private double DistanceCalculation(double[] FirstPoint, double[] SecondPoint)
        {
            var SquaredDistance = FirstPoint
                .Zip(SecondPoint,
                    (p1, p2) => Math.Pow(p1 - p2, 2)).Sum();
            return Math.Sqrt(SquaredDistance);
        }
    }
}


    

