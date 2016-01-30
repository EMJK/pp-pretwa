namespace Pretwa.Gui
{
    partial class MainForm
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.btn_SinglePlayer = new System.Windows.Forms.Button();
            this.btn_MultiPlayer = new System.Windows.Forms.Button();
            this.visualState1 = new Pretwa.Gui.VisualState();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.visualState1);
            this.panel1.Location = new System.Drawing.Point(33, 117);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(700, 700);
            this.panel1.TabIndex = 1;
            // 
            // btn_SinglePlayer
            // 
            this.btn_SinglePlayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btn_SinglePlayer.Location = new System.Drawing.Point(35, 12);
            this.btn_SinglePlayer.Name = "btn_SinglePlayer";
            this.btn_SinglePlayer.Size = new System.Drawing.Size(308, 99);
            this.btn_SinglePlayer.TabIndex = 2;
            this.btn_SinglePlayer.Text = "JEDEN GRACZ";
            this.btn_SinglePlayer.UseVisualStyleBackColor = true;
            this.btn_SinglePlayer.Click += new System.EventHandler(this.btn_SinglePlayer_Click);
            // 
            // btn_MultiPlayer
            // 
            this.btn_MultiPlayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btn_MultiPlayer.Location = new System.Drawing.Point(425, 12);
            this.btn_MultiPlayer.Name = "btn_MultiPlayer";
            this.btn_MultiPlayer.Size = new System.Drawing.Size(308, 99);
            this.btn_MultiPlayer.TabIndex = 2;
            this.btn_MultiPlayer.Text = "DWOJE GRACZY";
            this.btn_MultiPlayer.UseVisualStyleBackColor = true;
            this.btn_MultiPlayer.Click += new System.EventHandler(this.btn_MultiPlayer_Click);
            // 
            // visualState1
            // 
            this.visualState1.CurrentPlayer = null;
            this.visualState1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.visualState1.Location = new System.Drawing.Point(0, 0);
            this.visualState1.Name = "visualState1";
            this.visualState1.Size = new System.Drawing.Size(696, 696);
            this.visualState1.TabIndex = 0;
            this.visualState1.Text = "visualState1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(773, 829);
            this.Controls.Add(this.btn_MultiPlayer);
            this.Controls.Add(this.btn_SinglePlayer);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Pretwa";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private VisualState visualState1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_SinglePlayer;
        private System.Windows.Forms.Button btn_MultiPlayer;
    }
}