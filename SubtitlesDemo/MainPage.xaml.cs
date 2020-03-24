using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SubtitlesDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        

        // Keep a map to correlate sources with their URIs for error handling
        Dictionary<TimedTextSource, Uri> ttsMap = new Dictionary<TimedTextSource, Uri>();
        MediaPlaybackItem playbackItem;
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Create the media source and supplement with external timed text sources.
            /*var source = MediaSource.CreateFromUri(new Uri("https://mediaplatstorage1.blob.core.windows.net/windows-universal-samples-media/elephantsdream-clip-h264_sd-aac_eng-aac_spa-aac_eng_commentary.mp4"));
            var source = MediaSource.CreateFromUri( new Uri("https://mediaplatstorage1.blob.core.windows.net/windows-universal-samples-media/elephantsdream-clip-h264_sd-aac_eng-aac_spa-aac_eng_commentary-srt_eng-srt_por-srt_swe.mkv");
            var ttsEnUri = new Uri("ms-appx:///Assets/ElephantsDream-Clip-SRT_en.srt");
            Debug.WriteLine("#####" + ttsEnUri);
            var ttsEn = TimedTextSource.CreateFromUri(ttsEnUri);
            Debug.WriteLine("*****" + ttsEn);
            ttsMap[ttsEn] = ttsEnUri;

            ttsEn.Resolved += Tts_Resolved;
        
            source.ExternalTimedTextSources.Add(ttsEn);
            

            // Create the playback item from the source
             playbackItem = new MediaPlaybackItem(source);
           
            // Present the first track
            playbackItem.TimedMetadataTracksChanged += (item, args) =>
            {
                Debug.WriteLine($"TimedMetadataTracksChanged, Number of tracks: {item.TimedMetadataTracks.Count}");
                playbackItem.TimedMetadataTracks.SetPresentationMode(0, TimedMetadataTrackPresentationMode.PlatformPresented);
            };

            // Set the source to start playback of the item
            this.mediaplayerElement.Source = playbackItem;*/
            
            var source = MediaSource.CreateFromUri(new Uri("https://mediaplatstorage1.blob.core.windows.net/windows-universal-samples-media/elephantsdream-clip-h264_sd-aac_eng-aac_spa-aac_eng_commentary-srt_eng-srt_por-srt_swe.mkv"));
            // var source = MediaSource.CreateFromUri(new Uri("https://mediaplatstorage1.blob.core.windows.net/windows-universal-samples-media/elephantsdream-clip-h264_sd-aac_eng-aac_spa-aac_eng_commentary.mp4"));

            playbackItem = new MediaPlaybackItem(source);
            /*   playbackItem.TimedMetadataTracksChanged += (item, args) =>
               {
                   Debug.WriteLine($"{item.TimedMetadataTracks.Count}  ");
                   if (args.CollectionChange == CollectionChange.ItemInserted)
                   {

                       Debug.WriteLine($"TimedMetadataTracksChanged, Number of tracks: {item.TimedMetadataTracks.Count}");
                       uint changedTrackIndex = args.Index;
                       TimedMetadataTrack changedTrack = playbackItem.TimedMetadataTracks[(int)changedTrackIndex];

                       if (changedTrack.Language == "en")
                       {
                           playbackItem.TimedMetadataTracks.SetPresentationMode(changedTrackIndex, TimedMetadataTrackPresentationMode.PlatformPresented);
                       }
                   }

               };*/
            SetDefaultSubtitle("en");
            this.mediaplayerElement.Source = playbackItem;
        }
        private async void Open_File(object sender, RoutedEventArgs e)
        {
            await PickSingleVideoFile();
        }
        async private System.Threading.Tasks.Task PickSingleVideoFile()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wma");
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".mkv");

            var file = await openPicker.PickSingleFileAsync();
            //Debug.WriteLine(file);
            // mediaPlayer is a MediaPlayerElement defined in XAML
            if (file != null)
            {
                var source = MediaSource.CreateFromStorageFile(file);
                playbackItem = new MediaPlaybackItem(source);
                SetDefaultSubtitle("en");
                mediaplayerElement.Source = playbackItem;
            }
        }

            private void Tts_Resolved(TimedTextSource sender, TimedTextSourceResolveResultEventArgs args)
        {
            var ttsUri = ttsMap[sender];
            Debug.WriteLine($"TTs-Resolve  {ttsUri}");
            // Handle errors
            if (args.Error != null)
            {
                var ignoreAwaitWarning = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Debug.WriteLine($"Unresolved : {ttsUri} {args.Error}  {args.Error.ErrorCode} {args.Error.ExtendedError}");
                });
                return;
            }
            Debug.WriteLine($"Resolved Yayyyyyyyyyyy : {ttsUri}");

            // Update label manually since the external SRT does not contain it
            var ttsUriString = ttsUri.AbsoluteUri;
            if (ttsUriString.Contains("_en"))
                args.Tracks[0].Label = "English";
            else if (ttsUriString.Contains("_pt"))
                args.Tracks[0].Label = "Portuguese";
            else if (ttsUriString.Contains("_sv"))
                args.Tracks[0].Label = "Swedish";
        }
        private async void Pick_File(object sender, RoutedEventArgs e)
        {
            await PickSingleFile();
        }

        async private System.Threading.Tasks.Task PickSingleFile()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            openPicker.FileTypeFilter.Add(".srt");
            openPicker.FileTypeFilter.Add(".vtt");
            var file = await openPicker.PickSingleFileAsync();
            Debug.WriteLine(file);

            if (file != null)
            {
                IRandomAccessStream strSource = await file.OpenReadAsync();
                
                AddSubtitle(file, strSource);
                
            }
        }

         private void AddSubtitle(StorageFile file, IRandomAccessStream strSource) {

            var ttsUri = new Uri(file.Path);
            var ttsPicked = TimedTextSource.CreateFromStream(strSource);
            ttsMap[ttsPicked] = ttsUri;
            ttsPicked.Resolved += Tts_Resolved;
            playbackItem.Source.ExternalTimedTextSources.Add(ttsPicked);
            // Present the first track
            playbackItem.TimedMetadataTracksChanged += (item, args) =>
            {
                Debug.WriteLine($"TimedMetadataTracksChanged, Number of tracks: {item.TimedMetadataTracks.Count}");
                uint changedTrackIndex = args.Index;
                 TimedMetadataTrack changedTrack = playbackItem.TimedMetadataTracks[(int)changedTrackIndex];
                playbackItem.TimedMetadataTracks.SetPresentationMode(changedTrackIndex, TimedMetadataTrackPresentationMode.PlatformPresented);
            };

        }

        private void SetDefaultSubtitle(String lang)
        {
            // Present the first track
            Debug.WriteLine($"Tryin to set default subtitle to : {lang}");
            playbackItem.TimedMetadataTracksChanged += (item, args) =>
            {
                if (args.CollectionChange == CollectionChange.ItemInserted)
                {
                   
                    Debug.WriteLine($"TimedMetadataTracksChanged, Number of tracks: {item.TimedMetadataTracks.Count}");
                    uint changedTrackIndex = args.Index;
                    TimedMetadataTrack changedTrack = playbackItem.TimedMetadataTracks[(int)changedTrackIndex];
                    Debug.WriteLine($"Log : {changedTrack.Language}");
                    if (changedTrack.Language == lang)
                    {
                        Debug.WriteLine($"Trying  : {lang}");
                        playbackItem.TimedMetadataTracks.SetPresentationMode(changedTrackIndex, TimedMetadataTrackPresentationMode.PlatformPresented);
                        Debug.WriteLine($"Set default subtitle to : {lang}");
                    }
                   
                }
            };

        }

    }

    


}
