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
    /// The Editor. Import and record clips, adjust timing, and export your finished project.
    /// </summary>
    public sealed partial class Editor : Page
    {
        private VideoPiProject CurrentProject;
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
            for (int i=0; i<CurrentProject.Composition.OverlayLayers.Count; i++)
            {
                MediaOverlayLayer currentLayer = CurrentProject.Composition.OverlayLayers[i];
                for (int k=0; k<currentLayer.Overlays.Count; k++)
                {
                    TimeSpan currentClipEnd = currentLayer.Overlays[k].Clip.EndTimeInComposition;
                    if (currentClipEnd.CompareTo(lastTime) == 1) lastTime = currentClipEnd;
                }
            }

            if (lastTime.Ticks == 0) lastTime = new TimeSpan(0, 10, 0);
            CurrentProject.Composition.Clips.RemoveAt(0);
            CurrentProject.Composition.Clips.Add(MediaClip.CreateFromColor(Windows.UI.Color.FromArgb(1, 0, 0, 0), lastTime));

            // Generate and set the stream source
            MediaStreamSource mediaStreamSource = CurrentProject.Composition.GeneratePreviewMediaStreamSource((int)EditorPlaybackCanvas.ActualWidth, (int)EditorPlaybackCanvas.ActualHeight);
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
            CurrentProject = (VideoPiProject)js.ReadObject(ms);
            CurrentProject.File = projectFile;

            // Set up the media composition based on the project details
            CurrentProject.Composition.Clips.Add(MediaClip.CreateFromColor(Windows.UI.Color.FromArgb(1, 0, 0, 0), new TimeSpan(0)));
            for (int i = 0; i < 4; i++) CurrentProject.Composition.OverlayLayers.Add(new MediaOverlayLayer());
            UpdateMediaStreamSource();

            // Todo: dynamically build the layout and slots based on the loaded project file
            for (int i=0; i<CurrentProject.GridSlots.Length; i++)
            {
                Button tempHeader = new Button();
                tempHeader.Content = (i + 1).ToString();
                tempHeader.Style = TimelineHeaderStyle;
                tempHeader.Click += SlotButtonClicked;

                Grid tempTrack = new Grid();
                tempTrack.Style = TimelineTrackStyle;
                if (i % 2 == 0) tempTrack.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255));

                TimelineHeaderContainer.Children.Add(tempHeader);
                TimelineTrackContainer.Children.Add(tempTrack);

                CurrentProject.GridSlots[i].HeaderElement = tempHeader;
                CurrentProject.GridSlots[i].TrackElement = tempTrack;
            }

            // Set window title and enable the back button
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Unload;

            // Perform additional remaining setup tasks
            ApplicationView appView = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView();
            appView.Title = CurrentProject.File.DisplayName;

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

                // Generate coordinates for the overlay based on the slot
                double playbackWidth = EditorPlaybackCanvas.ActualWidth;
                double playbackHeight = EditorPlaybackCanvas.ActualHeight;

                Rect overlayPosition;
                overlayPosition.Width = CurrentProject.GridSlots[targetSlot].Width * playbackWidth;
                overlayPosition.Height = CurrentProject.GridSlots[targetSlot].Height * playbackHeight;
                overlayPosition.X = CurrentProject.GridSlots[targetSlot].X * playbackWidth;
                overlayPosition.Y = CurrentProject.GridSlots[targetSlot].Y * playbackHeight;

                // Set the overlay's coordinates and add it to the media composition
                mediaOverlayToImport.Position = overlayPosition;
                mediaOverlayToImport.AudioEnabled = true;
                CurrentProject.Composition.OverlayLayers[targetSlot].Overlays.Add(mediaOverlayToImport);

                // Create a display for the clip in the timeline
                StackPanel tempClipEl = new StackPanel();
                tempClipEl.Style = TimelineClipStyle;
                tempClipEl.Width = clipToImport.OriginalDuration.TotalMilliseconds / CurrentProject.MsPerPx;

                TextBlock tempTitle = new TextBlock();
                tempTitle.Style = TimelineClipTitleStyle;
                tempTitle.Text = fileToImport.DisplayName;
                tempClipEl.Children.Add(tempTitle);

                VideoGridClip clipObj = new VideoGridClip(fileToImport, tempClipEl);

                // Add the clip to the track and update it in the model
                CurrentProject.GridSlots[targetSlot].TrackElement.Children.Add(tempClipEl);
                CurrentProject.GridSlots[targetSlot].Clip = clipObj;

                // Update the playback canvas
                UpdateMediaStreamSource();

                // Trigger a save operation
                saveProject();
            }
        }

        async private void saveProject()
        {
            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Models.VideoPiProject));
            ser.WriteObject(stream1, CurrentProject);

            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            string projectJSON = sr.ReadToEnd();

            await FileIO.WriteTextAsync(CurrentProject.File, projectJSON);

            Debug.WriteLine("Saved the proejct.");
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

        private void TimelineViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            TimelineRulerScrollContainer.ChangeView(TimelineScrollContainer.HorizontalOffset, 0, TimelineRulerScrollContainer.ZoomFactor);
            TimelineHeaderScrollContainer.ChangeView(0, TimelineScrollContainer.VerticalOffset, TimelineHeaderScrollContainer.ZoomFactor);
        }
    }
}
