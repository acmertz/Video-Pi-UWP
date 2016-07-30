using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Video_Pi.Models
{
    [DataContract]
    class VideoGridSlot
    {
        [DataMember]
        public double X;

        [DataMember]
        public double Y;

        [DataMember]
        public double Width;

        [DataMember]
        public double Height;

        public Button HeaderElement;

        public Grid TrackElement;

        [DataMember]
        public VideoGridClip Clip;

        public VideoGridSlot (double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }
}
