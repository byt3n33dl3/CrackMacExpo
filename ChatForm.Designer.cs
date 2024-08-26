namespace Update_Service
{
    partial class ChatForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatForm));
            label1 = new Label();
            messagebox = new RichTextBox();
            closebutton = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            label1.Location = new Point(12, 19);
            label1.Name = "label1";
            label1.Size = new Size(116, 25);
            label1.TabIndex = 0;
            label1.Text = "Hacker Doll:";
            // 
            // messagebox
            // 
            messagebox.BackColor = Color.Plum;
            messagebox.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            messagebox.Location = new Point(90, 63);
            messagebox.Name = "messagebox";
            messagebox.Size = new Size(523, 227);
            messagebox.TabIndex = 1;
            messagebox.Text = "";
            // 
            // closebutton
            // 
            closebutton.FlatAppearance.BorderColor = Color.Yellow;
            closebutton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            closebutton.Location = new Point(287, 322);
            closebutton.Name = "closebutton";
            closebutton.Size = new Size(112, 34);
            closebutton.TabIndex = 2;
            closebutton.Text = "Close";
            closebutton.UseVisualStyleBackColor = true;
            closebutton.Click += closebutton_Click;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DeepPink;
            ClientSize = new Size(700, 422);
            ControlBox = false;
            Controls.Add(closebutton);
            Controls.Add(messagebox);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ChatForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ChatForm";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button closebutton;
        public RichTextBox messagebox;
    }
}