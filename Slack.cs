using AutoUpdaterDotNET;
using CefSharp;
using CefSharp.WinForms;
using Microsoft.WindowsAPICodePack.Taskbar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SlackIL
{

    public partial class Slack : Form
    {
        public ChromiumWebBrowser browser;
        public delegate void IconChange(Icon icon);
        public delegate void FlashBar(bool stop);
        public delegate void TitleChange(string title);
        public delegate void Notify(dynamic data);
        public IconChange iconChangeDelegate;
        public FlashBar flashBarDelegate;
        public TitleChange titleChangeDelegate;
        public Notify notifyDelegate;

        public void InitBrowser(string url)
        {
            if (!Cef.IsInitialized)
            {
                var settings = new CefSettings()
                {
                    AcceptLanguageList = "fr-ca",
                    CachePath = @"Cache\",
                    Locale = "fr-CA",
                    PersistSessionCookies = true,
                    PersistUserPreferences = true,

                };
                //settings.CefCommandLineArgs.Add("persist_session_cookies", "1");
                Cef.Initialize(settings);
            }


            browser = new ChromiumWebBrowser(url);
            this.Controls.Add(browser);
            browser.Top += menuStrip1.Height;

            browser.Size = new System.Drawing.Size(this.Size.Width - 10, this.Size.Height - menuStrip1.Height - 39);

            browser.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right;
            /* browser.TitleChanged += Browser_TitleChanged;*/
            browser.DownloadHandler = new DownloadHandler();
            browser.DisplayHandler = new DisplayHandler();
            //browser.ConsoleMessage += c_ThresholdReached;
            browser.LifeSpanHandler = new BrowserLifeSpanHandler();
            browser.Tag = this;

            browser.RequestHandler = new RequestHandler();
            /*
            ThumbnailToolBarButton button1 = new ThumbnailToolBarButton(Icon.FromHandle(((Bitmap)SlackIL.Icones.ResourceManager.GetObject("settings")).GetHicon()), "Paramètres");
            button1.Click += delegate
            {
                MessageBox.Show("button1 clicked");
            };

            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(this.Handle, button1);*/
        }

        /* static void c_ThresholdReached(object sender, ConsoleMessageEventArgs e)
         {
             var msg = e.Message.ToLower();

             if (msg.Contains("[COUNTS]"))
             {
                 var i =1;
             }

         }*/

        public Slack(string url)
        {
            iconChangeDelegate = new IconChange(IconChangeMethod);
            titleChangeDelegate = new TitleChange(TitleChangeMethod);
            flashBarDelegate = new FlashBar(FlashBarMethod);
            notifyDelegate = new Notify(NotifyMethod);
            InitializeComponent();
            InitBrowser(url);
        }

        public string AllreadyWarn { get; set; }

        private void NotifyMethod(dynamic js)
        {
            string no = AllreadyWarn;
            try
            {
                no = js.messageFull.ts;
            }
            catch (Exception)
            {
            }
            if (AllreadyWarn != no)
            {

                notify.BalloonTipIcon = ToolTipIcon.None;
                notify.Icon = GetIcon(js.avatar);

                var text = js.userDisplayName;

                if (((string)js.channelName)[0] != 'U')
                {
                    text += " - " + js.channelName;
                }
                else
                {
                    text += " (en privé)";
                }

                notify.ShowBalloonTip(5000, text, js.message, ToolTipIcon.None);



                /* var popupNotifier = new PopupNotifier();
                 popupNotifier.TitleFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold, GraphicsUnit.Point);
                 popupNotifier.TitleText = js.userDisplayName + " - " + js.channelName;
                 popupNotifier.ContentText = js.message;
                 //popupNotifier.HeaderColor = System.Drawing.ColorTranslator.FromHtml($"#{js.color}");
                 popupNotifier.Image = GetImage(js.avatar);
                 popupNotifier.HeaderHeight = 1;
                 popupNotifier.ContentFont = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold, GraphicsUnit.Point);
                 popupNotifier.IsRightToLeft = false;
                 popupNotifier.Scroll = false;

                 popupNotifier.Popup();*/

                AllreadyWarn = js.messageFull.ts;
            }
        }
        public Icon GetIcon(string url)
        {
            Icon img = null;
            //Icon ik = null;
            var request = WebRequest.Create(url);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                Bitmap i = new Bitmap(stream);
                img = Icon.FromHandle(i.GetHicon());
                //ik = new Icon(img, new Size(256, 256));
            }

            return img;
        }

        public Image GetImage(string url)
        {
            Image img = null;
            var request = WebRequest.Create(url);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                img = Bitmap.FromStream(stream);
            }

            return img;
        }

        public Slack()
        {
            iconChangeDelegate = new IconChange(IconChangeMethod);
            titleChangeDelegate = new TitleChange(TitleChangeMethod);
            flashBarDelegate = new FlashBar(FlashBarMethod);
            notifyDelegate = new Notify(NotifyMethod);
            InitializeComponent();
            this.Icon = (System.Drawing.Icon)SlackIL.Icones.ResourceManager.GetObject("Slack");
            InitBrowser("https://mtess-dgti.slack.com/");
        }

        private void FlashBarMethod(bool stop)
        {
            this.FlashNotification(stop);
        }

        private void TitleChangeMethod(string title)
        {
            this.Text = title.Replace("Slack", "Slack++");
        }

        private void IconChangeMethod(Icon icon)
        {
            this.Icon = icon;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Windows.Window w = new System.Windows.Window();
            w.TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo() { ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal };
            w.Loaded += delegate
            {
                Action<Object> callUpdateProgress = (o) =>
                {
                    w.TaskbarItemInfo.ProgressValue = (double)o;
                };

                Thread t = new Thread(() =>
                {
                    for (int i = 1; i <= 10; i++)
                    {
                        w.Dispatcher.BeginInvoke(callUpdateProgress, 1.0 * i / 10);
                        Thread.Sleep(1000);
                    }
                });
                t.Start();
            };




            var updateURL = "http://slack.infologique.net/update/update.xml";
            AutoUpdater.ShowSkipButton = true;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Start(updateURL);

            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 60 * 60 * 1000,
                SynchronizingObject = this
            };
            timer.Elapsed += delegate
            {
                AutoUpdater.Start(updateURL);
            };
            timer.Start();

            //Icon badge = null;



            /*var v = ((Bitmap)Icones.ResourceManager.GetObject($"n1"));//.GetHicon();

            v = new Bitmap(v, new System.Drawing.Size(175,175));

            //var v2 = MergeTwoImages(Icones.app_256, v);

            badge = Icon.FromHandle(v2.GetHicon());*/

            //this.Icon = badge;
        }




        public class BrowserLifeSpanHandler : ILifeSpanHandler
        {
            public bool OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName,
                WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo,
                IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
            {

                /*
                                var request = (HttpWebRequest)WebRequest.Create(targetUrl);
                                request.Method = "GET";
                                using (var response = (HttpWebResponse)request.GetResponse())
                                {

                                    if (response.ContentType.Contains("html"))
                                    {*/
                try
                {
                    Process.Start("chrome.exe", targetUrl);
                }
                catch (Exception)
                {
                    Process.Start(targetUrl);
                }
                /*   }
               }*/


                /*  var t = new SlackExternal(targetUrl);

              newBrowser = t.browser;
              t.Show();*/
                /*try
                {
                    Process.Start("chrome.exe", targetUrl);
                }
                catch(Exception)
                {
                    Process.Start(targetUrl);
                }
*/
                newBrowser = null;
                return true;
            }

            public void OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
            {
                //
            }

            public bool DoClose(IWebBrowser browserControl, IBrowser browser)
            {
                return false;
            }

            public void OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
            {
                //nothing
            }
        }

        public class DownloadHandler : IDownloadHandler
        {

            public event EventHandler<DownloadItem> OnBeforeDownloadFired;
            public event EventHandler<DownloadItem> OnDownloadUpdatedFired;
            private bool stop;
            private DownloadWindow window;

            public void OnBeforeDownload(IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
            {
                var handler = OnBeforeDownloadFired;
                if (handler != null)
                {
                    handler(this, downloadItem);
                }

                if (!callback.IsDisposed)
                {
                    using (callback)
                    {
                        callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                    }
                }
                window = new DownloadWindow(downloadItem.TotalBytes, downloadItem.SuggestedFileName, this);
                window.Show();
            }

            public void OnDownloadUpdated(IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
            {
                var handler = OnDownloadUpdatedFired;
                if (handler != null)
                {
                    handler(this, downloadItem);
                }

                if (window != null)
                {
                    window.UpdateProgress(downloadItem.ReceivedBytes);
                }

                if (downloadItem.IsComplete)
                    window.Close();

                if (stop)
                {
                    callback.Cancel();
                }
            }

            public void StopDownload()
            {
                stop = true;
            }

            public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
            {
                //  throw new NotImplementedException();
            }

            public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
            {
                ///throw new NotImplementedException();
            }
        }


        internal class DisplayHandler : IDisplayHandler
        {


            public void OnAddressChanged(IWebBrowser browserControl, AddressChangedEventArgs addressChangedArgs)
            {

            }

            public bool OnAutoResize(IWebBrowser browserControl, IBrowser browser, CefSharp.Structs.Size newSize)
            {
                return true;
            }

            public static Bitmap MergeTwoImages(Image firstImage, Image secondImage)
            {
                if (firstImage == null)
                {
                    throw new ArgumentNullException("firstImage");
                }

                if (secondImage == null)
                {
                    throw new ArgumentNullException("secondImage");
                }

                int outputImageWidth = firstImage.Width > secondImage.Width ? firstImage.Width : secondImage.Width;
                int outputImageHeight = firstImage.Height > secondImage.Height ? firstImage.Height : secondImage.Height;

                //int outputImageHeight = firstImage.Height + secondImage.Height + 1;

                Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (Graphics graphics = Graphics.FromImage(outputImage))
                {
                    graphics.DrawImage(firstImage, new Rectangle(new System.Drawing.Point(), firstImage.Size), new Rectangle(new System.Drawing.Point(), firstImage.Size), GraphicsUnit.Pixel);
                    graphics.DrawImage(secondImage, new Rectangle(new System.Drawing.Point((firstImage.Size.Width / 2) - secondImage.Size.Width / 2, (firstImage.Size.Height / 2) - secondImage.Size.Height / 2), secondImage.Size), new Rectangle(new System.Drawing.Point(), secondImage.Size), GraphicsUnit.Pixel);
                }

                return outputImage;
            }

            public void LancerNotification(ChromiumWebBrowser browser, string channelId)
            {
                var form = ((Slack)(browser.Tag));
                //Obtenir le nom du channel : 
                //    TS.interop.redux.models.channels.getChannelById(TS.redux.getState(), "D9WB9N406").name
                //Obtenir l'utilisateur : 
                //    TS.interop.redux.models.members.getMemberById(TS.redux.getState(), "U9V09266M").profile.display_name
                //Obtenir le message : 
                //    TS.interop.redux.models.messages.getMessagesByChannelId(TS.redux.getState(), "D9WB9N406")["1522155233.000036"].text
                //Obtenir le dernier message (timestamp)
                //    TS.interop.redux.models.notifications.getMaxTimestamp(TS.redux.getState())

                //Avatar :
                //     TS.interop.redux.models.members.getMemberById(TS.redux.getState(), "U9V09266M").profile.image_24 (32,48,72,192,512,1024)

                //Color :
                //     TS.interop.redux.models.members.getMemberById(TS.redux.getState(), "U9V09266M").color

                Task<JavascriptResponse> task2 = null;
                try
                {
                    var script = @"       (function() {

                                            function findVal(object, key) {
                                                var value;
                                                Object.keys(object).some(function(k) {
                                                    if (k === key) {
                                                        value = object[k];
                                                        return true;
                                                    }
                                                    if (object[k] && typeof object[k] === 'object') {
                                                        value = findVal(object[k], key);
                                                        return value !== undefined;
                                                    }
                                                });
                                                return value;
                                            }

                                                var timestamp = slackDebug.activeTeam.redux.getState().notifications.maxTs;
                                                var message = findVal(slackDebug.activeTeam.redux.getState().messages, timestamp);
                                                var text = message.text;
                                                var userId = message.user;
                                                var channel = slackDebug.activeTeam.redux.getState().channels[message.channel];
                                                var channelName = channel.name;
                                             
                                                var member = slackDebug.activeTeam.redux.getState().members[message.user];
                                                var color = member.color;
                                                var userDisplayName = member.profile.display_name;
                                                var avatar = member.profile.image_72;

                                                return {""messageFull"": message, ""userDisplayName"" : userDisplayName, ""message"" : text, ""channelName"": channelName, ""color"": color, ""avatar"" : avatar}; 
                                            }
                                   )();";
                    task2 = browser?.GetMainFrame().EvaluateScriptAsync(script, null);
                }
                catch (Exception) { }


                task2?.ContinueWith(t =>
                {
                    try
                    {
                        if (!t.IsFaulted)
                        {
                            var response = t.Result;
                            var js = (dynamic)response.Result;

                            if (js != null)
                            {

                                form.Invoke(form.notifyDelegate, js);

                            }

                        }

                    }
                    catch (Exception) { }

                });
            }



            public int Unreads { get; set; } = -1;

            public void MettreAjourNbMessage(ChromiumWebBrowser browser)
            {
                var form = ((Slack)(browser.Tag));
                Task<JavascriptResponse> task2 = null;
                try
                {
                    task2 = browser?.GetMainFrame().EvaluateScriptAsync(@"(
                                function() { 
                                    return {""unread_count"" : slackDebug.activeTeam.redux.getState().unreadCounts.totalUnreads}; 
                                 })();", null);
                }
                catch (Exception) { }


                task2?.ContinueWith(t =>
                {
                    try
                    {
                        if (!t.IsFaulted)
                        {
                            var response = t.Result;
                            var evaluateJavaScriptResult = (dynamic)response.Result;

                            if (evaluateJavaScriptResult != null && evaluateJavaScriptResult.unread_count != 0)
                            {
                                if (Unreads != evaluateJavaScriptResult.unread_count)
                                {
                                    Unreads = evaluateJavaScriptResult.unread_count;
                                    Icon badge = null;
                                    var txt = (Unreads > 9) ? "X" : Unreads.ToString();

                                    var v = ((Bitmap)Icones.ResourceManager.GetObject($"n{txt}"));//.GetHicon();

                                    v = new Bitmap(v, new System.Drawing.Size(180, 180));

                                    //var v2 = MergeTwoImages(Icones.app_256, v);

                                    var v2 = MergeTwoImages(ResizeImage(Icones.Slack.ToBitmap(), 256, 256), v);

                                    badge = Icon.FromHandle(v2.GetHicon());

                                    //TaskbarManager.Instance.SetOverlayIcon(badge, evaluateJavaScriptResult.ToString());
                                    form.Invoke(form.flashBarDelegate, false);
                                    form.Invoke(form.iconChangeDelegate, badge);
                                }
                            }
                            else
                            {
                                if (Unreads != 0)
                                {
                                    Unreads = 0;
                                    form.Invoke(form.iconChangeDelegate, Icones.ResourceManager.GetObject($"Slack"));
                                    //TaskbarManager.Instance.SetOverlayIcon(null, null);
                                    form.Invoke(form.flashBarDelegate, true);
                                }
                            }

                        }

                    }
                    catch (Exception) { }

                });

                Bitmap ResizeImage(Image image, int width, int height)
                {
                    var destRect = new Rectangle(0, 0, width, height);
                    var destImage = new Bitmap(width, height);

                    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

                    using (var graphics = Graphics.FromImage(destImage))
                    {
                        graphics.CompositingMode = CompositingMode.SourceCopy;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        using (var wrapMode = new ImageAttributes())
                        {
                            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                        }
                    }

                    return destImage;
                }

            }



            public bool OnConsoleMessage(IWebBrowser browserControl, ConsoleMessageEventArgs consoleMessageArgs)
            {
                var mots = consoleMessageArgs.Message.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                var estUnread = mots.Contains("[COUNTS]");

                if (mots.Contains("[COUNTS]") && mots.Contains("unread_cnt"))
                {
                    var i = 0;
                    foreach (var item in mots)
                    {
                        i++;
                        if (item == "for")
                            break;
                    }

                    var channel = mots[i].Trim(':');

                    Task.Delay(600).ContinueWith(t => LancerNotification((ChromiumWebBrowser)browserControl, channel));
                }

                Task.Delay(600).ContinueWith(t => MettreAjourNbMessage((ChromiumWebBrowser)browserControl));

                //    var form = ((Form1)(((ChromiumWebBrowser)browserControl).Tag));

                //var task = ((ChromiumWebBrowser)browserControl).GetMainFrame().EvaluateScriptAsync("(function() { return TS.model.all_unread_cnt; })();", null);

                //Icon icon = data.ToIcon();






                return true;
            }

            public void OnFaviconUrlChange(IWebBrowser browserControl, IBrowser browser, IList<string> urls)
            {
                /* string iconP;
                 if (urls.Count > 1)
                 {
                     iconP = urls[1];
                     byte[] data = null;
                     if (iconP.StartsWith("data:"))
                     {
                         iconP = iconP.Replace("data:image/png;base64,", string.Empty);
                         data = Convert.FromBase64String(iconP);
                     }
                     else
                     {
                         using (WebClient webClient = new WebClient())
                         {
                             data = webClient.DownloadData(iconP);
                         }
                     }

                     var form = ((Form1)(((ChromiumWebBrowser)browserControl).Tag));

                     var task = ((ChromiumWebBrowser)browserControl).GetMainFrame().EvaluateScriptAsync("(function() { return TS.model.all_unread_cnt; })();", null);


                     Icon icon = data.ToIcon();

                     task.ContinueWith(t =>
                     {
                         if (!t.IsFaulted)
                         {
                             var response = t.Result;
                             var evaluateJavaScriptResult = (response.Success ? (response.Result ?? "null") : response.Message).ToString();

                             if (!string.IsNullOrWhiteSpace(evaluateJavaScriptResult.ToString()) && evaluateJavaScriptResult != "0")
                             {
                                 Icon badge = null;

                                 var v = ((Bitmap)Icones.ResourceManager.GetObject($"n{evaluateJavaScriptResult.ToString()}")).GetHicon();

                                 badge = Icon.FromHandle(v);

                                 TaskbarManager.Instance.SetOverlayIcon(badge, evaluateJavaScriptResult.ToString());
                             }

                         }
                     });

                     form.Invoke(form.iconChangeDelegate, icon);


                 } */
            }

            public void OnFullscreenModeChange(IWebBrowser browserControl, IBrowser browser, bool fullscreen)
            {

            }

            public void OnStatusMessage(IWebBrowser browserControl, StatusMessageEventArgs statusMessageArgs)
            {
                // var r = 0;

            }

            public void OnTitleChanged(IWebBrowser browserControl, TitleChangedEventArgs titleChangedArgs)
            {
                var form = ((Slack)(((ChromiumWebBrowser)browserControl).Tag));
                form?.Invoke(form.titleChangeDelegate, titleChangedArgs.Title);
            }

            public bool OnTooltipChanged(IWebBrowser browserControl, ref string text)
            {
                return true;
            }

            public void OnLoadingProgressChange(IWebBrowser chromiumWebBrowser, IBrowser browser, double progress)
            {

            }
        }

        private void notify_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.Show();
            this.BringToFront();
        }

        private void notify_BalloonTipClicked(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.Show();
            this.BringToFront();
        }

    }

    internal class RequestHandler : IRequestHandler
    {
        public bool CanGetCookies(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return true;
            //  throw new NotImplementedException();
        }

        public bool CanSetCookie(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, CefSharp.Cookie cookie)
        {
            return true; // throw new NotImplementedException();
        }

        /* public bool GetAuthCredentials(IWebBrowser browserControl, IBrowser browser, IFrame frame, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
         {
             return false; //throw new NotImplementedException();
         }*/

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            return false;// throw new NotImplementedException();
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return null; // throw new NotImplementedException();
        }

        public IResponseFilter GetResourceResponseFilter(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return null;// throw new NotImplementedException();
        }

        public bool OnBeforeBrowse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, bool isRedirect)
        {
            return false; //  throw new NotImplementedException();
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            return false;
            //throw new NotImplementedException();
        }

        public CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;// throw new NotImplementedException();
        }

        public bool OnCertificateError(IWebBrowser browserControl, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            return true; // throw new NotImplementedException();
        }

        public bool OnOpenUrlFromTab(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public void OnPluginCrashed(IWebBrowser browserControl, IBrowser browser, string pluginPath)
        {

        }

        public bool OnProtocolExecution(IWebBrowser browserControl, IBrowser browser, string url)
        {
            return false;
        }

        public bool OnQuotaRequest(IWebBrowser browserControl, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            return false;
        }

        public void OnRenderProcessTerminated(IWebBrowser browserControl, IBrowser browser, CefTerminationStatus status)
        {

        }

        public void OnRenderViewReady(IWebBrowser browserControl, IBrowser browser)
        {
            //browserControl.ShowDevTools();

            var form = ((Slack)(((ChromiumWebBrowser)browserControl).Tag));
            Task<JavascriptResponse> task2 = null;
           /* try
            {
                browserControl.ShowDevTools();
                /* browserControl?.GetMainFrame().ExecuteJavaScriptAsync(SlackIL.Icones.wsHook);
                 browserControl?.GetMainFrame().ExecuteJavaScriptAsync(@"wsHook.before = function(data, url) 
                                                                                 console.log(""Sending message to "" + url + "" : "" + data);}");
                //browserControl.ShowDevTools();
                //task2 = browserControl?.GetMainFrame().EvaluateScriptAsync("(function() { return TS.model.all_unread_cnt; })();", null);
            }
            catch (Exception) { }*/


            task2?.ContinueWith(t =>
            {
                try
                {
                    if (!t.IsFaulted)
                    {
                        var response = t.Result;
                        var evaluateJavaScriptResult = (response.Success ? (response.Result ?? "null") : response.Message).ToString();

                        if (!string.IsNullOrWhiteSpace(evaluateJavaScriptResult.ToString()) && evaluateJavaScriptResult != "0")
                        {
                            Icon badge = null;

                            if (int.Parse(evaluateJavaScriptResult) > 9)
                                evaluateJavaScriptResult = "X";

                            var v = ((Bitmap)Icones.ResourceManager.GetObject($"n{evaluateJavaScriptResult.ToString()}")).GetHicon();

                            badge = Icon.FromHandle(v);

                            TaskbarManager.Instance.SetOverlayIcon(badge, evaluateJavaScriptResult.ToString());
                            // TaskbarItemInfo.Overlay = badge;
                            form.Invoke(form.flashBarDelegate, false);
                        }
                        else
                        {
                            TaskbarManager.Instance.SetOverlayIcon(null, null);
                            form.Invoke(form.flashBarDelegate, true);
                        }

                    }

                }
                catch (Exception) { }

            });
        }

        public void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {

        }

        public void OnResourceRedirect(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {

        }

        public bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return false;
        }

        public bool OnSelectClientCertificate(IWebBrowser browserControl, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            return false;
        }
    }
}
