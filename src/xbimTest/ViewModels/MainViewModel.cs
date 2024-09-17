using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xbimTest.Services;

namespace xbimTest.ViewModels
{
    internal class MainViewModel
    {
        ProjectService projectService;
        string path;

        public MainViewModel(ProjectService projectService)
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "武汉台北路万象城.ifc");
            this.projectService = projectService;
        }

        public void Run(string prompt)
        {
            var result = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            result += " ";
            prompt = prompt.ToLower();
            switch (prompt)
            {
                case "createstore":
                    var store = projectService.CreateStore();
                    store.SaveAs(path);
                    break;
                default:
                    result += "Invalid prompt words...";
                    break;

            }
            Console.WriteLine(result);
        }
    }
}
