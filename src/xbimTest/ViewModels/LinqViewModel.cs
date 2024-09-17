using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xbimTest.Services;

namespace xbimTest.ViewModels
{
    internal class LinqViewModel
    {
        private readonly LinqService linqService;
        private readonly string path;
        private const string pathConst= "";

        public LinqViewModel(LinqService linqService)
        {
            this.linqService = linqService;


            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "武汉台北路万象城.ifc");

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            linqService.GetWallWithoutLinq(path);
            stopWatch.Stop();
            Console.WriteLine($"GetWallWithoutLinq:{stopWatch.ElapsedMilliseconds}");
            stopWatch.Reset();

            stopWatch.Start();
            linqService.GetWallByLinqWhere(path);
            stopWatch.Stop();
            Console.WriteLine($"GetWallByLinqWhere:{stopWatch.ElapsedMilliseconds}");
            stopWatch.Reset();

            stopWatch.Start();
            linqService.GetWallByLinqConcat(path);
            stopWatch.Stop();
            Console.WriteLine($"GetWallByLinqConcat:{stopWatch.ElapsedMilliseconds}");
            stopWatch.Reset();
        }
    }
}
