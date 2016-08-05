using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Video_Pi.Models
{
    [DataContract]
    class VideoGridClip
    {
        public StackPanel ClipElement;

        public StorageFile File;

        [DataMember]
        public string Path;

        public VideoGridClip (StorageFile file)
        {
            File = file;
            Path = file.Path;
        }
    }
}
