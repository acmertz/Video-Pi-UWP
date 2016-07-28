using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Json;
using Video_Pi.Models;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
    public sealed partial class Editor : Page
    {
        private MediaComposition mediaComposition;
        private MediaStreamSource mediaStreamSource;
        public Editor()
        {
            this.InitializeComponent();

            mediaComposition = new MediaComposition();
            mediaComposition.Clips.Add(MediaClip.CreateFromColor(Windows.UI.Color.FromArgb(1, 0, 0, 0), new TimeSpan(10000)));
            for (int i=0; i<4; i++) mediaComposition.OverlayLayers.Add(new MediaOverlayLayer());
            UpdateMediaStreamSource();

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Unload;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string projectPath = (string)e.Parameter;
            FileNameTextBlock.Text = projectPath;
            loadProject(projectPath);            
        }

        private void Unload(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = "";
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                rootFrame.GoBack();
            }
        }

        private void UpdateMediaStreamSource()
        {
            mediaStreamSource = mediaComposition.GeneratePreviewMediaStreamSource((int)EditorPlaybackCanvas.ActualWidth, (int)EditorPlaybackCanvas.ActualHeight);
            EditorPlaybackCanvas.SetMediaStreamSource(mediaStreamSource);
        }

        async private void loadProject (string pathToProject)
        {
            // Open the file at the given path and read its contents
            StorageFile projectFile = await StorageFile.GetFileFromPathAsync(pathToProject);
            string fileContents = await Windows.Storage.FileIO.ReadTextAsync(projectFile);

            // Deserialize the JSON data
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(VideoPiProject));
            MemoryStream ms = new MemoryStream(System.Text.ASCIIEncoding.ASCII.GetBytes(fileContents));
            VideoPiProject currentProject = (VideoPiProject)js.ReadObject(ms);

            // Perform additional remaining setup tasks
            currentProject.Name = projectFile.DisplayName;
            ApplicationView appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.Title = currentProject.Name;

            Debug.WriteLine("Loaded the project.");
        }

        async private void importMedia (int targetSlot)
        {
            // Pick a media file
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".avi");
            picker.CommitButtonText = "Import";

            // Add the file
            Windows.Storage.StorageFile fileToImport = await picker.PickSingleFileAsync();
            if (fileToImport != null)
            {
                // Add the item to the future access list so we can reload the project later
                var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
                storageItemAccessList.Add(fileToImport);

                // Create a MediaClip and MediaOverlay to hold it
                var clipToImport = await MediaClip.CreateFromFileAsync(fileToImport);
                var mediaOverlayToImport = new MediaOverlay(clipToImport);

                // Todo: Generate coordinates for the overlay based on the slot
                Rect overlayPosition;
                overlayPosition.Width = 320;
                overlayPosition.Height = 180;
                overlayPosition.X = 0;
                overlayPosition.Y = 0;

                // Set the overlay's coordinates and add it to the media composition
                mediaOverlayToImport.Position = overlayPosition;
                mediaComposition.OverlayLayers[targetSlot].Overlays.Add(mediaOverlayToImport);

                // Update the playback canvas
                UpdateMediaStreamSource();
            }
        }

        private void SlotButtonClicked(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            int clickedSlot = Int32.Parse((string)clickedButton.Content);
            importMedia(clickedSlot);
        }
    }
}
