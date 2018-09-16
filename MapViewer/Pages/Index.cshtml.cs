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
    public class IndexModel : PageModel
    {
        private readonly IConfiguration configuration;

        public IndexModel(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Dictionary<string, string> Maps { get; private set; }

        public void OnGet()
        {
            var rootFolder = Path.GetFullPath(configuration.GetValue<string>("RenderedMapLocation"));

            var worlds = Directory.EnumerateDirectories(rootFolder);
            this.Maps = worlds.Select(x => new
            {
                path = x,
                namePath = Path.Combine(x, "levelname.txt"),
            })
            .Where(x => System.IO.File.Exists(x.namePath))
            .Select(x => new
            {
                name = System.IO.File.ReadAllText(x.namePath),
                Id = Path.GetFileName(x.path)
            }).ToDictionary(x => x.Id, x => x.name);
        }
    }
}
