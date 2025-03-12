using Google.Gemini;
using NAudio.Wave;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Windowing;
using System;
using System.IO;
using System.Text.Json;
using Microsoft.UI;
using Windows.Graphics;
using Windows.ApplicationModel.DataTransfer;
using WinUIEx;
using Microsoft.VisualBasic;
using System.Drawing;
using Microsoft.UI.Xaml.Media;
using Google.Protobuf;
using static System.Net.Mime.MediaTypeNames;


namespace Xing
{
    public partial class MainWindow : Window
    {
        bool isRecording = false;
        private static WaveInEvent waveIn;
        private static WaveFileWriter waveFile;

        public MainWindow()
        {
            this.InitializeComponent();

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new SizeInt32(1440, 800));


            translateButton.Click += TranslateButton_Click;
            WindowManager.Get(this).IsMaximizable = false;
            WindowManager.Get(this).IsResizable = false;

        }

        public async void TranslateButton_Click(object sender, RoutedEventArgs e)
        {
            string text = inputTextBox.Text;
            string fromLang = ((ComboBoxItem)fromLanguageComboBox.SelectedItem).Content.ToString();
            string toLang = ((ComboBoxItem)toLanguageComboBox.SelectedItem).Content.ToString();
            var selectedItem = promptBox.SelectedItem;
            var selectedItemL = promptBoxL.SelectedItem;
            var translation = "0";
            if (selectedItem == standard || selectedItemL == standardL)
            {
                translation = await Translate(text, fromLang, toLang, "default_prompt");
            }
            else if (selectedItem == friendly || selectedItemL == friendlyL) {
                translation = await Translate(text, fromLang, toLang, "friendly_prompt");
            }
            else if (selectedItem == formal || selectedItemL == formalL)
            {
                translation = await Translate(text, fromLang, toLang, "formal_prompt");
            }
            else if (selectedItem == creative || selectedItemL == creativeL)
            {
                translation = await Translate(text, fromLang, toLang, "creative_prompt");
            }

            if (DarkGrid.Visibility == Visibility.Visible)
            {
                outputTextBox.Text = translation;
            }
            else
            {
                outputTextBoxL.Text = translation;
            }
        }
        public async void RecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isRecording)
            {
                recordButton.Foreground = new SolidColorBrush(Colors.LightBlue);
                RecordStart();
                isRecording = true;
            }
            else if (isRecording)
            {
                if (DarkGrid.Visibility == Visibility.Visible)
                {
                    recordButton.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    recordButton.Foreground = new SolidColorBrush(Colors.Black);
                }
                RecordStop();
                isRecording = false;
            }

        }

        private async void InputCopy_Click(object sender, RoutedEventArgs e)
        {
            var package = new DataPackage();
            if (DarkGrid.Visibility == Visibility.Visible)
            {
                package.SetText(inputTextBox.Text);
                Clipboard.SetContent(package);
            }
            else
            {
                package.SetText(inputTextBoxL.Text);
                Clipboard.SetContent(package);
            }
        }

        private async void OutputCopy_Click(object sender, RoutedEventArgs e)
        {
            var package = new DataPackage();
            if (DarkGrid.Visibility == Visibility.Visible)
            {
                package.SetText(outputTextBox.Text);
                Clipboard.SetContent(package);
            }
            else
            {
                package.SetText(outputTextBoxL.Text);
                Clipboard.SetContent(package);
            }
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DarkGrid.Visibility == Visibility.Visible)
            {
                inputTextBox.Text = "";
                outputTextBox.Text = "";
            }
            else
            {
                inputTextBoxL.Text = "";
                outputTextBoxL.Text = "";
            }
        }

        private async void RegenButton_Click(object sender, RoutedEventArgs e)
        {

            string text = inputTextBox.Text;
            string fromLang = ((ComboBoxItem)fromLanguageComboBox.SelectedItem).Content.ToString();
            string toLang = ((ComboBoxItem)toLanguageComboBox.SelectedItem).Content.ToString();

            string translation = await Translate(text, fromLang, toLang, "default_prompt");
            if (DarkGrid.Visibility == Visibility.Visible)
            {
                outputTextBox.Text = translation;
            }
            else
            {
                outputTextBoxL.Text = translation;
            }

        }

        private async void ReportButton_Click(Object sender, RoutedEventArgs e)
        {
            WebWindow webWindow = new WebWindow();
            webWindow.Show();
        }

        private async void ThemeButtonD_Click(object sender, RoutedEventArgs e)
        {
            DarkGrid.Visibility = Visibility.Collapsed;
            LightGrid.Visibility = Visibility.Visible;
            inputTextBoxL.Text = String.Empty;
            outputTextBoxL.Text = String.Empty;
            inputTextBoxL.Text += inputTextBox.Text;
            outputTextBoxL.Text += outputTextBox.Text;
        }

        private async void ThemeButtonL_Click(object sender, RoutedEventArgs e)
        {
            DarkGrid.Visibility = Visibility.Visible;
            LightGrid.Visibility = Visibility.Collapsed;
            inputTextBox.Text = String.Empty;
            outputTextBox.Text = String.Empty;
            inputTextBox.Text = inputTextBoxL.Text;
            outputTextBox.Text = outputTextBoxL.Text;
        }


        public async Task<string> Translate(string text, string fromLang, string toLang, string promptName)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prompts.json");
            string json = File.ReadAllText(filePath);
            using JsonDocument document = JsonDocument.Parse(json);
            string prompt = document.RootElement.GetProperty(promptName).GetString();
            prompt = prompt
                .Replace("{fromLang}", fromLang)
                .Replace("{toLang}", toLang)
                .Replace("{text}", text);

            var client = new GeminiClient("AIzaSyDiBsgMGVEM6dPxYcEcuU9BTCg-In14AGA");

            var response = await client.GenerateContentAsync(
                modelId: "gemini-1.5-flash-latest",
                contents: new List<Content>
                {
                new Content
                {
                    Parts = new List<Part>
                    {
                        new Part
                        {
                            Text = prompt,
                        },
                    },
                    Role = "user",
                },
                }
            );

            return response.Candidates[0].Content.Parts[0].Text;
        }

        public async Task<string> Transcribe(string path)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prompts.json");
            string json = File.ReadAllText(filePath);
            using JsonDocument document = JsonDocument.Parse(json);
            string defaultPrompt = document.RootElement.GetProperty("transcribe_prompt").GetString();
            byte[] audioBytes = await File.ReadAllBytesAsync(path);

            var client = new GeminiClient("AIzaSyDiBsgMGVEM6dPxYcEcuU9BTCg-In14AGA");

            var response = await client.GenerateContentAsync(
                modelId: "gemini-1.5-flash-latest",
                contents: new List<Content>
                {
                new Content
                {
                    Parts = new List<Part>
                    {
                        new Part
                        {
                            InlineData = new Blob
                            {
                                Data = audioBytes,
                                MimeType = "audio/wav"
                            },
                        },
                        new Part
                        {
                            Text = defaultPrompt,
                        },
                    },
                    Role = "user",
                },
                        }
            );


            return response.Candidates[0].Content.Parts[0].Text;
        }

        public async void RecordStart()
        {
            string filePath = @"C:\Users\Roma\source\repos\Xing\Xing\audio.wav";
            var waveFormat = new WaveFormat(44100, 2);
            waveIn = new WaveInEvent
            {
                WaveFormat = waveFormat,
                BufferMilliseconds = 100
            };

            waveFile = new WaveFileWriter(filePath, waveFormat);
            waveIn.DataAvailable += (s, e) =>
            {
                waveFile.Write(e.Buffer, 0, e.BytesRecorded);
                Transcribe(filePath);
            };
            waveIn.StartRecording();
        }
        public async void RecordStop()
        {
            string filePath = @"C:\Users\Roma\source\repos\Xing\Xing\audio.wav";
            waveIn.StopRecording();
            waveIn.Dispose();
            waveFile.Dispose();
            waveFile.Close();
            if (DarkGrid.Visibility == Visibility.Visible)
            {
                string transcription = await Transcribe(filePath);
                inputTextBox.Text = transcription;
            }
            else
            {
                string transcription = await Transcribe(filePath);
                inputTextBoxL.Text = transcription;
            }
        }
    }
}

