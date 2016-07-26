using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using System.Xml;
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
        public MainMenuHome()
        {
            this.InitializeComponent();
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

            Models.VideoPiProject myNewProject = new Models.VideoPiProject("Untitled project", aspectRatio, width, height);

            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Models.VideoPiProject));
            ser.WriteObject(stream1, myNewProject);

            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            string newProjectJSON = sr.ReadToEnd();

            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile newProjectFile = await localFolder.CreateFileAsync(myNewProject.Name + ".vpp", Windows.Storage.CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteTextAsync(newProjectFile, newProjectJSON);

            Debug.WriteLine("Navigating to the Editor...");

            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(Editor));
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
