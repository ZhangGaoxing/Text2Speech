using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.Storage.Pickers;
using Text2Speech.Managers;
using System.Threading.Tasks;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using System.Text;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Text2Speech.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ConvertPage : Page
    {
        private SpeechSynthesizer synthesizer;

        public ConvertPage()
        {
            this.InitializeComponent();

            synthesizer = new SpeechSynthesizer();

            InitConvertVoice();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SetStyle();

            if (DeviceInfoManager.GetOsVersion() < 16299)
            {
                ConvertFuncPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid grid = sender as Grid;

            if (grid.ActualWidth < 641)
            {
                Grid.SetRow(ConvertText, 0);
                Grid.SetColumn(ConvertText, 0);
                Grid.SetRowSpan(ConvertText, 1);
                Grid.SetColumnSpan(ConvertText, 2);

                Grid.SetRow(ConvertFuncPanel, 1);
                Grid.SetColumn(ConvertFuncPanel, 0);
                Grid.SetRowSpan(ConvertFuncPanel, 1);
                Grid.SetColumnSpan(ConvertFuncPanel, 2);

                ConvertText.MaxHeight = (Window.Current.Bounds.Height - 110) / 2.0;
            }
            else
            {
                Grid.SetRow(ConvertText, 0);
                Grid.SetColumn(ConvertText, 0);
                Grid.SetRowSpan(ConvertText, 2);
                Grid.SetColumnSpan(ConvertText, 1);

                Grid.SetRow(ConvertFuncPanel, 0);
                Grid.SetColumn(ConvertFuncPanel, 1);
                Grid.SetRowSpan(ConvertFuncPanel, 2);
                Grid.SetColumnSpan(ConvertFuncPanel, 1);

                ConvertText.MaxHeight = Window.Current.Bounds.Height - 110;
            }
        }

        private void InitConvertVoice()
        {
            var voices = SpeechSynthesizer.AllVoices;
            VoiceInformation currentVoice = synthesizer.Voice;

            foreach (VoiceInformation voice in voices.OrderBy(p => p.Language))
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Name = voice.DisplayName;
                item.Tag = voice;
                item.Content = voice.DisplayName + " (" + voice.Language + ")";
                ConvertVoice.Items.Add(item);

                if (currentVoice.Id == voice.Id)
                {
                    item.IsSelected = true;
                    ConvertVoice.SelectedItem = item;
                }
            }
        }

        private void ConvertVoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = ConvertVoice.SelectedItem as ComboBoxItem;
            VoiceInformation voice = (VoiceInformation)(item.Tag);
            synthesizer.Voice = voice;
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Media.CurrentState == MediaElementState.Playing)
            {
                Media.Stop();
            }
            else
            {
                string text = ConvertText.Text;
                if (!String.IsNullOrEmpty(text))
                {
                    try
                    {
                        ConvertProgress.IsActive = true;

                        if (DeviceInfoManager.GetOsVersion() >= 16299)
                        {
                            SetSpeechOptions();
                        }
                        SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

                        var mediaSource = MediaSource.CreateFromStream(synthesisStream, synthesisStream.ContentType); 
                        var mediaPlaybackItem = new MediaPlaybackItem(mediaSource);  
                        RegisterForWordBoundaryEvents(mediaPlaybackItem);

                        Media.AutoPlay = true;
                        //Media.SetSource(synthesisStream, synthesisStream.ContentType);
                        Media.SetPlaybackSource(mediaPlaybackItem);
                        Media.Play();

                        ConvertProgress.IsActive = false;
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components unavailable");
                        await messageDialog.ShowAsync();
                        ConvertProgress.IsActive = false;
                    }
                    catch (Exception)
                    {
                        Media.AutoPlay = false;
                        var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to synthesize text");
                        await messageDialog.ShowAsync();
                        ConvertProgress.IsActive = false;
                    }
                }
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string text = ConvertText.Text;
            if (!String.IsNullOrEmpty(text))
            {
                try
                {
                    var savePicker = new FileSavePicker();
                    savePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
                    savePicker.FileTypeChoices.Add("WAV 文件", new List<string>() { ".wav" });
                    savePicker.SuggestedFileName = $"Speech_{ DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.wav";
                    StorageFile file = await savePicker.PickSaveFileAsync();

                    ConvertProgress.IsActive = true;

                    if (DeviceInfoManager.GetOsVersion() >= 16299)
                    {
                        SetSpeechOptions();
                    }
                    SpeechSynthesisStream synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(text);

                    var fileStream = await file.OpenStreamForWriteAsync();
                    byte[] bytes = new byte[synthesisStream.AsStream().Length];
                    synthesisStream.AsStream().Read(bytes, 0, bytes.Length);
                    await fileStream.WriteAsync(bytes, 0, bytes.Length);
                    await fileStream.FlushAsync();

                    ConvertProgress.IsActive = false;
                }
                catch (ArgumentNullException)
                {
                    ConvertProgress.IsActive = false;
                }
                catch (Exception)
                {
                    ConvertProgress.IsActive = false;
                    var messageDialog = new Windows.UI.Popups.MessageDialog("Unable to synthesize text");
                    await messageDialog.ShowAsync();
                }
            } 
        }

        private void SetSpeechOptions()
        {
            ConvertText.SelectionHighlightColorWhenNotFocused = new SolidColorBrush(Colors.Blue);
            synthesizer.Options.IncludeWordBoundaryMetadata = true;
            synthesizer.Options.AudioPitch = ConvertPitch.Value * 2 / 100.0;
            synthesizer.Options.AudioVolume = ConvertVolume.Value / 100.0;
            synthesizer.Options.SpeakingRate = ConvertRate.Value * 6 / 100.0;
        }

        public void SetStyle()
        {
            if (DeviceInfoManager.GetOsVersion() >= 16299)
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
                {
                    Style button = (Style)Application.Current.Resources["ButtonRevealStyle"];
                    Play.Style = button;
                    Save.Style = button;
                }
            }
        }

        #region WordBoundary
        private void RegisterForWordBoundaryEvents(MediaPlaybackItem mediaPlaybackItem)
        {
            for (int index = 0; index < mediaPlaybackItem.TimedMetadataTracks.Count; index++)
            {
                RegisterMetadataHandlerForWords(mediaPlaybackItem, index);
            }

            mediaPlaybackItem.TimedMetadataTracksChanged += (MediaPlaybackItem sender, IVectorChangedEventArgs args) =>
            {
                if (args.CollectionChange == CollectionChange.ItemInserted)
                {
                    RegisterMetadataHandlerForWords(sender, (int)args.Index);
                }
                else if (args.CollectionChange == CollectionChange.Reset)
                {
                    for (int index = 0; index < sender.TimedMetadataTracks.Count; index++)
                    {
                        RegisterMetadataHandlerForWords(sender, index);
                    }
                }
            };
        }

        private void RegisterMetadataHandlerForWords(MediaPlaybackItem mediaPlaybackItem, int index)
        {
            var timedTrack = mediaPlaybackItem.TimedMetadataTracks[index];

            if (timedTrack.Id == "SpeechWord")
            {
                timedTrack.CueEntered += Metadata_SpeechCueEntered;
                mediaPlaybackItem.TimedMetadataTracks.SetPresentationMode((uint)index, TimedMetadataTrackPresentationMode.ApplicationPresented);
            }
        }

        private async void Metadata_SpeechCueEntered(TimedMetadataTrack timedMetadataTrack, MediaCueEventArgs args)
        {
            if (timedMetadataTrack.TimedMetadataKind == TimedMetadataKind.Speech)
            {
                var cue = args.Cue as SpeechCue;
                if (cue != null)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                     () =>
                     {
                         HighlightTextOnScreen(cue.StartPositionInInput, cue.EndPositionInInput);
                     });
                }
            }
        }

        private void HighlightTextOnScreen(int? startPositionInInput, int? endPositionInInput)
        {
            if (startPositionInInput != null && endPositionInInput != null)
            {
                ConvertText.Select(startPositionInInput.Value, endPositionInInput.Value - startPositionInInput.Value + 1);
            }
        }
        #endregion

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            var openFile = new FileOpenPicker();
            openFile.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openFile.FileTypeFilter.Add(".txt");
            var file = await openFile.PickSingleFileAsync();

            if (file != null)
            {
                var stream = await file.OpenStreamForReadAsync();
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);

                ConvertText.Text = Encoding.UTF8.GetString(buffer);
            }
        }
    }
}
