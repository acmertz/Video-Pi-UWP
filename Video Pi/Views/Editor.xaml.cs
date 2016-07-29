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
        private VideoPiProject currentProject;
        private MediaComposition mediaComposition;
        private MediaStreamSource mediaStreamSource;
        public Editor()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string projectPath = (string)e.Parameter;
            loadProject(projectPath);            
        }

        private void Unload(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                e.Handled = true;
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Title = "";
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                rootFrame.GoBack();
            }
        }

        private void UpdateMediaStreamSource()
        {
            // Todo: Determine how long the entire composition is
            TimeSpan lastTime = new TimeSpan(0);
            for (int i=0; i<mediaComposition.OverlayLayers.Count; i++)
            {
                MediaOverlayLayer currentLayer = mediaComposition.OverlayLayers[i];
                for (int k=0; k<currentLayer.Overlays.Count; k++)
                {
                    TimeSpan currentClipEnd = currentLayer.Overlays[k].Clip.EndTimeInComposition;
                    if (currentClipEnd.CompareTo(lastTime) == 1) lastTime = currentClipEnd;
                }
            }

            if (lastTime.Ticks == 0) lastTime = new TimeSpan(0, 10, 0);
            mediaComposition.Clips.RemoveAt(0);
            mediaComposition.Clips.Add(MediaClip.CreateFromColor(Windows.UI.Color.FromArgb(1, 0, 0, 0), lastTime));

            // Generate and set the stream source
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
            currentProject = (VideoPiProject)js.ReadObject(ms);

            // Set up the media composition based on the project details
            mediaComposition = new MediaComposition();
            mediaComposition.Clips.Add(MediaClip.CreateFromColor(Windows.UI.Color.FromArgb(1, 0, 0, 0), new TimeSpan(0)));
            for (int i = 0; i < 4; i++) mediaComposition.OverlayLayers.Add(new MediaOverlayLayer());
            UpdateMediaStreamSource();

            // Todo: dynamically build the layout and slots based on the loaded project file
            for (int i=0; i<currentProject.GridSlots.Length; i++)
            {
                Button tempHeader = new Button();
                tempHeader.Content = (i + 1).ToString();
                tempHeader.Style = TimelineHeaderStyle;
                tempHeader.Click += SlotButtonClicked;

                RelativePanel tempTrack = new RelativePanel();
                tempTrack.Style = TimelineTrackStyle;
                if (i % 2 == 0) tempTrack.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255));

                TimelineHeaderContainer.Children.Add(tempHeader);
                TimelineTrackContainer.Children.Add(tempTrack);
            }

            // Set window title and enable the back button
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Unload;

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
                double playbackWidth = EditorPlaybackCanvas.ActualWidth;
                double playbackHeight = EditorPlaybackCanvas.ActualHeight;

                Rect overlayPosition;
                overlayPosition.Width = currentProject.GridSlots[targetSlot].Width * playbackWidth;
                overlayPosition.Height = currentProject.GridSlots[targetSlot].Height * playbackHeight;
                overlayPosition.X = currentProject.GridSlots[targetSlot].X * playbackWidth;
                overlayPosition.Y = currentProject.GridSlots[targetSlot].Y * playbackHeight;

                // Set the overlay's coordinates and add it to the media composition
                mediaOverlayToImport.Position = overlayPosition;
                mediaOverlayToImport.AudioEnabled = true;
                mediaComposition.OverlayLayers[targetSlot].Overlays.Add(mediaOverlayToImport);

                // Update the playback canvas
                UpdateMediaStreamSource();
            }
        }

        private void SlotButtonClicked(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            int clickedSlot = Int32.Parse((string)clickedButton.Content) - 1;
            importMedia(clickedSlot);
        }

        private void WindowResized(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("The screen was resized. Update the media element's size.");
        }
    }
}
