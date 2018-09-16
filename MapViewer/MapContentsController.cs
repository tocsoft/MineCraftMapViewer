using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MapViewer
{
    [Route("[controller]")]
    public class MapContentsController : Controller
    {
        private readonly string rootFolder;
        private readonly IContentTypeProvider contentTypeProvider;

        public MapContentsController(IConfiguration configuration)
        {
            this.rootFolder = Path.GetFullPath(configuration.GetValue<string>("RenderedMapLocation"));
            this.contentTypeProvider = new FileExtensionContentTypeProvider();
        }

        // GET: api/<controller>
        [HttpGet("{*path}")]
        public IActionResult Get(string path)
        {
            path = path ?? "";

            var finalPath = Path.Combine(this.rootFolder, path);
            if (!finalPath.StartsWith(this.rootFolder))
            {
                return this.NotFound();
            }
            
            if (Directory.Exists(finalPath))
            {
                if (!path.EndsWith("/"))
                {
                    return this.Redirect(this.Request.Path + '/');
                    //return this.RedirectToAction("Get", new { path = path + '/' });
                }
                finalPath = Path.Combine(finalPath, "index.html");
            }

            if (!System.IO.File.Exists(finalPath))
            {
                return NotFound();
            }

            if (!this.contentTypeProvider.TryGetContentType(finalPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }


            return this.PhysicalFile(finalPath, contentType);
        }
    }
}
