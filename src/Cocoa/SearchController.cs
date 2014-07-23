using System;
using MonoMac.AppKit;
using MonoMac.Foundation;
using NLog;
using Radish.Support;
using System.Collections.Generic;
using Radish.Models;

namespace Radish
{
    [MonoMac.Foundation.Register("SearchController")]
    public partial class SearchController : NSViewController
    {
        static private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private FindAPhotoClient _client;
        private string _lastSavedHost;
        private bool _allowFocusChange;

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

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
        }

        private void SetUiToStart()
        {
            errorLabel.StringValue = "";
            _lastSavedHost = hostName.StringValue = Preferences.Instance.FindAPhotoHost;
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

        partial void cancel(MonoMac.Foundation.NSObject sender)
        {
            logger.Info("Cancel");
            SearchResults = null;

            CloseSearch(sender);
        }

        partial void startSearch(MonoMac.Foundation.NSObject sender)
        {
            _client.Host = hostName.StringValue;
            logger.Info("search '{0}' for '{1}'", _client.Host, searchText.StringValue);

            SearchResults = _client.Search(searchText.StringValue);
            UpdateUiError();
            UpdateHost();
            if (!_client.HasError)
            {
                logger.Info("Found {0} matches", SearchResults.Count);
                CloseSearch(sender);
            }
        }

        partial void testHost(MonoMac.Foundation.NSObject sender)
        {
            _client.Host = hostName.StringValue;
            logger.Info("testHost: '{0}'", _client.Host);

            _client.TestConnection();
            UpdateUiError();
            UpdateHost();
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
                Preferences.Instance.FindAPhotoHost = _client.Host;
                Preferences.Instance.Save();

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
