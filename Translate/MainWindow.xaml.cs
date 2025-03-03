using Google.Gemini;
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
using System.Collections;
using Windows.ApplicationModel.DataTransfer;
using WinUIEx;


namespace Translate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

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
            string text;
            string fromLang = ((ComboBoxItem)fromLanguageComboBox.SelectedItem).Content.ToString();
            string toLang = ((ComboBoxItem)toLanguageComboBox.SelectedItem).Content.ToString();

            if (DarkGrid.Visibility == Visibility.Visible)
            {
                text = inputTextBox.Text;
                string translation = await Translate(text, fromLang, toLang);
                outputTextBox.Text = translation;
            }
            else
            {
                text = inputTextBoxL.Text;
                string translation = await Translate(text, fromLang, toLang);
                outputTextBoxL.Text = translation;
            }
        }

        public async Task<string> Translate(string text, string fromLang, string toLang)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "prompts.json");
            string json = File.ReadAllText(filePath);
            using JsonDocument document = JsonDocument.Parse(json);
            string defaultPrompt = document.RootElement.GetProperty("").GetString();
            string prompt = defaultPrompt
                .Replace("{fromLang}", fromLang)
                .Replace("{toLang}", toLang)
                .Replace("{text}", text);

            var client = new GeminiClient("YOUR API KEY HERE");

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


        private void InputCopy_Click(object sender, RoutedEventArgs e)
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

        private void OutputCopy_Click(object sender, RoutedEventArgs e)
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

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
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

            string translation = await Translate(text, fromLang, toLang);
            if (DarkGrid.Visibility == Visibility.Visible)
            {
                outputTextBox.Text = translation;
            }
            else
            {
                outputTextBoxL.Text = translation;
            }

        }

        private void ThemeButtonD_Click(object sender, RoutedEventArgs e) 
        {
            DarkGrid.Visibility = Visibility.Collapsed;
            LightGrid.Visibility = Visibility.Visible;
        }

        private void ThemeButtonL_Click(object sender, RoutedEventArgs e)
        {
            DarkGrid.Visibility = Visibility.Visible;
            LightGrid.Visibility = Visibility.Collapsed;
        }

    }
}
