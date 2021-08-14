
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
            this.b_search = new System.Windows.Forms.Button();
            this.l_counter = new System.Windows.Forms.Label();
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
            this.richTextBox_output.Location = new System.Drawing.Point(212, 108);
            this.richTextBox_output.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox_output.Name = "richTextBox_output";
            this.richTextBox_output.ReadOnly = true;
            this.richTextBox_output.Size = new System.Drawing.Size(271, 268);
            this.richTextBox_output.TabIndex = 1;
            this.richTextBox_output.Text = "int getRandomNumber() {\n     return 4; // chosen by fair dice roll.\n             " +
    "       // guaranteed to be random.\n}";
            this.richTextBox_output.WordWrap = false;
            // 
            // button_rand
            // 
            this.button_rand.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_rand.Location = new System.Drawing.Point(488, 336);
            this.button_rand.Margin = new System.Windows.Forms.Padding(2);
            this.button_rand.Name = "button_rand";
            this.button_rand.Size = new System.Drawing.Size(135, 39);
            this.button_rand.TabIndex = 2;
            this.button_rand.Text = "Randomize\r\n";
            this.button_rand.UseVisualStyleBackColor = true;
            this.button_rand.Click += new System.EventHandler(this.button_rand_Click);
            // 
            // radio_shopitems_1safe
            // 
            this.radio_shopitems_1safe.AutoSize = true;
            this.radio_shopitems_1safe.Checked = true;
            this.radio_shopitems_1safe.Location = new System.Drawing.Point(12, 17);
            this.radio_shopitems_1safe.Margin = new System.Windows.Forms.Padding(2);
            this.radio_shopitems_1safe.Name = "radio_shopitems_1safe";
            this.radio_shopitems_1safe.Size = new System.Drawing.Size(47, 17);
            this.radio_shopitems_1safe.TabIndex = 3;
            this.radio_shopitems_1safe.TabStop = true;
            this.radio_shopitems_1safe.Text = "Safe";
            this.radio_shopitems_1safe.UseVisualStyleBackColor = true;
            this.radio_shopitems_1safe.CheckedChanged += new System.EventHandler(this.radio_shopitems_1safe_CheckedChanged);
            // 
            // radio_synthesis
            // 
            this.radio_synthesis.AutoSize = true;
            this.radio_synthesis.Location = new System.Drawing.Point(15, 18);
            this.radio_synthesis.Margin = new System.Windows.Forms.Padding(2);
            this.radio_synthesis.Name = "radio_synthesis";
            this.radio_synthesis.Size = new System.Drawing.Size(78, 17);
            this.radio_synthesis.TabIndex = 4;
            this.radio_synthesis.Text = "Randomize";
            this.radio_synthesis.UseVisualStyleBackColor = true;
            this.radio_synthesis.CheckedChanged += new System.EventHandler(this.radio_synthesis_CheckedChanged);
            // 
            // radio_defaultequipment
            // 
            this.radio_defaultequipment.AutoSize = true;
            this.radio_defaultequipment.Location = new System.Drawing.Point(6, 9);
            this.radio_defaultequipment.Margin = new System.Windows.Forms.Padding(2);
            this.radio_defaultequipment.Name = "radio_defaultequipment";
            this.radio_defaultequipment.Size = new System.Drawing.Size(109, 17);
            this.radio_defaultequipment.TabIndex = 5;
            this.radio_defaultequipment.Text = "default equipment";
            this.radio_defaultequipment.UseVisualStyleBackColor = true;
            this.radio_defaultequipment.CheckedChanged += new System.EventHandler(this.radio_defaultequipment_CheckedChanged);
            // 
            // radio_basestats
            // 
            this.radio_basestats.AutoSize = true;
            this.radio_basestats.Location = new System.Drawing.Point(6, 31);
            this.radio_basestats.Name = "radio_basestats";
            this.radio_basestats.Size = new System.Drawing.Size(73, 17);
            this.radio_basestats.TabIndex = 6;
            this.radio_basestats.Text = "base stats";
            this.radio_basestats.UseVisualStyleBackColor = true;
            this.radio_basestats.CheckedChanged += new System.EventHandler(this.radio_basestats_CheckedChanged);
            // 
            // radio_abilitygems
            // 
            this.radio_abilitygems.AutoSize = true;
            this.radio_abilitygems.Location = new System.Drawing.Point(6, 54);
            this.radio_abilitygems.Name = "radio_abilitygems";
            this.radio_abilitygems.Size = new System.Drawing.Size(80, 17);
            this.radio_abilitygems.TabIndex = 7;
            this.radio_abilitygems.Text = "Ability gems";
            this.radio_abilitygems.UseVisualStyleBackColor = true;
            this.radio_abilitygems.CheckedChanged += new System.EventHandler(this.radio_abilitygems_CheckedChanged);
            // 
            // label_output
            // 
            this.label_output.AutoSize = true;
            this.label_output.Location = new System.Drawing.Point(209, 90);
            this.label_output.Name = "label_output";
            this.label_output.Size = new System.Drawing.Size(39, 13);
            this.label_output.TabIndex = 11;
            this.label_output.Text = "Output";
            // 
            // label_seed
            // 
            this.label_seed.AutoSize = true;
            this.label_seed.Location = new System.Drawing.Point(10, 13);
            this.label_seed.Name = "label_seed";
            this.label_seed.Size = new System.Drawing.Size(39, 13);
            this.label_seed.TabIndex = 12;
            this.label_seed.Text = "SEED:";
            // 
            // textBox_seed
            // 
            this.textBox_seed.Location = new System.Drawing.Point(50, 11);
            this.textBox_seed.Name = "textBox_seed";
            this.textBox_seed.Size = new System.Drawing.Size(484, 20);
            this.textBox_seed.TabIndex = 13;
            // 
            // richTextBox_debug
            // 
            this.richTextBox_debug.Location = new System.Drawing.Point(488, 108);
            this.richTextBox_debug.Name = "richTextBox_debug";
            this.richTextBox_debug.ReadOnly = true;
            this.richTextBox_debug.Size = new System.Drawing.Size(136, 225);
            this.richTextBox_debug.TabIndex = 14;
            this.richTextBox_debug.Text = "no bugs yet";
            this.richTextBox_debug.TextChanged += new System.EventHandler(this.richTextBox_debug_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(488, 90);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Debug";
            // 
            // pictureBox_yuno
            // 
            this.pictureBox_yuno.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox_yuno.Image = global::rand9er.Properties.Resources.yuno_error;
            this.pictureBox_yuno.Location = new System.Drawing.Point(441, 336);
            this.pictureBox_yuno.Name = "pictureBox_yuno";
            this.pictureBox_yuno.Size = new System.Drawing.Size(41, 39);
            this.pictureBox_yuno.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_yuno.TabIndex = 16;
            this.pictureBox_yuno.TabStop = false;
            this.pictureBox_yuno.Visible = false;
            this.pictureBox_yuno.Click += new System.EventHandler(this.pictureBox_yuno_Click);
            // 
            // radio_shopitems_2max
            // 
            this.radio_shopitems_2max.AutoSize = true;
            this.radio_shopitems_2max.Location = new System.Drawing.Point(12, 58);
            this.radio_shopitems_2max.Margin = new System.Windows.Forms.Padding(2);
            this.radio_shopitems_2max.Name = "radio_shopitems_2max";
            this.radio_shopitems_2max.Size = new System.Drawing.Size(45, 17);
            this.radio_shopitems_2max.TabIndex = 17;
            this.radio_shopitems_2max.Text = "Max";
            this.radio_shopitems_2max.UseVisualStyleBackColor = true;
            this.radio_shopitems_2max.CheckedChanged += new System.EventHandler(this.radio_shopitems_2max_CheckedChanged);
            // 
            // radio_shopitems_3rand
            // 
            this.radio_shopitems_3rand.AutoSize = true;
            this.radio_shopitems_3rand.Location = new System.Drawing.Point(12, 37);
            this.radio_shopitems_3rand.Margin = new System.Windows.Forms.Padding(2);
            this.radio_shopitems_3rand.Name = "radio_shopitems_3rand";
            this.radio_shopitems_3rand.Size = new System.Drawing.Size(65, 17);
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
            this.tabControl1.Location = new System.Drawing.Point(8, 90);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(196, 242);
            this.tabControl1.TabIndex = 19;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.c_medicshops);
            this.tabPage1.Controls.Add(this.c_medicitems);
            this.tabPage1.Controls.Add(this.radio_shopitems_1safe);
            this.tabPage1.Controls.Add(this.radio_shopitems_2max);
            this.tabPage1.Controls.Add(this.radio_shopitems_3rand);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(188, 213);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Shops";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // c_medicshops
            // 
            this.c_medicshops.AutoSize = true;
            this.c_medicshops.Location = new System.Drawing.Point(12, 115);
            this.c_medicshops.Margin = new System.Windows.Forms.Padding(2);
            this.c_medicshops.Name = "c_medicshops";
            this.c_medicshops.Size = new System.Drawing.Size(126, 17);
            this.c_medicshops.TabIndex = 24;
            this.c_medicshops.Text = "override medic shops\r\n";
            this.c_medicshops.UseVisualStyleBackColor = true;
            this.c_medicshops.CheckedChanged += new System.EventHandler(this.c_medicshops_CheckedChanged);
            // 
            // c_medicitems
            // 
            this.c_medicitems.AutoSize = true;
            this.c_medicitems.Location = new System.Drawing.Point(12, 79);
            this.c_medicitems.Margin = new System.Windows.Forms.Padding(2);
            this.c_medicitems.Name = "c_medicitems";
            this.c_medicitems.Size = new System.Drawing.Size(118, 30);
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
            this.tabPage3.Location = new System.Drawing.Point(4, 25);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(188, 213);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Synthesis";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Location = new System.Drawing.Point(2, 81);
            this.checkBox5.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(88, 17);
            this.checkBox5.TabIndex = 28;
            this.checkBox5.Text = "Synthed Item";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(22, 39);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(2);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(47, 17);
            this.radioButton1.TabIndex = 21;
            this.radioButton1.Text = "Safe";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Location = new System.Drawing.Point(1, 102);
            this.checkBox4.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(96, 17);
            this.checkBox4.TabIndex = 27;
            this.checkBox4.Text = "Locations Only";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(6, 60);
            this.checkBox2.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(91, 17);
            this.checkBox2.TabIndex = 26;
            this.checkBox2.Text = "Requirements";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.radio_defaultequipment);
            this.tabPage2.Controls.Add(this.radio_basestats);
            this.tabPage2.Controls.Add(this.radio_abilitygems);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(188, 213);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Characters";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // clear
            // 
            this.clear.Location = new System.Drawing.Point(586, 83);
            this.clear.Margin = new System.Windows.Forms.Padding(2);
            this.clear.Name = "clear";
            this.clear.Size = new System.Drawing.Size(38, 20);
            this.clear.TabIndex = 20;
            this.clear.Text = "clear";
            this.clear.UseVisualStyleBackColor = true;
            this.clear.Click += new System.EventHandler(this.clear_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(13, 336);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(189, 13);
            this.progressBar1.TabIndex = 21;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(13, 360);
            this.progressBar2.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(188, 13);
            this.progressBar2.TabIndex = 22;
            // 
            // b_rseed
            // 
            this.b_rseed.Location = new System.Drawing.Point(538, 10);
            this.b_rseed.Margin = new System.Windows.Forms.Padding(2);
            this.b_rseed.Name = "b_rseed";
            this.b_rseed.Size = new System.Drawing.Size(85, 19);
            this.b_rseed.TabIndex = 23;
            this.b_rseed.Text = "Random Seed";
            this.b_rseed.UseVisualStyleBackColor = true;
            this.b_rseed.Click += new System.EventHandler(this.b_rseed_Click_1);
            // 
            // b_open
            // 
            this.b_open.Location = new System.Drawing.Point(12, 35);
            this.b_open.Margin = new System.Windows.Forms.Padding(2);
            this.b_open.Name = "b_open";
            this.b_open.Size = new System.Drawing.Size(99, 20);
            this.b_open.TabIndex = 24;
            this.b_open.Text = "Set ff9 location";
            this.b_open.UseVisualStyleBackColor = true;
            this.b_open.Click += new System.EventHandler(this.b_open_Click);
            // 
            // tb_fl
            // 
            this.tb_fl.Location = new System.Drawing.Point(117, 35);
            this.tb_fl.Margin = new System.Windows.Forms.Padding(2);
            this.tb_fl.Name = "tb_fl";
            this.tb_fl.ReadOnly = true;
            this.tb_fl.Size = new System.Drawing.Size(417, 20);
            this.tb_fl.TabIndex = 25;
            // 
            // b_restore
            // 
            this.b_restore.Location = new System.Drawing.Point(538, 60);
            this.b_restore.Margin = new System.Windows.Forms.Padding(2);
            this.b_restore.Name = "b_restore";
            this.b_restore.Size = new System.Drawing.Size(86, 19);
            this.b_restore.TabIndex = 26;
            this.b_restore.Text = "Restore Stock";
            this.b_restore.UseVisualStyleBackColor = true;
            this.b_restore.Click += new System.EventHandler(this.b_restore_Click);
            // 
            // b_search
            // 
            this.b_search.Location = new System.Drawing.Point(538, 35);
            this.b_search.Name = "b_search";
            this.b_search.Size = new System.Drawing.Size(85, 20);
            this.b_search.TabIndex = 27;
            this.b_search.Text = "Auto Locate";
            this.b_search.UseVisualStyleBackColor = true;
            this.b_search.Click += new System.EventHandler(this.b_search_Click);
            // 
            // l_counter
            // 
            this.l_counter.AutoSize = true;
            this.l_counter.Location = new System.Drawing.Point(85, 360);
            this.l_counter.Name = "l_counter";
            this.l_counter.Size = new System.Drawing.Size(35, 13);
            this.l_counter.TabIndex = 28;
            this.l_counter.Text = "label2";
            // 
            // rand9er
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 384);
            this.Controls.Add(this.l_counter);
            this.Controls.Add(this.b_search);
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
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "rand9er";
            this.Text = "FFIX Randomizer Assistant 0.8";
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
        private System.Windows.Forms.Button b_search;
        private System.Windows.Forms.Label l_counter;
    }
}

