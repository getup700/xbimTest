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
        SimpleProjectService wallService;
        string path;

        public MainViewModel(ProjectService projectService, SimpleProjectService wallService)
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "HELLO.ifc");
            this.projectService = projectService;
            this.wallService = wallService;
        }

        public void Run(string prompt)
        {
            var result = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            result += " ";
            prompt = prompt.ToLower();
            switch (prompt)
            {
                case "createstore":
                    var store = projectService.CreateStore("NewProject");
                    projectService.Save(store, path);
                    break;
                case "helloifc":
                    wallService.BuildingSimpleIfcProject(path); 
                    break;
                default:
                    result += "Invalid prompt words...";
                    break;
            }
            Console.WriteLine(result);
        }
    }
}
