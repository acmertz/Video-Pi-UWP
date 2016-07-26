using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Video_Pi.Models
{
    [DataContract]
    class VideoPiProject
    {
        public string Name { get; set; }

        [DataMember]
        public ProjectResolution Resolution { get; set; }

        [DataMember]
        public string AspectRatio { get; set; }

        public VideoPiProject(string name, string aspectRatio, int width, int height)
        {
            Name = name;
            AspectRatio = aspectRatio;
            Resolution = new ProjectResolution(width, height);
        }
    }

    [DataContract]
    class ProjectResolution
    {
        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }

        public ProjectResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
