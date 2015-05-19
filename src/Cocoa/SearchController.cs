using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using NLog;
using Radish.Support;
using System.Collections.Generic;
using Radish.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Threading;
using Rangic.Utilities.Preferences;
using Rangic.Utilities.Geo;

namespace Radish
{
    [MonoMac.Foundation.Register("SearchController")]
    public partial class SearchController : NSViewController
    {
        static private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private FindAPhotoClient _client;
        private string _lastSavedHost;
        private bool _allowFocusChange;
        private bool _isSearching;
        private bool _cancelSearch;

        public IList<MediaMetadata> SearchResults { get; private set; }

        public SearchController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export ("initWithCoder:")]
        public SearchController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        void Initialize()
        {
            _client = new FindAPhotoClient();
        }

        private void SetUiToStart()
        {
            _cancelSearch = false;
            _isSearching = false;
            progressIndicator.Hidden = true;
            errorLabel.StringValue = "";
            _lastSavedHost = hostName.StringValue = Preferences<RadishPreferences>.Instance.FindAPhotoHost;
            connectionImage.Image = null;
            _allowFocusChange = true;
        }

        public void RunModal(NSWindow window)
        {
            SetUiToStart();

            window.DidBecomeKey -= DidBecomeKey;
            window.DidBecomeKey += DidBecomeKey;
            window.WillClose -= WindowWillClose;
            window.WillClose += WindowWillClose;
            NSApplication.SharedApplication.RunModalForWindow(window);
        }

        partial void cancel(NSObject sender)
        {
            logger.Info("Cancel");
            if (IsSearching())
            {
                _cancelSearch = true;
            }
            else
            {
                SearchResults = null;
                CloseSearch(sender);
            }
        }

        partial void startSearch(NSObject sender)
        {
            _client.Host = hostName.StringValue;
            var query = searchText.StringValue;
            logger.Info("search '{0}' for '{1}'", _client.Host, query);

            SearchResults = new List<MediaMetadata>();
            SearchHasStarted();

            Task.Run( () =>
            {
                try
                {
                    _client.ShouldCancel = () => !IsSearching();
                    _client.Search(query, HandleMatch);
                }
                catch (Exception e)
                {
                    logger.Error("Search exception: {0}", e);
                }

                BeginInvokeOnMainThread( () => 
                {
                    var wasCanceled = _cancelSearch;
                    SearchHasEnded();
                    UpdateHost();

                    if (wasCanceled)
                    {
                        errorLabel.StringValue = "Search was canceled";
                        connectionImage.Image = NSImage.ImageNamed("FailedCheck.png");
                    }
                    else
                    {
                        UpdateUiError();

                        if (!_client.HasError)
                        {
                            logger.Info("Found {0} matches", SearchResults.Count);
                            CloseSearch(sender);
                        }
                    }
                });
            });
        }

        private void ShowProgressIndicator()
        {
            // To ensure 'SearchHasEnded' isn't called while we are setting up. And to do UI things, of course.
            BeginInvokeOnMainThread( () =>
            {
                progressIndicator.Hidden = false;
            });
        }

        private void SearchHasStarted()
        {
            _isSearching = true;
            _cancelSearch = false;
            progressIndicator.DoubleValue = 0;

            errorLabel.StringValue = "";
            connectionImage.Image = null;

            var timer = new System.Timers.Timer(100) { AutoReset = false, Enabled = true, };
            timer.Elapsed += (s, e) => ShowProgressIndicator();

            searchButton.Enabled = false;
            searchText.Enabled = false;
            testButton.Enabled = false;
            hostName.Enabled = false;
        }

        private void SearchHasEnded()
        {
            progressIndicator.Hidden = true;
            _isSearching = false;

            searchButton.Enabled = true;
            searchText.Enabled = true;
            testButton.Enabled = true;
            hostName.Enabled = true;
        }

        private bool IsSearching()
        {
            return _isSearching && !_cancelSearch;
        }

        partial void testHost(NSObject sender)
        {
            _client.Host = hostName.StringValue;
            logger.Info("testHost: '{0}'", _client.Host);

            _client.TestConnection();
            UpdateUiError();
            UpdateHost();
        }

        private void HandleMatch(int totalMatches, int visiting, dynamic match)
        {
            if (!IsSearching())
            {
                return;
            }

            int percent = (100 * visiting) / totalMatches;
            BeginInvokeOnMainThread( () =>
            {
                progressIndicator.DoubleValue = percent;
            });

            var mimeType = match["mimeType"].ToString();
            if (mimeType != null && mimeType.StartsWith("image"))
            {
                Location location = null;
                if (match["latitude"].Type == JTokenType.Float && match["longitude"].Type == JTokenType.Float)
                {
                    location = new Location((double) match["latitude"], (double) match["longitude"]);
                }

                DateTime createdDate = match["createdDate"];
                var item = new FindAPhotoMetadata(
                    _client.Host + "/" + match["fullUrl"].ToString(),
                    createdDate,
                    location,
                    match["keywords"].ToObject<string[]>(),
                    _client.Host + "/" + match["thumbUrl"].ToString());
                SearchResults.Add(item);
            }
        }

        private void CloseSearch(NSObject sender)
        {
            var view = sender as NSView;
            if (sender != null)
            {
                view.Window.PerformClose(sender);
            }
            else
            {
                logger.Info("Unable to find window to close: {0}", sender);
            }
        }

        private void UpdateHost()
        {
            if (_lastSavedHost != _client.Host)
            {
                Preferences<RadishPreferences>.Instance.FindAPhotoHost = _client.Host;
                Preferences<RadishPreferences>.Save();

                _lastSavedHost = _client.Host;
            }
        }

        private void UpdateUiError()
        {
            if (_client.HasError)
            {
                errorLabel.StringValue = _client.LastError;
                connectionImage.Image = NSImage.ImageNamed("FailedCheck.png");
            }
            else
            {
                errorLabel.StringValue = "";
                connectionImage.Image = NSImage.ImageNamed("SucceededCheck.png");
            }
        }

        private void DidBecomeKey(object sender, EventArgs args)
        {
            if (_allowFocusChange && !String.IsNullOrWhiteSpace(hostName.StringValue))
            {
                var notification = sender as NSNotification;
                if (notification != null)
                {
                    var window = notification.Object as NSWindow;
                    if (window != null)
                    {
                        window.MakeFirstResponder(searchText);
                    }
                }
            }
            _allowFocusChange = false;
        }

        private void WindowWillClose(object sender, EventArgs args)
        {
            NSApplication.SharedApplication.StopModal();
        }
    }
}
