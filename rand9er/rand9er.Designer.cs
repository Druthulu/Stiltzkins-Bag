
namespace rand9er
{
    partial class rand9er
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
            this.richTextBox_output = new System.Windows.Forms.RichTextBox();
            this.button_rand = new System.Windows.Forms.Button();
            this.radio_shopitems_1safe = new System.Windows.Forms.RadioButton();
            this.radio_synthesis = new System.Windows.Forms.RadioButton();
            this.radio_defaultequipment = new System.Windows.Forms.RadioButton();
            this.radio_basestats = new System.Windows.Forms.RadioButton();
            this.radio_abilitygems = new System.Windows.Forms.RadioButton();
            this.label_output = new System.Windows.Forms.Label();
            this.label_seed = new System.Windows.Forms.Label();
            this.textBox_seed = new System.Windows.Forms.TextBox();
            this.richTextBox_debug = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox_yuno = new System.Windows.Forms.PictureBox();
            this.radio_shopitems_2max = new System.Windows.Forms.RadioButton();
            this.radio_shopitems_3rand = new System.Windows.Forms.RadioButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.c_medicshops = new System.Windows.Forms.CheckBox();
            this.c_medicitems = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.clear = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.b_rseed = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.b_open = new System.Windows.Forms.Button();
            this.tb_fl = new System.Windows.Forms.TextBox();
            this.b_restore = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_yuno)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox_output
            // 
            this.richTextBox_output.ForeColor = System.Drawing.SystemColors.InfoText;
            this.richTextBox_output.Location = new System.Drawing.Point(283, 119);
            this.richTextBox_output.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.richTextBox_output.Name = "richTextBox_output";
            this.richTextBox_output.ReadOnly = true;
            this.richTextBox_output.Size = new System.Drawing.Size(360, 343);
            this.richTextBox_output.TabIndex = 1;
            this.richTextBox_output.Text = "int getRandomNumber() {\n     return 4; // chosen by fair dice roll.\n             " +
    "       // guaranteed to be random.\n}";
            this.richTextBox_output.WordWrap = false;
            // 
            // button_rand
            // 
            this.button_rand.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_rand.Location = new System.Drawing.Point(650, 414);
            this.button_rand.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_rand.Name = "button_rand";
            this.button_rand.Size = new System.Drawing.Size(180, 48);
            this.button_rand.TabIndex = 2;
            this.button_rand.Text = "Randomize\r\n";
            this.button_rand.UseVisualStyleBackColor = true;
            this.button_rand.Click += new System.EventHandler(this.button_rand_Click);
            // 
            // radio_shopitems_1safe
            // 
            this.radio_shopitems_1safe.AutoSize = true;
            this.radio_shopitems_1safe.Checked = true;
            this.radio_shopitems_1safe.Location = new System.Drawing.Point(16, 21);
            this.radio_shopitems_1safe.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radio_shopitems_1safe.Name = "radio_shopitems_1safe";
            this.radio_shopitems_1safe.Size = new System.Drawing.Size(58, 21);
            this.radio_shopitems_1safe.TabIndex = 3;
            this.radio_shopitems_1safe.TabStop = true;
            this.radio_shopitems_1safe.Text = "Safe";
            this.radio_shopitems_1safe.UseVisualStyleBackColor = true;
            this.radio_shopitems_1safe.CheckedChanged += new System.EventHandler(this.radio_shopitems_1safe_CheckedChanged);
            // 
            // radio_synthesis
            // 
            this.radio_synthesis.AutoSize = true;
            this.radio_synthesis.Location = new System.Drawing.Point(20, 22);
            this.radio_synthesis.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radio_synthesis.Name = "radio_synthesis";
            this.radio_synthesis.Size = new System.Drawing.Size(100, 21);
            this.radio_synthesis.TabIndex = 4;
            this.radio_synthesis.Text = "Randomize";
            this.radio_synthesis.UseVisualStyleBackColor = true;
            this.radio_synthesis.CheckedChanged += new System.EventHandler(this.radio_synthesis_CheckedChanged);
            // 
            // radio_defaultequipment
            // 
            this.radio_defaultequipment.AutoSize = true;
            this.radio_defaultequipment.Location = new System.Drawing.Point(8, 11);
            this.radio_defaultequipment.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radio_defaultequipment.Name = "radio_defaultequipment";
            this.radio_defaultequipment.Size = new System.Drawing.Size(142, 21);
            this.radio_defaultequipment.TabIndex = 5;
            this.radio_defaultequipment.Text = "default equipment";
            this.radio_defaultequipment.UseVisualStyleBackColor = true;
            this.radio_defaultequipment.CheckedChanged += new System.EventHandler(this.radio_defaultequipment_CheckedChanged);
            // 
            // radio_basestats
            // 
            this.radio_basestats.AutoSize = true;
            this.radio_basestats.Location = new System.Drawing.Point(8, 38);
            this.radio_basestats.Margin = new System.Windows.Forms.Padding(4);
            this.radio_basestats.Name = "radio_basestats";
            this.radio_basestats.Size = new System.Drawing.Size(94, 21);
            this.radio_basestats.TabIndex = 6;
            this.radio_basestats.Text = "base stats";
            this.radio_basestats.UseVisualStyleBackColor = true;
            this.radio_basestats.CheckedChanged += new System.EventHandler(this.radio_basestats_CheckedChanged);
            // 
            // radio_abilitygems
            // 
            this.radio_abilitygems.AutoSize = true;
            this.radio_abilitygems.Location = new System.Drawing.Point(8, 67);
            this.radio_abilitygems.Margin = new System.Windows.Forms.Padding(4);
            this.radio_abilitygems.Name = "radio_abilitygems";
            this.radio_abilitygems.Size = new System.Drawing.Size(104, 21);
            this.radio_abilitygems.TabIndex = 7;
            this.radio_abilitygems.Text = "Ability gems";
            this.radio_abilitygems.UseVisualStyleBackColor = true;
            this.radio_abilitygems.CheckedChanged += new System.EventHandler(this.radio_abilitygems_CheckedChanged);
            // 
            // label_output
            // 
            this.label_output.AutoSize = true;
            this.label_output.Location = new System.Drawing.Point(280, 95);
            this.label_output.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_output.Name = "label_output";
            this.label_output.Size = new System.Drawing.Size(51, 17);
            this.label_output.TabIndex = 11;
            this.label_output.Text = "Output";
            // 
            // label_seed
            // 
            this.label_seed.AutoSize = true;
            this.label_seed.Location = new System.Drawing.Point(13, 16);
            this.label_seed.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_seed.Name = "label_seed";
            this.label_seed.Size = new System.Drawing.Size(49, 17);
            this.label_seed.TabIndex = 12;
            this.label_seed.Text = "SEED:";
            // 
            // textBox_seed
            // 
            this.textBox_seed.Location = new System.Drawing.Point(66, 14);
            this.textBox_seed.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_seed.Name = "textBox_seed";
            this.textBox_seed.Size = new System.Drawing.Size(644, 22);
            this.textBox_seed.TabIndex = 13;
            // 
            // richTextBox_debug
            // 
            this.richTextBox_debug.Location = new System.Drawing.Point(650, 119);
            this.richTextBox_debug.Margin = new System.Windows.Forms.Padding(4);
            this.richTextBox_debug.Name = "richTextBox_debug";
            this.richTextBox_debug.ReadOnly = true;
            this.richTextBox_debug.Size = new System.Drawing.Size(180, 289);
            this.richTextBox_debug.TabIndex = 14;
            this.richTextBox_debug.Text = "no bugs yet";
            this.richTextBox_debug.TextChanged += new System.EventHandler(this.richTextBox_debug_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(650, 93);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 17);
            this.label1.TabIndex = 15;
            this.label1.Text = "Debug";
            // 
            // pictureBox_yuno
            // 
            this.pictureBox_yuno.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox_yuno.Image = global::rand9er.Properties.Resources.yuno_error;
            this.pictureBox_yuno.Location = new System.Drawing.Point(588, 414);
            this.pictureBox_yuno.Margin = new System.Windows.Forms.Padding(4);
            this.pictureBox_yuno.Name = "pictureBox_yuno";
            this.pictureBox_yuno.Size = new System.Drawing.Size(55, 48);
            this.pictureBox_yuno.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_yuno.TabIndex = 16;
            this.pictureBox_yuno.TabStop = false;
            this.pictureBox_yuno.Visible = false;
            this.pictureBox_yuno.Click += new System.EventHandler(this.pictureBox_yuno_Click);
            // 
            // radio_shopitems_2max
            // 
            this.radio_shopitems_2max.AutoSize = true;
            this.radio_shopitems_2max.Location = new System.Drawing.Point(16, 71);
            this.radio_shopitems_2max.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radio_shopitems_2max.Name = "radio_shopitems_2max";
            this.radio_shopitems_2max.Size = new System.Drawing.Size(54, 21);
            this.radio_shopitems_2max.TabIndex = 17;
            this.radio_shopitems_2max.Text = "Max";
            this.radio_shopitems_2max.UseVisualStyleBackColor = true;
            this.radio_shopitems_2max.CheckedChanged += new System.EventHandler(this.radio_shopitems_2max_CheckedChanged);
            // 
            // radio_shopitems_3rand
            // 
            this.radio_shopitems_3rand.AutoSize = true;
            this.radio_shopitems_3rand.Location = new System.Drawing.Point(16, 46);
            this.radio_shopitems_3rand.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.radio_shopitems_3rand.Name = "radio_shopitems_3rand";
            this.radio_shopitems_3rand.Size = new System.Drawing.Size(82, 21);
            this.radio_shopitems_3rand.TabIndex = 18;
            this.radio_shopitems_3rand.Text = "Random";
            this.radio_shopitems_3rand.UseVisualStyleBackColor = true;
            this.radio_shopitems_3rand.CheckedChanged += new System.EventHandler(this.radio_shopitems_3rand_CheckedChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 95);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(261, 313);
            this.tabControl1.TabIndex = 19;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.c_medicshops);
            this.tabPage1.Controls.Add(this.c_medicitems);
            this.tabPage1.Controls.Add(this.radio_shopitems_1safe);
            this.tabPage1.Controls.Add(this.radio_shopitems_2max);
            this.tabPage1.Controls.Add(this.radio_shopitems_3rand);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(253, 281);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Shops";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // c_medicshops
            // 
            this.c_medicshops.AutoSize = true;
            this.c_medicshops.Location = new System.Drawing.Point(16, 141);
            this.c_medicshops.Name = "c_medicshops";
            this.c_medicshops.Size = new System.Drawing.Size(165, 21);
            this.c_medicshops.TabIndex = 24;
            this.c_medicshops.Text = "override medic shops\r\n";
            this.c_medicshops.UseVisualStyleBackColor = true;
            this.c_medicshops.CheckedChanged += new System.EventHandler(this.c_medicshops_CheckedChanged);
            // 
            // c_medicitems
            // 
            this.c_medicitems.AutoSize = true;
            this.c_medicitems.Location = new System.Drawing.Point(16, 97);
            this.c_medicitems.Name = "c_medicitems";
            this.c_medicitems.Size = new System.Drawing.Size(153, 38);
            this.c_medicitems.TabIndex = 22;
            this.c_medicitems.Text = "include medic items\r\n(5.9% chance)";
            this.c_medicitems.UseVisualStyleBackColor = true;
            this.c_medicitems.CheckedChanged += new System.EventHandler(this.c_medicitems_CheckedChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.checkBox5);
            this.tabPage3.Controls.Add(this.radio_synthesis);
            this.tabPage3.Controls.Add(this.radioButton1);
            this.tabPage3.Controls.Add(this.checkBox4);
            this.tabPage3.Controls.Add(this.checkBox2);
            this.tabPage3.Location = new System.Drawing.Point(4, 28);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(253, 281);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Synthesis";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(8, 93);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(112, 21);
            this.checkBox5.TabIndex = 28;
            this.checkBox5.Text = "Synthed Item";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(30, 48);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(58, 21);
            this.radioButton1.TabIndex = 21;
            this.radioButton1.Text = "Safe";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(8, 120);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(124, 21);
            this.checkBox4.TabIndex = 27;
            this.checkBox4.Text = "Locations Only";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(8, 75);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(118, 21);
            this.checkBox2.TabIndex = 26;
            this.checkBox2.Text = "Requirements";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.radio_defaultequipment);
            this.tabPage2.Controls.Add(this.radio_basestats);
            this.tabPage2.Controls.Add(this.radio_abilitygems);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(253, 281);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Characters";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // clear
            // 
            this.clear.Location = new System.Drawing.Point(780, 87);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(50, 25);
            this.clear.TabIndex = 20;
            this.clear.Text = "clear";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.clear_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 414);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(257, 15);
            this.progressBar1.TabIndex = 21;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(12, 447);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(257, 15);
            this.progressBar2.TabIndex = 22;
            // 
            // b_rseed
            // 
            this.b_rseed.Location = new System.Drawing.Point(717, 12);
            this.b_rseed.Name = "b_rseed";
            this.b_rseed.Size = new System.Drawing.Size(113, 23);
            this.b_rseed.TabIndex = 23;
            this.b_rseed.Text = "Random Seed";
            this.b_rseed.UseVisualStyleBackColor = true;
            this.b_rseed.Click += new System.EventHandler(this.b_rseed_Click_1);
            // 
            // b_open
            // 
            this.b_open.Location = new System.Drawing.Point(16, 43);
            this.b_open.Name = "b_open";
            this.b_open.Size = new System.Drawing.Size(132, 23);
            this.b_open.TabIndex = 24;
            this.b_open.Text = "Set ff9 location";
            this.b_open.UseVisualStyleBackColor = true;
            this.b_open.Click += new System.EventHandler(this.b_open_Click);
            // 
            // tb_fl
            // 
            this.tb_fl.Location = new System.Drawing.Point(156, 43);
            this.tb_fl.Name = "tb_fl";
            this.tb_fl.ReadOnly = true;
            this.tb_fl.Size = new System.Drawing.Size(555, 22);
            this.tb_fl.TabIndex = 25;
            this.tb_fl.Text = "Computer\\HKEY_CLASSES_ROOT\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCac" +
    "he";
            // 
            // b_restore
            // 
            this.b_restore.Location = new System.Drawing.Point(717, 43);
            this.b_restore.Name = "b_restore";
            this.b_restore.Size = new System.Drawing.Size(113, 23);
            this.b_restore.TabIndex = 26;
            this.b_restore.Text = "Restore Stock";
            this.b_restore.UseVisualStyleBackColor = true;
            // 
            // rand9er
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 473);
            this.Controls.Add(this.b_restore);
            this.Controls.Add(this.tb_fl);
            this.Controls.Add(this.b_open);
            this.Controls.Add(this.b_rseed);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.clear);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.pictureBox_yuno);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox_debug);
            this.Controls.Add(this.textBox_seed);
            this.Controls.Add(this.label_seed);
            this.Controls.Add(this.label_output);
            this.Controls.Add(this.button_rand);
            this.Controls.Add(this.richTextBox_output);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "rand9er";
            this.Text = "FFIX Randomizer Assistant 0.75";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_yuno)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox_output;
        private System.Windows.Forms.Button button_rand;
        private System.Windows.Forms.RadioButton radio_shopitems_1safe;
        private System.Windows.Forms.RadioButton radio_synthesis;
        private System.Windows.Forms.RadioButton radio_defaultequipment;
        private System.Windows.Forms.RadioButton radio_basestats;
        private System.Windows.Forms.RadioButton radio_abilitygems;
        private System.Windows.Forms.Label label_output;
        private System.Windows.Forms.Label label_seed;
        private System.Windows.Forms.TextBox textBox_seed;
        private System.Windows.Forms.RichTextBox richTextBox_debug;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox_yuno;
        private System.Windows.Forms.RadioButton radio_shopitems_2max;
        private System.Windows.Forms.RadioButton radio_shopitems_3rand;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox c_medicitems;
        private System.Windows.Forms.CheckBox c_medicshops;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.Button clear;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button b_rseed;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button b_open;
        private System.Windows.Forms.TextBox tb_fl;
        private System.Windows.Forms.Button b_restore;
    }
}

