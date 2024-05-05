using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace AllSports.Helpers
{
    public enum Folders { Images = 0, Uploads = 1 }
    public class HelperPathProvider
    {
        private IServer server;
        private IWebHostEnvironment hostEnvironment;

        public HelperPathProvider(IWebHostEnvironment hostEnvironment, IServer server)

        {

            this.hostEnvironment = hostEnvironment;
            this.server = server;

        }
        public string MapPath(string fileName, Folders folder)

        {

            string carpeta = "";

            if (folder == Folders.Images)

            {

                carpeta = "images";

            }
            else if (folder == Folders.Uploads)
            {
                carpeta = "uploads";
            }
            string rootPath = this.hostEnvironment.WebRootPath;

            string path = Path.Combine(rootPath, carpeta, fileName);

            return path;

        }

        
        public string MapUrlPath(string fileName, Folders folder)
        {
            string carpeta = "";

            if (folder == Folders.Images)
            {
                carpeta = "images";
            }
            else if (folder == Folders.Uploads)
            {
                carpeta = "uploads";
            }
            var addresses = server.Features.Get<IServerAddressesFeature>().Addresses;
            string serverUrl = addresses.FirstOrDefault();
            string urlPath = serverUrl + "/" + carpeta + "/" + fileName;
            return urlPath;
        }
    }
}
