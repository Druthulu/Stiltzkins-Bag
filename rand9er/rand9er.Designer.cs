
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
            this.label_output = new System.Windows.Forms.Label();
            this.label_seed = new System.Windows.Forms.Label();
            this.textBox_seed = new System.Windows.Forms.TextBox();
            this.richTextBox_debug = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox_yuno = new System.Windows.Forms.PictureBox();
            this.c_medicshops = new System.Windows.Forms.CheckBox();
            this.c_medicitems = new System.Windows.Forms.CheckBox();
            this.c_prices = new System.Windows.Forms.CheckBox();
            this.c_result = new System.Windows.Forms.CheckBox();
            this.c_require = new System.Windows.Forms.CheckBox();
            this.clear = new System.Windows.Forms.Button();
            this.pbar_branch = new System.Windows.Forms.ProgressBar();
            this.pbar_tree = new System.Windows.Forms.ProgressBar();
            this.b_rseed = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.b_open = new System.Windows.Forms.Button();
            this.tb_fl = new System.Windows.Forms.TextBox();
            this.b_restore = new System.Windows.Forms.Button();
            this.b_search = new System.Windows.Forms.Button();
            this.l_tree = new System.Windows.Forms.Label();
            this.b_rw = new System.Windows.Forms.Button();
            this.pbar_leaf = new System.Windows.Forms.ProgressBar();
            this.l_branch = new System.Windows.Forms.Label();
            this.l_leaf = new System.Windows.Forms.Label();
            this.pbar_seed = new System.Windows.Forms.ProgressBar();
            this.l_seed = new System.Windows.Forms.Label();
            this.cm_synth = new System.Windows.Forms.CheckBox();
            this.cm_itemshop = new System.Windows.Forms.CheckBox();
            this.cm_char = new System.Windows.Forms.CheckBox();
            this.c_safe = new System.Windows.Forms.CheckBox();
            this.c_random = new System.Windows.Forms.CheckBox();
            this.c_max = new System.Windows.Forms.CheckBox();
            this.c_default = new System.Windows.Forms.CheckBox();
            this.c_basestats = new System.Windows.Forms.CheckBox();
            this.c_abilitygems = new System.Windows.Forms.CheckBox();
            this.l_shopm = new System.Windows.Forms.Label();
            this.c_random_e = new System.Windows.Forms.CheckBox();
            this.c_main_e = new System.Windows.Forms.CheckBox();
            this.c_all_e = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_yuno)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox_output
            // 
            this.richTextBox_output.ForeColor = System.Drawing.SystemColors.InfoText;
            this.richTextBox_output.Location = new System.Drawing.Point(155, 285);
            this.richTextBox_output.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox_output.Name = "richTextBox_output";
            this.richTextBox_output.ReadOnly = true;
            this.richTextBox_output.Size = new System.Drawing.Size(328, 533);
            this.richTextBox_output.TabIndex = 1;
            this.richTextBox_output.Text = "int getRandomNumber() {\n     return 4; // chosen by fair dice roll.\n             " +
    "       // guaranteed to be random.\n}";
            this.richTextBox_output.WordWrap = false;
            // 
            // button_rand
            // 
            this.button_rand.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_rand.Location = new System.Drawing.Point(491, 269);
            this.button_rand.Margin = new System.Windows.Forms.Padding(2);
            this.button_rand.Name = "button_rand";
            this.button_rand.Size = new System.Drawing.Size(133, 72);
            this.button_rand.TabIndex = 2;
            this.button_rand.Text = "Randomize\r\n";
            this.button_rand.UseVisualStyleBackColor = true;
            this.button_rand.Click += new System.EventHandler(this.button_rand_Click);
            // 
            // label_output
            // 
            this.label_output.AutoSize = true;
            this.label_output.Location = new System.Drawing.Point(152, 269);
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
            this.richTextBox_debug.Location = new System.Drawing.Point(491, 126);
            this.richTextBox_debug.Name = "richTextBox_debug";
            this.richTextBox_debug.ReadOnly = true;
            this.richTextBox_debug.Size = new System.Drawing.Size(133, 138);
            this.richTextBox_debug.TabIndex = 14;
            this.richTextBox_debug.Text = "no bugs yet";
            this.richTextBox_debug.TextChanged += new System.EventHandler(this.richTextBox_debug_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(488, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Debug";
            // 
            // pictureBox_yuno
            // 
            this.pictureBox_yuno.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox_yuno.Image = global::rand9er.Properties.Resources.yuno_error;
            this.pictureBox_yuno.Location = new System.Drawing.Point(442, 337);
            this.pictureBox_yuno.Name = "pictureBox_yuno";
            this.pictureBox_yuno.Size = new System.Drawing.Size(41, 39);
            this.pictureBox_yuno.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_yuno.TabIndex = 16;
            this.pictureBox_yuno.TabStop = false;
            this.pictureBox_yuno.Visible = false;
            this.pictureBox_yuno.Click += new System.EventHandler(this.pictureBox_yuno_Click);
            // 
            // c_medicshops
            // 
            this.c_medicshops.AutoSize = true;
            this.c_medicshops.Enabled = false;
            this.c_medicshops.Location = new System.Drawing.Point(24, 225);
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
            this.c_medicitems.Enabled = false;
            this.c_medicitems.Location = new System.Drawing.Point(24, 191);
            this.c_medicitems.Margin = new System.Windows.Forms.Padding(2);
            this.c_medicitems.Name = "c_medicitems";
            this.c_medicitems.Size = new System.Drawing.Size(118, 30);
            this.c_medicitems.TabIndex = 22;
            this.c_medicitems.Text = "include medic items\r\n(5.9% chance)";
            this.c_medicitems.UseVisualStyleBackColor = true;
            this.c_medicitems.CheckedChanged += new System.EventHandler(this.c_medicitems_CheckedChanged);
            // 
            // c_prices
            // 
            this.c_prices.AutoSize = true;
            this.c_prices.Enabled = false;
            this.c_prices.Location = new System.Drawing.Point(180, 146);
            this.c_prices.Name = "c_prices";
            this.c_prices.Size = new System.Drawing.Size(55, 17);
            this.c_prices.TabIndex = 29;
            this.c_prices.Text = "Prices";
            this.c_prices.UseVisualStyleBackColor = true;
            // 
            // c_result
            // 
            this.c_result.AutoSize = true;
            this.c_result.Enabled = false;
            this.c_result.Location = new System.Drawing.Point(180, 124);
            this.c_result.Margin = new System.Windows.Forms.Padding(2);
            this.c_result.Name = "c_result";
            this.c_result.Size = new System.Drawing.Size(56, 17);
            this.c_result.TabIndex = 28;
            this.c_result.Text = "Result";
            this.c_result.UseVisualStyleBackColor = true;
            // 
            // c_require
            // 
            this.c_require.AutoSize = true;
            this.c_require.Enabled = false;
            this.c_require.Location = new System.Drawing.Point(180, 103);
            this.c_require.Margin = new System.Windows.Forms.Padding(2);
            this.c_require.Name = "c_require";
            this.c_require.Size = new System.Drawing.Size(91, 17);
            this.c_require.TabIndex = 26;
            this.c_require.Text = "Requirements";
            this.c_require.UseVisualStyleBackColor = true;
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
            // pbar_branch
            // 
            this.pbar_branch.Location = new System.Drawing.Point(12, 337);
            this.pbar_branch.Margin = new System.Windows.Forms.Padding(2);
            this.pbar_branch.Name = "pbar_branch";
            this.pbar_branch.Size = new System.Drawing.Size(138, 13);
            this.pbar_branch.TabIndex = 21;
            // 
            // pbar_tree
            // 
            this.pbar_tree.Location = new System.Drawing.Point(12, 360);
            this.pbar_tree.Margin = new System.Windows.Forms.Padding(2);
            this.pbar_tree.Name = "pbar_tree";
            this.pbar_tree.Size = new System.Drawing.Size(138, 13);
            this.pbar_tree.TabIndex = 22;
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
            this.b_restore.Location = new System.Drawing.Point(538, 59);
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
            // l_tree
            // 
            this.l_tree.AutoSize = true;
            this.l_tree.Location = new System.Drawing.Point(34, 360);
            this.l_tree.Name = "l_tree";
            this.l_tree.Size = new System.Drawing.Size(34, 13);
            this.l_tree.TabIndex = 28;
            this.l_tree.Text = "Trees";
            // 
            // b_rw
            // 
            this.b_rw.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_rw.Location = new System.Drawing.Point(491, 346);
            this.b_rw.Name = "b_rw";
            this.b_rw.Size = new System.Drawing.Size(133, 26);
            this.b_rw.TabIndex = 29;
            this.b_rw.Text = "WRITE OUTPUT";
            this.b_rw.UseVisualStyleBackColor = true;
            this.b_rw.Click += new System.EventHandler(this.b_rw_Click);
            // 
            // pbar_leaf
            // 
            this.pbar_leaf.Location = new System.Drawing.Point(12, 310);
            this.pbar_leaf.Name = "pbar_leaf";
            this.pbar_leaf.Size = new System.Drawing.Size(138, 13);
            this.pbar_leaf.TabIndex = 30;
            // 
            // l_branch
            // 
            this.l_branch.AutoSize = true;
            this.l_branch.Location = new System.Drawing.Point(34, 337);
            this.l_branch.Name = "l_branch";
            this.l_branch.Size = new System.Drawing.Size(41, 13);
            this.l_branch.TabIndex = 31;
            this.l_branch.Text = "Branch";
            // 
            // l_leaf
            // 
            this.l_leaf.AutoSize = true;
            this.l_leaf.Location = new System.Drawing.Point(34, 310);
            this.l_leaf.Name = "l_leaf";
            this.l_leaf.Size = new System.Drawing.Size(28, 13);
            this.l_leaf.TabIndex = 32;
            this.l_leaf.Text = "Leaf";
            // 
            // pbar_seed
            // 
            this.pbar_seed.Location = new System.Drawing.Point(13, 285);
            this.pbar_seed.Name = "pbar_seed";
            this.pbar_seed.Size = new System.Drawing.Size(137, 13);
            this.pbar_seed.TabIndex = 33;
            // 
            // l_seed
            // 
            this.l_seed.AutoSize = true;
            this.l_seed.Location = new System.Drawing.Point(34, 285);
            this.l_seed.Name = "l_seed";
            this.l_seed.Size = new System.Drawing.Size(37, 13);
            this.l_seed.TabIndex = 34;
            this.l_seed.Text = "Seeds";
            // 
            // cm_synth
            // 
            this.cm_synth.AutoSize = true;
            this.cm_synth.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_synth.Location = new System.Drawing.Point(168, 74);
            this.cm_synth.Name = "cm_synth";
            this.cm_synth.Size = new System.Drawing.Size(100, 22);
            this.cm_synth.TabIndex = 40;
            this.cm_synth.Text = "Synthesis";
            this.cm_synth.UseVisualStyleBackColor = true;
            this.cm_synth.CheckedChanged += new System.EventHandler(this.cm_synth_CheckedChanged);
            // 
            // cm_itemshop
            // 
            this.cm_itemshop.AutoSize = true;
            this.cm_itemshop.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_itemshop.Location = new System.Drawing.Point(13, 74);
            this.cm_itemshop.Name = "cm_itemshop";
            this.cm_itemshop.Size = new System.Drawing.Size(112, 22);
            this.cm_itemshop.TabIndex = 41;
            this.cm_itemshop.Text = "Item Shops";
            this.cm_itemshop.UseVisualStyleBackColor = true;
            this.cm_itemshop.CheckedChanged += new System.EventHandler(this.cm_itemshop_CheckedChanged);
            // 
            // cm_char
            // 
            this.cm_char.AutoSize = true;
            this.cm_char.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_char.Location = new System.Drawing.Point(324, 74);
            this.cm_char.Name = "cm_char";
            this.cm_char.Size = new System.Drawing.Size(101, 22);
            this.cm_char.TabIndex = 42;
            this.cm_char.Text = "Character";
            this.cm_char.UseVisualStyleBackColor = true;
            this.cm_char.CheckedChanged += new System.EventHandler(this.cm_char_CheckedChanged);
            // 
            // c_safe
            // 
            this.c_safe.AutoSize = true;
            this.c_safe.Enabled = false;
            this.c_safe.Location = new System.Drawing.Point(23, 103);
            this.c_safe.Name = "c_safe";
            this.c_safe.Size = new System.Drawing.Size(48, 17);
            this.c_safe.TabIndex = 43;
            this.c_safe.Text = "Safe";
            this.c_safe.UseVisualStyleBackColor = true;
            this.c_safe.CheckedChanged += new System.EventHandler(this.c_safe_CheckedChanged);
            // 
            // c_random
            // 
            this.c_random.AutoSize = true;
            this.c_random.Enabled = false;
            this.c_random.Location = new System.Drawing.Point(23, 126);
            this.c_random.Name = "c_random";
            this.c_random.Size = new System.Drawing.Size(66, 17);
            this.c_random.TabIndex = 44;
            this.c_random.Text = "Random";
            this.c_random.UseVisualStyleBackColor = true;
            this.c_random.CheckedChanged += new System.EventHandler(this.c_random_CheckedChanged);
            this.c_random.EnabledChanged += new System.EventHandler(this.c_random_EnabledChanged);
            // 
            // c_max
            // 
            this.c_max.AutoSize = true;
            this.c_max.Enabled = false;
            this.c_max.Location = new System.Drawing.Point(23, 149);
            this.c_max.Name = "c_max";
            this.c_max.Size = new System.Drawing.Size(46, 17);
            this.c_max.TabIndex = 45;
            this.c_max.Text = "Max";
            this.c_max.UseVisualStyleBackColor = true;
            this.c_max.CheckedChanged += new System.EventHandler(this.c_max_CheckedChanged);
            this.c_max.EnabledChanged += new System.EventHandler(this.c_max_EnabledChanged);
            // 
            // c_default
            // 
            this.c_default.AutoSize = true;
            this.c_default.Enabled = false;
            this.c_default.Location = new System.Drawing.Point(336, 146);
            this.c_default.Name = "c_default";
            this.c_default.Size = new System.Drawing.Size(115, 17);
            this.c_default.TabIndex = 46;
            this.c_default.Text = "Starting Equipment";
            this.c_default.UseVisualStyleBackColor = true;
            this.c_default.CheckedChanged += new System.EventHandler(this.c_default_CheckedChanged);
            this.c_default.EnabledChanged += new System.EventHandler(this.c_default_EnabledChanged);
            // 
            // c_basestats
            // 
            this.c_basestats.AutoSize = true;
            this.c_basestats.Enabled = false;
            this.c_basestats.Location = new System.Drawing.Point(336, 124);
            this.c_basestats.Name = "c_basestats";
            this.c_basestats.Size = new System.Drawing.Size(77, 17);
            this.c_basestats.TabIndex = 47;
            this.c_basestats.Text = "Base Stats";
            this.c_basestats.UseVisualStyleBackColor = true;
            this.c_basestats.CheckedChanged += new System.EventHandler(this.c_basestats_CheckedChanged);
            // 
            // c_abilitygems
            // 
            this.c_abilitygems.AutoSize = true;
            this.c_abilitygems.Enabled = false;
            this.c_abilitygems.Location = new System.Drawing.Point(336, 103);
            this.c_abilitygems.Name = "c_abilitygems";
            this.c_abilitygems.Size = new System.Drawing.Size(90, 17);
            this.c_abilitygems.TabIndex = 48;
            this.c_abilitygems.Text = "Abilitys\' Gems";
            this.c_abilitygems.UseVisualStyleBackColor = true;
            this.c_abilitygems.CheckedChanged += new System.EventHandler(this.c_abilitygems_CheckedChanged);
            // 
            // l_shopm
            // 
            this.l_shopm.AutoSize = true;
            this.l_shopm.Enabled = false;
            this.l_shopm.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.l_shopm.Location = new System.Drawing.Point(10, 176);
            this.l_shopm.Name = "l_shopm";
            this.l_shopm.Size = new System.Drawing.Size(91, 13);
            this.l_shopm.TabIndex = 49;
            this.l_shopm.Text = "Shop Modifiers";
            // 
            // c_random_e
            // 
            this.c_random_e.AutoSize = true;
            this.c_random_e.Enabled = false;
            this.c_random_e.Location = new System.Drawing.Point(348, 192);
            this.c_random_e.Name = "c_random_e";
            this.c_random_e.Size = new System.Drawing.Size(104, 17);
            this.c_random_e.TabIndex = 50;
            this.c_random_e.Text = "random no types";
            this.c_random_e.UseVisualStyleBackColor = true;
            this.c_random_e.CheckedChanged += new System.EventHandler(this.c_random_e_CheckedChanged);
            // 
            // c_main_e
            // 
            this.c_main_e.AutoSize = true;
            this.c_main_e.Enabled = false;
            this.c_main_e.Location = new System.Drawing.Point(348, 169);
            this.c_main_e.Name = "c_main_e";
            this.c_main_e.Size = new System.Drawing.Size(93, 17);
            this.c_main_e.TabIndex = 51;
            this.c_main_e.Text = "maintain types";
            this.c_main_e.UseVisualStyleBackColor = true;
            this.c_main_e.CheckedChanged += new System.EventHandler(this.c_main_e_CheckedChanged);
            // 
            // c_all_e
            // 
            this.c_all_e.AutoSize = true;
            this.c_all_e.Enabled = false;
            this.c_all_e.Location = new System.Drawing.Point(348, 215);
            this.c_all_e.Name = "c_all_e";
            this.c_all_e.Size = new System.Drawing.Size(93, 17);
            this.c_all_e.TabIndex = 53;
            this.c_all_e.Text = "share all types";
            this.c_all_e.UseVisualStyleBackColor = true;
            this.c_all_e.CheckedChanged += new System.EventHandler(this.c_all_e_CheckedChanged);
            // 
            // rand9er
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(632, 829);
            this.Controls.Add(this.c_all_e);
            this.Controls.Add(this.c_main_e);
            this.Controls.Add(this.c_random_e);
            this.Controls.Add(this.l_shopm);
            this.Controls.Add(this.c_abilitygems);
            this.Controls.Add(this.c_basestats);
            this.Controls.Add(this.c_default);
            this.Controls.Add(this.c_max);
            this.Controls.Add(this.c_random);
            this.Controls.Add(this.c_safe);
            this.Controls.Add(this.cm_char);
            this.Controls.Add(this.cm_itemshop);
            this.Controls.Add(this.cm_synth);
            this.Controls.Add(this.c_medicshops);
            this.Controls.Add(this.c_prices);
            this.Controls.Add(this.c_medicitems);
            this.Controls.Add(this.c_result);
            this.Controls.Add(this.l_seed);
            this.Controls.Add(this.c_require);
            this.Controls.Add(this.pbar_seed);
            this.Controls.Add(this.l_leaf);
            this.Controls.Add(this.l_branch);
            this.Controls.Add(this.pbar_leaf);
            this.Controls.Add(this.b_rw);
            this.Controls.Add(this.l_tree);
            this.Controls.Add(this.b_search);
            this.Controls.Add(this.b_restore);
            this.Controls.Add(this.tb_fl);
            this.Controls.Add(this.b_open);
            this.Controls.Add(this.b_rseed);
            this.Controls.Add(this.pbar_tree);
            this.Controls.Add(this.pbar_branch);
            this.Controls.Add(this.clear);
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
            this.Text = "FFIX Randomizer Assistant 0.88";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_yuno)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox_output;
        private System.Windows.Forms.Button button_rand;
        private System.Windows.Forms.Label label_output;
        private System.Windows.Forms.Label label_seed;
        private System.Windows.Forms.TextBox textBox_seed;
        private System.Windows.Forms.RichTextBox richTextBox_debug;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox_yuno;
        private System.Windows.Forms.CheckBox c_medicitems;
        private System.Windows.Forms.CheckBox c_medicshops;
        private System.Windows.Forms.CheckBox c_result;
        private System.Windows.Forms.CheckBox c_require;
        private System.Windows.Forms.Button clear;
        private System.Windows.Forms.ProgressBar pbar_branch;
        private System.Windows.Forms.ProgressBar pbar_tree;
        private System.Windows.Forms.Button b_rseed;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button b_open;
        private System.Windows.Forms.TextBox tb_fl;
        private System.Windows.Forms.Button b_restore;
        private System.Windows.Forms.Button b_search;
        private System.Windows.Forms.Label l_tree;
        private System.Windows.Forms.Button b_rw;
        private System.Windows.Forms.CheckBox c_prices;
        private System.Windows.Forms.ProgressBar pbar_leaf;
        private System.Windows.Forms.Label l_branch;
        private System.Windows.Forms.Label l_leaf;
        private System.Windows.Forms.ProgressBar pbar_seed;
        private System.Windows.Forms.Label l_seed;
        private System.Windows.Forms.CheckBox cm_synth;
        private System.Windows.Forms.CheckBox cm_itemshop;
        private System.Windows.Forms.CheckBox cm_char;
        private System.Windows.Forms.CheckBox c_safe;
        private System.Windows.Forms.CheckBox c_random;
        private System.Windows.Forms.CheckBox c_max;
        private System.Windows.Forms.CheckBox c_default;
        private System.Windows.Forms.CheckBox c_basestats;
        private System.Windows.Forms.CheckBox c_abilitygems;
        private System.Windows.Forms.Label l_shopm;
        private System.Windows.Forms.CheckBox c_random_e;
        private System.Windows.Forms.CheckBox c_main_e;
        private System.Windows.Forms.CheckBox c_all_e;
    }
}

