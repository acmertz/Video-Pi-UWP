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
        public VideoGridSlot[] GridSlots { get; set; }

        public VideoPiProject(string name, int width, int height, VideoGridSlot[] gridSlots)
        {
            Name = name;
            Resolution = new ProjectResolution(width, height);
            GridSlots = gridSlots;
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
