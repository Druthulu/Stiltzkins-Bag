namespace rand9er
{
    partial class DebugForm
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
            this.dir1Button = new System.Windows.Forms.Button();
            this.dir2Button = new System.Windows.Forms.Button();
            this.dir1Text = new System.Windows.Forms.TextBox();
            this.dir2Text = new System.Windows.Forms.TextBox();
            this.outputText = new System.Windows.Forms.RichTextBox();
            this.p0data2Files = new System.Windows.Forms.RadioButton();
            this.p0data2BIN = new System.Windows.Forms.RadioButton();
            this.p0data7Files = new System.Windows.Forms.RadioButton();
            this.p0data7BIN = new System.Windows.Forms.RadioButton();
            this.runButton = new System.Windows.Forms.Button();
            this.eventText = new System.Windows.Forms.RichTextBox();
            this.JsonCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // dir1Button
            // 
            this.dir1Button.Location = new System.Drawing.Point(12, 12);
            this.dir1Button.Name = "dir1Button";
            this.dir1Button.Size = new System.Drawing.Size(75, 23);
            this.dir1Button.TabIndex = 0;
            this.dir1Button.Text = "Dir1";
            this.dir1Button.UseVisualStyleBackColor = true;
            this.dir1Button.Click += new System.EventHandler(this.dir1Button_Click);
            // 
            // dir2Button
            // 
            this.dir2Button.Location = new System.Drawing.Point(12, 47);
            this.dir2Button.Name = "dir2Button";
            this.dir2Button.Size = new System.Drawing.Size(75, 23);
            this.dir2Button.TabIndex = 1;
            this.dir2Button.Text = "Dir2";
            this.dir2Button.UseVisualStyleBackColor = true;
            this.dir2Button.Click += new System.EventHandler(this.dir2Button_Click);
            // 
            // dir1Text
            // 
            this.dir1Text.Location = new System.Drawing.Point(111, 12);
            this.dir1Text.Name = "dir1Text";
            this.dir1Text.Size = new System.Drawing.Size(677, 20);
            this.dir1Text.TabIndex = 2;
            this.dir1Text.Text = "D:\\git\\Hades Workshop\\Byte align Work\\p0data2_Stock_Stock\\HadesWorkshopAssets\\p0d" +
    "ata2";
            // 
            // dir2Text
            // 
            this.dir2Text.Location = new System.Drawing.Point(111, 47);
            this.dir2Text.Name = "dir2Text";
            this.dir2Text.Size = new System.Drawing.Size(677, 20);
            this.dir2Text.TabIndex = 3;
            this.dir2Text.Text = "D:\\git\\Hades Workshop\\Byte align Work\\p0data2_Moguri_Stock\\HadesWorkshopAssets\\p0" +
    "data2";
            // 
            // outputText
            // 
            this.outputText.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.outputText.DetectUrls = false;
            this.outputText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputText.ForeColor = System.Drawing.Color.SaddleBrown;
            this.outputText.Location = new System.Drawing.Point(172, 85);
            this.outputText.Name = "outputText";
            this.outputText.ReadOnly = true;
            this.outputText.Size = new System.Drawing.Size(616, 353);
            this.outputText.TabIndex = 4;
            this.outputText.Text = "";
            this.outputText.TextChanged += new System.EventHandler(this.outputText_TextChanged);
            // 
            // p0data2Files
            // 
            this.p0data2Files.AutoSize = true;
            this.p0data2Files.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.p0data2Files.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p0data2Files.ForeColor = System.Drawing.Color.White;
            this.p0data2Files.Location = new System.Drawing.Point(12, 85);
            this.p0data2Files.Name = "p0data2Files";
            this.p0data2Files.Size = new System.Drawing.Size(154, 17);
            this.p0data2Files.TabIndex = 5;
            this.p0data2Files.TabStop = true;
            this.p0data2Files.Text = "p0data2 Compare Files";
            this.p0data2Files.UseVisualStyleBackColor = true;
            this.p0data2Files.CheckedChanged += new System.EventHandler(this.p0data2Files_CheckedChanged);
            // 
            // p0data2BIN
            // 
            this.p0data2BIN.AutoSize = true;
            this.p0data2BIN.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.p0data2BIN.Enabled = false;
            this.p0data2BIN.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p0data2BIN.ForeColor = System.Drawing.Color.White;
            this.p0data2BIN.Location = new System.Drawing.Point(12, 120);
            this.p0data2BIN.Name = "p0data2BIN";
            this.p0data2BIN.Size = new System.Drawing.Size(146, 17);
            this.p0data2BIN.TabIndex = 9;
            this.p0data2BIN.TabStop = true;
            this.p0data2BIN.Text = "p0data2 Files VS BIN";
            this.p0data2BIN.UseVisualStyleBackColor = true;
            // 
            // p0data7Files
            // 
            this.p0data7Files.AutoSize = true;
            this.p0data7Files.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.p0data7Files.Enabled = false;
            this.p0data7Files.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p0data7Files.ForeColor = System.Drawing.Color.White;
            this.p0data7Files.Location = new System.Drawing.Point(12, 155);
            this.p0data7Files.Name = "p0data7Files";
            this.p0data7Files.Size = new System.Drawing.Size(154, 17);
            this.p0data7Files.TabIndex = 10;
            this.p0data7Files.TabStop = true;
            this.p0data7Files.Text = "p0data7 Compare Files";
            this.p0data7Files.UseVisualStyleBackColor = true;
            // 
            // p0data7BIN
            // 
            this.p0data7BIN.AutoSize = true;
            this.p0data7BIN.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.p0data7BIN.Enabled = false;
            this.p0data7BIN.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p0data7BIN.ForeColor = System.Drawing.Color.White;
            this.p0data7BIN.Location = new System.Drawing.Point(12, 190);
            this.p0data7BIN.Name = "p0data7BIN";
            this.p0data7BIN.Size = new System.Drawing.Size(146, 17);
            this.p0data7BIN.TabIndex = 11;
            this.p0data7BIN.TabStop = true;
            this.p0data7BIN.Text = "p0data7 Files VS BIN";
            this.p0data7BIN.UseVisualStyleBackColor = true;
            // 
            // runButton
            // 
            this.runButton.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.runButton.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.runButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.runButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.runButton.ForeColor = System.Drawing.SystemColors.Control;
            this.runButton.Location = new System.Drawing.Point(12, 370);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(154, 68);
            this.runButton.TabIndex = 12;
            this.runButton.Text = "Run Debug Func";
            this.runButton.UseVisualStyleBackColor = false;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // eventText
            // 
            this.eventText.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.eventText.DetectUrls = false;
            this.eventText.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eventText.ForeColor = System.Drawing.Color.SaddleBrown;
            this.eventText.Location = new System.Drawing.Point(12, 328);
            this.eventText.Name = "eventText";
            this.eventText.ReadOnly = true;
            this.eventText.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.eventText.Size = new System.Drawing.Size(154, 26);
            this.eventText.TabIndex = 13;
            this.eventText.Text = "";
            // 
            // JsonCheckBox
            // 
            this.JsonCheckBox.AutoSize = true;
            this.JsonCheckBox.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.JsonCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.JsonCheckBox.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.JsonCheckBox.Location = new System.Drawing.Point(52, 286);
            this.JsonCheckBox.Name = "JsonCheckBox";
            this.JsonCheckBox.Size = new System.Drawing.Size(59, 20);
            this.JsonCheckBox.TabIndex = 14;
            this.JsonCheckBox.Text = "Json";
            this.JsonCheckBox.UseVisualStyleBackColor = true;
            // 
            // DebugForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.JsonCheckBox);
            this.Controls.Add(this.eventText);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.p0data7BIN);
            this.Controls.Add(this.p0data7Files);
            this.Controls.Add(this.p0data2BIN);
            this.Controls.Add(this.p0data2Files);
            this.Controls.Add(this.outputText);
            this.Controls.Add(this.dir2Text);
            this.Controls.Add(this.dir1Text);
            this.Controls.Add(this.dir2Button);
            this.Controls.Add(this.dir1Button);
            this.Name = "DebugForm";
            this.Text = "FFIX Stiltzkin\'s Bag 2.x Debug Functions";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button dir1Button;
        private System.Windows.Forms.Button dir2Button;
        private System.Windows.Forms.TextBox dir1Text;
        private System.Windows.Forms.TextBox dir2Text;
        private System.Windows.Forms.RichTextBox outputText;
        private System.Windows.Forms.RadioButton p0data2Files;
        private System.Windows.Forms.RadioButton p0data2BIN;
        private System.Windows.Forms.RadioButton p0data7Files;
        private System.Windows.Forms.RadioButton p0data7BIN;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.RichTextBox eventText;
        private System.Windows.Forms.CheckBox JsonCheckBox;
    }
}