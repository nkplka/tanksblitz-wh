namespace WotBlitz
{
	
	public partial class Form1 : global::System.Windows.Forms.Form
	{
		
		protected override void Dispose(bool disposing)
		{
			bool flag = disposing && this.components != null;
			if (flag)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		
		private void InitializeComponent()
		{
            this.btn_attatch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            
            this.btn_attatch.Location = new System.Drawing.Point(10, 13);
            this.btn_attatch.Name = "btn_attatch";
            this.btn_attatch.Size = new System.Drawing.Size(193, 25);
            this.btn_attatch.TabIndex = 0;
            this.btn_attatch.Text = "wh dlya pidorov";
            this.btn_attatch.UseVisualStyleBackColor = true;
            this.btn_attatch.Click += new System.EventHandler(this.btn_attatch_Click);
            
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(213, 45);
            this.Controls.Add(this.btn_attatch);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "nkpl1337";
            this.ResumeLayout(false);

		}

		
		private global::System.ComponentModel.IContainer components = null;

		
		private global::System.Windows.Forms.Button btn_attatch;
	}
}
