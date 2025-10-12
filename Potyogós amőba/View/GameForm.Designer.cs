namespace Potyogós_amőba
{
    partial class GameForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            újJátékToolStripMenuItem = new ToolStripMenuItem();
            pauseGameToolStripMenuItem = new ToolStripMenuItem();
            saveGameToolStripMenuItem = new ToolStripMenuItem();
            betöltéséreToolStripMenuItem = new ToolStripMenuItem();
            colorDialog1 = new ColorDialog();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { újJátékToolStripMenuItem, pauseGameToolStripMenuItem, saveGameToolStripMenuItem, betöltéséreToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(46, 24);
            fileToolStripMenuItem.Text = "File";
            // 
            // újJátékToolStripMenuItem
            // 
            újJátékToolStripMenuItem.Name = "újJátékToolStripMenuItem";
            újJátékToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            újJátékToolStripMenuItem.Size = new Size(265, 26);
            újJátékToolStripMenuItem.Text = "Új játék";
            újJátékToolStripMenuItem.Click += newGameToolStripMenuItem_Click;
            // 
            // játékSzüneteltetéséreToolStripMenuItem
            // 
            pauseGameToolStripMenuItem.Name = "játékSzüneteltetéséreToolStripMenuItem";
            pauseGameToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.P;
            pauseGameToolStripMenuItem.Size = new Size(265, 26);
            pauseGameToolStripMenuItem.Text = "Játék szüneteltetés";
            pauseGameToolStripMenuItem.Click += pauseGameToolStripMenuItem_Click;
            // 
            // játékMentéséreToolStripMenuItem
            // 
            saveGameToolStripMenuItem.Name = "játékMentéséreToolStripMenuItem";
            saveGameToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            saveGameToolStripMenuItem.Size = new Size(265, 26);
            saveGameToolStripMenuItem.Text = "Játék mentés";
            saveGameToolStripMenuItem.Click += saveGameToolStripMenuItem_Click;
            // 
            // betöltéséreToolStripMenuItem
            // 
            betöltéséreToolStripMenuItem.Name = "betöltéséreToolStripMenuItem";
            betöltéséreToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.L;
            betöltéséreToolStripMenuItem.Size = new Size(265, 26);
            betöltéséreToolStripMenuItem.Text = "Játék betöltés";
            betöltéséreToolStripMenuItem.Click += loadGameToolStripMenuItem_Click;
            // 
            // GameForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "GameForm";
            Text = "Potyogós amőba";
            Load += GameForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem újJátékToolStripMenuItem;
        private ToolStripMenuItem pauseGameToolStripMenuItem;
        private ToolStripMenuItem saveGameToolStripMenuItem;
        private ToolStripMenuItem betöltéséreToolStripMenuItem;
        private ColorDialog colorDialog1;
    }
}
