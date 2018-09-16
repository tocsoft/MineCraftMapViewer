using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace MapViewer.Pages
{
    public class MapModel : PageModel
    {
        private readonly IConfiguration configuration;

        public MapModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Dictionary<string, string> Maps { get; private set; }
        public bool MapName { get; private set; }
        public string Id { get; private set; }

        public void OnGet(string id)
        {
            var rootFolder = Path.GetFullPath(configuration.GetValue<string>("RenderedMapLocation"));
            var mapDataDirectory = Path.Combine(rootFolder, id);

            this.MapName = System.IO.File.Exists(Path.Combine(mapDataDirectory, "levelname.txt"));
            this.Id = id;
        }
    }
}
