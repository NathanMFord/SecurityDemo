using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BruteForcer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly ConcurrentQueue<string> _passwordQueue;
        
        private int _threads;
        private string _server;
        private bool _broken;
        private bool _pause = true;
        private int _passwords;
        private Stopwatch _stopwatch;

        public MainWindow() {
            InitializeComponent();

            // List<string> passwords = File.ReadAllLines("passwords.txt").ToList();
            // passwords.Shuffle();
            // _passwordQueue= new ConcurrentQueue<string>(passwords);
            _passwordQueue = new ConcurrentQueue<string>(new List<string> {"test", "password"});
        }

        private async void StartBtn_OnClick(object sender, RoutedEventArgs e) {
            if (_pause) {
                StartBtn.Content = "Pause";
                _stopwatch?.Start();
                _pause = false;
            } else {
                StartBtn.Content = "Start";
                _stopwatch?.Stop();
                _pause = true;
                return;
            }

            if (int.TryParse(Threads.Text, out _threads) == false) {
                MessageBox.Show("Invalid number of threads, please enter a number!");
                return;
            }

            if (Uri.TryCreate(Server.Text.Trim(), UriKind.Absolute, out _) == false) {
                MessageBox.Show("Invalid URL for login server");
                return;
            }

            _server = Server.Text.Trim();
            
            _stopwatch = Stopwatch.StartNew();
            Task[] tasks = new Task[_threads];
            for (int i = 0; i < _threads; i++) {
                tasks[i] = Task.Run(BruteForceLogin);
            }
            
            await Task.WhenAny(tasks);
            _stopwatch.Stop();
            MessageBox.Show(_broken ? $"Done -- Finished in {_stopwatch.Elapsed:g}" : "Paused");
            StartBtn.Content = "Start";
            _pause = true;
        }

        private async Task BruteForceLogin() {
            while (!_broken && !_pause) {
                if (_passwordQueue.TryDequeue(out string password) == false)
                    return;
                _broken = await PostServer("admin", password);
                _passwords++;                    
            }
        }

        private async Task<bool> PostServer(string username, string password) {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(_server + $"?username={username}&password={password}");
            using HttpContent content = response.Content;
            string data = await content.ReadAsStringAsync();
            
            if (data != null && bool.TryParse(data, out bool broken))
                return broken;
            
            return false;
        }
    }
}