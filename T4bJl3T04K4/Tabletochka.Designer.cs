namespace T4bJl3T04K4
{
    partial class Tabletochka
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Tabletochka));
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            SuspendLayout();
            // 
            // webView
            // 
            webView.AllowExternalDrop = true;
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Dock = DockStyle.Fill;
            webView.Location = new Point(0, 0);
            webView.Margin = new Padding(6, 6, 6, 6);
            webView.Name = "webView";
            webView.Size = new Size(1733, 1107);
            webView.TabIndex = 0;
            webView.ZoomFactor = 1D;
            webView.WebMessageReceived += WebView_WebMessageReceived;
            // 
            // Tabletochka
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1733, 1107);
            Controls.Add(webView);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(7, 9, 7, 9);
            Name = "Tabletochka";
            Text = "Tabletochka";
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
    }
}

