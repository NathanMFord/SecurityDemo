using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BruteForcer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly ConcurrentQueue<string> _passwordQueue;
        private readonly HttpClient _client = new HttpClient();

        private int _threads;
        private string _server;
        private bool _broken;
        private bool _pause = true;
        private int _passwords;
        private Stopwatch _stopwatch;

        public MainWindow() {
            InitializeComponent();

            List<string> passwords = File.ReadAllLines("passwords.txt").ToList().GetRange(0, 100_000);
            passwords.Shuffle();
            PasswordIndexLbl.Content = $"Index: {passwords.IndexOf("password")}";
            _passwordQueue = new ConcurrentQueue<string>(passwords);
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
                MessageBox.Show("Paused");
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
                tasks[i] = Task.Run(BruteForceLogin, CancellationToken.None);
            }

            await Task.WhenAny(tasks);
            _stopwatch.Stop();
            if (_pause)
                return;

            MessageBox.Show($"Done -- Finished in {_stopwatch.Elapsed:g} after {_passwords} attempts");
            if (_passwordQueue.Contains("password"))
                throw new Exception("Stopped processing but didn't crack password!");

            StartBtn.Content = "Start";
            _pause = true;
        }

        private async Task BruteForceLogin() {
            while (!_broken && !_pause) {
                if (_passwordQueue.TryDequeue(out string password) == false) {
                    Console.WriteLine("Failed to dequeue password");
                    return;
                }

                _broken = await PostServer("admin", password);
                _passwords++;
            }
        }

        private async Task<bool> PostServer(string username, string password) {
            try {
                using HttpResponseMessage response =
                    await _client.GetAsync(_server + $"?username={username}&password={password}");
                using HttpContent content = response.Content;
                string data = await content.ReadAsStringAsync();

                if (data != null && bool.TryParse(data, out bool broken))
                    return broken || _broken;

                return _broken;
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
                _passwordQueue.Enqueue(password);
                return _broken;
            }
        }
    }
}