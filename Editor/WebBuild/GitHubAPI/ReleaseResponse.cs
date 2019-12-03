using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packages.tour_creator.Editor.WebBuild.GitHubAPI
{
    [Serializable]
    class ReleaseResponse
    {
        public int id;
        public string tag_name;
        public string published_at;
        public DateTime PublishedAt => DateTime.Parse(published_at);
        public AssetResponse[] assets;
    }
}
