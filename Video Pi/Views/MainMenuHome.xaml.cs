using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Xml;
using Video_Pi.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Video_Pi.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenuHome : Page
    {

        private List<VideoPiProject> Projects;
        public MainMenuHome()
        {
            this.InitializeComponent();
            Projects = new List<VideoPiProject>();
            for (int i=0; i<4; i++)
            {
                Projects.Add(new VideoPiProject(1920, 1080, new VideoGridSlot[0]));
            }
        }

        private async void CreateNewProject(string aspectRatio)
        {
            int width = 0;
            int height = 0;

            switch(aspectRatio)
            {
                case "4:3":
                    width = 2560;
                    height = 1920;
                    break;
                case "16:10":
                    width = 2560;
                    height = 1600;
                    break;
                case "16:9":
                    width = 2560;
                    height= 1440;
                    break;
                case "2.39:1":
                    width = 3824;
                    height = 1600;
                    break;
            }

            // Todo: build UI for selecting grid presets and initialize the grid based on the user's selection
            Models.VideoGridSlot[] gridSlots = new Models.VideoGridSlot[4];
            gridSlots[0] = new Models.VideoGridSlot(0, 0, .5, .5);
            gridSlots[1] = new Models.VideoGridSlot(.5, 0, .5, .5);
            gridSlots[2] = new Models.VideoGridSlot(0, .5, .5, .5);
            gridSlots[3] = new Models.VideoGridSlot(.5, .5, .5, .5);

            Models.VideoPiProject myNewProject = new Models.VideoPiProject(width, height, gridSlots);

            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Models.VideoPiProject));
            ser.WriteObject(stream1, myNewProject);

            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            string newProjectJSON = sr.ReadToEnd();

            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile newProjectFile = await localFolder.CreateFileAsync("Untitled project.vpp", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteTextAsync(newProjectFile, newProjectJSON);

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(Editor), newProjectFile.Path);
        }

        private void NewProjectButtonClicked(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            switch (clickedButton.Name)
            {
                case "AspectButton_43":
                    CreateNewProject("4:3");
                    break;
                case "AspectButton_1610":
                    CreateNewProject("16:10");
                    break;
                case "AspectButton_169":
                    CreateNewProject("16:9");
                    break;
                case "AspectButton_2391":
                    CreateNewProject("2.39:1");
                    break;
            }
        }
    }
}
