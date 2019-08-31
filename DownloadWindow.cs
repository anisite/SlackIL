using CefSharp;
using System.Windows.Forms;

namespace SlackIL
{
    internal class DownloadWindow : Form
    {
        private long totalBytes;
        private string suggestedFileName;
        private IDownloadHandler downloadHandler;

        public DownloadWindow(long totalBytes, string suggestedFileName, IDownloadHandler downloadHandler)
        {
            this.totalBytes = totalBytes;
            this.suggestedFileName = suggestedFileName;
            this.downloadHandler = downloadHandler;
        }

        internal void UpdateProgress(long receivedBytes)
        {
            //throw new NotImplementedException();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DownloadWindow
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "DownloadWindow";
            this.Load += new System.EventHandler(this.DownloadWindow_Load);
            this.ResumeLayout(false);

        }

        private void DownloadWindow_Load(object sender, System.EventArgs e)
        {

        }
    }
}