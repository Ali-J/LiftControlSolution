namespace LiftGUI
{
    partial class Form1
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
            this.UpReqBtn0 = new System.Windows.Forms.Button();
            this.UpReqBtn1 = new System.Windows.Forms.Button();
            this.DownReqBtn1 = new System.Windows.Forms.Button();
            this.DownReqBtn2 = new System.Windows.Forms.Button();
            this.LiftPanel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // UpReqBtn0
            // 
            this.UpReqBtn0.Location = new System.Drawing.Point(12, 373);
            this.UpReqBtn0.Name = "UpReqBtn0";
            this.UpReqBtn0.Size = new System.Drawing.Size(75, 23);
            this.UpReqBtn0.TabIndex = 0;
            this.UpReqBtn0.Text = "Up";
            this.UpReqBtn0.UseVisualStyleBackColor = true;
            this.UpReqBtn0.Click += new System.EventHandler(this.UpReqBtn0_Click);
            // 
            // UpReqBtn1
            // 
            this.UpReqBtn1.Location = new System.Drawing.Point(12, 265);
            this.UpReqBtn1.Name = "UpReqBtn1";
            this.UpReqBtn1.Size = new System.Drawing.Size(75, 23);
            this.UpReqBtn1.TabIndex = 1;
            this.UpReqBtn1.Text = "Up";
            this.UpReqBtn1.UseVisualStyleBackColor = true;
            this.UpReqBtn1.Click += new System.EventHandler(this.UpReqBtn1_Click);
            // 
            // DownReqBtn1
            // 
            this.DownReqBtn1.Location = new System.Drawing.Point(12, 294);
            this.DownReqBtn1.Name = "DownReqBtn1";
            this.DownReqBtn1.Size = new System.Drawing.Size(75, 23);
            this.DownReqBtn1.TabIndex = 2;
            this.DownReqBtn1.Text = "Down";
            this.DownReqBtn1.UseVisualStyleBackColor = true;
            this.DownReqBtn1.Click += new System.EventHandler(this.DownReqBtn1_Click);
            // 
            // DownReqBtn2
            // 
            this.DownReqBtn2.Location = new System.Drawing.Point(12, 171);
            this.DownReqBtn2.Name = "DownReqBtn2";
            this.DownReqBtn2.Size = new System.Drawing.Size(75, 23);
            this.DownReqBtn2.TabIndex = 3;
            this.DownReqBtn2.Text = "Down";
            this.DownReqBtn2.UseVisualStyleBackColor = true;
            this.DownReqBtn2.Click += new System.EventHandler(this.DownReqBtn2_Click);
            // 
            // LiftPanel1
            // 
            this.LiftPanel1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.LiftPanel1.Location = new System.Drawing.Point(133, 362);
            this.LiftPanel1.Name = "LiftPanel1";
            this.LiftPanel1.Size = new System.Drawing.Size(28, 46);
            this.LiftPanel1.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 408);
            this.Controls.Add(this.LiftPanel1);
            this.Controls.Add(this.DownReqBtn2);
            this.Controls.Add(this.DownReqBtn1);
            this.Controls.Add(this.UpReqBtn1);
            this.Controls.Add(this.UpReqBtn0);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button UpReqBtn0;
        private System.Windows.Forms.Button UpReqBtn1;
        private System.Windows.Forms.Button DownReqBtn1;
        private System.Windows.Forms.Button DownReqBtn2;
        public System.Windows.Forms.Panel LiftPanel1;
    }
}

