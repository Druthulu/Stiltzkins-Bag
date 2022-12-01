
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(rand9er));
            this.richTextBox_output = new System.Windows.Forms.RichTextBox();
            this.label_seed = new System.Windows.Forms.Label();
            this.textBox_seed = new System.Windows.Forms.TextBox();
            this.richTextBox_debug = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.c_prices = new System.Windows.Forms.CheckBox();
            this.c_result = new System.Windows.Forms.CheckBox();
            this.c_require = new System.Windows.Forms.CheckBox();
            this.pbar_tree = new System.Windows.Forms.ProgressBar();
            this.b_rseed = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.b_open = new System.Windows.Forms.Button();
            this.tb_fl = new System.Windows.Forms.TextBox();
            this.b_restore = new System.Windows.Forms.Button();
            this.b_search = new System.Windows.Forms.Button();
            this.cm_synth = new System.Windows.Forms.CheckBox();
            this.cm_itemshop = new System.Windows.Forms.CheckBox();
            this.cm_char = new System.Windows.Forms.CheckBox();
            this.c_default = new System.Windows.Forms.CheckBox();
            this.c_basestats = new System.Windows.Forms.CheckBox();
            this.c_abilitygems = new System.Windows.Forms.CheckBox();
            this.c_random_e = new System.Windows.Forms.CheckBox();
            this.c_main_e = new System.Windows.Forms.CheckBox();
            this.c_all_e = new System.Windows.Forms.CheckBox();
            this.button_rand = new System.Windows.Forms.Button();
            this.cm_entrances = new System.Windows.Forms.CheckBox();
            this.cm_enemies = new System.Windows.Forms.CheckBox();
            this.c_esteals = new System.Windows.Forms.CheckBox();
            this.c_edrops = new System.Windows.Forms.CheckBox();
            this.c_es1 = new System.Windows.Forms.CheckBox();
            this.c_es4 = new System.Windows.Forms.CheckBox();
            this.c_ed4 = new System.Windows.Forms.CheckBox();
            this.c_ed1 = new System.Windows.Forms.CheckBox();
            this.cm_treasure = new System.Windows.Forms.CheckBox();
            this.cm_stiltzkin = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Serial_But = new System.Windows.Forms.CheckBox();
            this.checkBoxRecommended = new System.Windows.Forms.CheckBox();
            this.checkBoxChaos = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.checkBox6 = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // richTextBox_output
            // 
            this.richTextBox_output.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox_output.ForeColor = System.Drawing.SystemColors.InfoText;
            this.richTextBox_output.Location = new System.Drawing.Point(44, 318);
            this.richTextBox_output.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox_output.Name = "richTextBox_output";
            this.richTextBox_output.ReadOnly = true;
            this.richTextBox_output.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox_output.Size = new System.Drawing.Size(160, 143);
            this.richTextBox_output.TabIndex = 1;
            this.richTextBox_output.Text = "int getRandomNumber() {\n     return 4; // chosen by fair dice roll.\n             " +
    "       // guaranteed to be random.\n}";
            this.richTextBox_output.WordWrap = false;
            this.richTextBox_output.TextChanged += new System.EventHandler(this.richTextBox_output_TextChanged);
            // 
            // label_seed
            // 
            this.label_seed.AutoSize = true;
            this.label_seed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label_seed.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_seed.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label_seed.Location = new System.Drawing.Point(10, 14);
            this.label_seed.Name = "label_seed";
            this.label_seed.Size = new System.Drawing.Size(48, 16);
            this.label_seed.TabIndex = 12;
            this.label_seed.Text = "SEED";
            // 
            // textBox_seed
            // 
            this.textBox_seed.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_seed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_seed.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_seed.Location = new System.Drawing.Point(75, 11);
            this.textBox_seed.Name = "textBox_seed";
            this.textBox_seed.Size = new System.Drawing.Size(638, 20);
            this.textBox_seed.TabIndex = 13;
            this.textBox_seed.TextChanged += new System.EventHandler(this.textBox_seed_TextChanged);
            // 
            // richTextBox_debug
            // 
            this.richTextBox_debug.Location = new System.Drawing.Point(220, 368);
            this.richTextBox_debug.Name = "richTextBox_debug";
            this.richTextBox_debug.ReadOnly = true;
            this.richTextBox_debug.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox_debug.Size = new System.Drawing.Size(221, 93);
            this.richTextBox_debug.TabIndex = 14;
            this.richTextBox_debug.Text = "no bugs yet";
            this.richTextBox_debug.TextChanged += new System.EventHandler(this.richTextBox_debug_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label1.Image = global::rand9er.Properties.Resources.bgc;
            this.label1.Location = new System.Drawing.Point(388, 347);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Debug";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // c_prices
            // 
            this.c_prices.AutoSize = true;
            this.c_prices.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_prices.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_prices.Enabled = false;
            this.c_prices.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_prices.ForeColor = System.Drawing.Color.White;
            this.c_prices.Location = new System.Drawing.Point(84, 205);
            this.c_prices.Name = "c_prices";
            this.c_prices.Size = new System.Drawing.Size(61, 17);
            this.c_prices.TabIndex = 29;
            this.c_prices.Text = "Prices";
            this.c_prices.UseVisualStyleBackColor = false;
            this.c_prices.CheckedChanged += new System.EventHandler(this.c_prices_CheckedChanged);
            // 
            // c_result
            // 
            this.c_result.AutoSize = true;
            this.c_result.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_result.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_result.Enabled = false;
            this.c_result.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_result.ForeColor = System.Drawing.Color.White;
            this.c_result.Location = new System.Drawing.Point(84, 182);
            this.c_result.Margin = new System.Windows.Forms.Padding(2);
            this.c_result.Name = "c_result";
            this.c_result.Size = new System.Drawing.Size(62, 17);
            this.c_result.TabIndex = 28;
            this.c_result.Text = "Result";
            this.c_result.UseVisualStyleBackColor = false;
            this.c_result.CheckedChanged += new System.EventHandler(this.c_result_CheckedChanged);
            // 
            // c_require
            // 
            this.c_require.AutoSize = true;
            this.c_require.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_require.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_require.Enabled = false;
            this.c_require.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_require.ForeColor = System.Drawing.Color.White;
            this.c_require.Location = new System.Drawing.Point(84, 159);
            this.c_require.Margin = new System.Windows.Forms.Padding(2);
            this.c_require.Name = "c_require";
            this.c_require.Size = new System.Drawing.Size(103, 17);
            this.c_require.TabIndex = 26;
            this.c_require.Text = "Requirements";
            this.c_require.UseVisualStyleBackColor = false;
            this.c_require.CheckedChanged += new System.EventHandler(this.c_require_CheckedChanged);
            // 
            // pbar_tree
            // 
            this.pbar_tree.Location = new System.Drawing.Point(635, 433);
            this.pbar_tree.Margin = new System.Windows.Forms.Padding(2);
            this.pbar_tree.Name = "pbar_tree";
            this.pbar_tree.Size = new System.Drawing.Size(175, 10);
            this.pbar_tree.TabIndex = 22;
            this.pbar_tree.Click += new System.EventHandler(this.pbar_tree_Click);
            // 
            // b_rseed
            // 
            this.b_rseed.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.b_rseed.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.b_rseed.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_rseed.FlatAppearance.BorderSize = 3;
            this.b_rseed.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_rseed.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_rseed.ForeColor = System.Drawing.Color.White;
            this.b_rseed.Location = new System.Drawing.Point(718, 14);
            this.b_rseed.Margin = new System.Windows.Forms.Padding(2);
            this.b_rseed.Name = "b_rseed";
            this.b_rseed.Size = new System.Drawing.Size(101, 19);
            this.b_rseed.TabIndex = 23;
            this.b_rseed.Text = "Random Seed";
            this.b_rseed.UseVisualStyleBackColor = false;
            this.b_rseed.Click += new System.EventHandler(this.b_rseed_Click_1);
            // 
            // b_open
            // 
            this.b_open.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.b_open.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.b_open.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_open.FlatAppearance.BorderSize = 3;
            this.b_open.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_open.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_open.ForeColor = System.Drawing.Color.White;
            this.b_open.Location = new System.Drawing.Point(661, 42);
            this.b_open.Margin = new System.Windows.Forms.Padding(2);
            this.b_open.Name = "b_open";
            this.b_open.Size = new System.Drawing.Size(158, 20);
            this.b_open.TabIndex = 24;
            this.b_open.Text = "Choose FFIX location";
            this.b_open.UseVisualStyleBackColor = false;
            this.b_open.Click += new System.EventHandler(this.b_open_Click);
            // 
            // tb_fl
            // 
            this.tb_fl.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tb_fl.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_fl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tb_fl.Location = new System.Drawing.Point(75, 40);
            this.tb_fl.Margin = new System.Windows.Forms.Padding(2);
            this.tb_fl.Name = "tb_fl";
            this.tb_fl.Size = new System.Drawing.Size(475, 20);
            this.tb_fl.TabIndex = 25;
            this.tb_fl.TextChanged += new System.EventHandler(this.tb_fl_TextChanged);
            // 
            // b_restore
            // 
            this.b_restore.AutoEllipsis = true;
            this.b_restore.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.b_restore.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.b_restore.Enabled = false;
            this.b_restore.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_restore.FlatAppearance.BorderSize = 3;
            this.b_restore.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_restore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_restore.ForeColor = System.Drawing.Color.White;
            this.b_restore.Location = new System.Drawing.Point(486, 426);
            this.b_restore.Margin = new System.Windows.Forms.Padding(2);
            this.b_restore.Name = "b_restore";
            this.b_restore.Size = new System.Drawing.Size(101, 35);
            this.b_restore.TabIndex = 26;
            this.b_restore.Text = "Restore Stock Checked Files";
            this.b_restore.UseVisualStyleBackColor = false;
            this.b_restore.Click += new System.EventHandler(this.b_restore_Click);
            // 
            // b_search
            // 
            this.b_search.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.b_search.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.b_search.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_search.FlatAppearance.BorderSize = 3;
            this.b_search.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_search.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_search.ForeColor = System.Drawing.Color.White;
            this.b_search.Location = new System.Drawing.Point(555, 41);
            this.b_search.Name = "b_search";
            this.b_search.Size = new System.Drawing.Size(101, 22);
            this.b_search.TabIndex = 27;
            this.b_search.Text = "Auto Locate";
            this.b_search.UseVisualStyleBackColor = false;
            this.b_search.Click += new System.EventHandler(this.b_search_Click);
            // 
            // cm_synth
            // 
            this.cm_synth.AutoSize = true;
            this.cm_synth.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_synth.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_synth.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_synth.ForeColor = System.Drawing.Color.White;
            this.cm_synth.Location = new System.Drawing.Point(64, 132);
            this.cm_synth.Name = "cm_synth";
            this.cm_synth.Size = new System.Drawing.Size(100, 22);
            this.cm_synth.TabIndex = 40;
            this.cm_synth.Text = "Synthesis";
            this.cm_synth.UseVisualStyleBackColor = false;
            this.cm_synth.CheckedChanged += new System.EventHandler(this.cm_synth_CheckedChanged);
            // 
            // cm_itemshop
            // 
            this.cm_itemshop.AutoSize = true;
            this.cm_itemshop.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_itemshop.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_itemshop.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_itemshop.ForeColor = System.Drawing.Color.White;
            this.cm_itemshop.Location = new System.Drawing.Point(64, 104);
            this.cm_itemshop.Name = "cm_itemshop";
            this.cm_itemshop.Size = new System.Drawing.Size(112, 22);
            this.cm_itemshop.TabIndex = 41;
            this.cm_itemshop.Text = "Item Shops";
            this.cm_itemshop.UseVisualStyleBackColor = false;
            this.cm_itemshop.CheckedChanged += new System.EventHandler(this.cm_itemshop_CheckedChanged);
            // 
            // cm_char
            // 
            this.cm_char.AutoSize = true;
            this.cm_char.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_char.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_char.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_char.ForeColor = System.Drawing.Color.White;
            this.cm_char.Location = new System.Drawing.Point(249, 76);
            this.cm_char.Name = "cm_char";
            this.cm_char.Size = new System.Drawing.Size(110, 22);
            this.cm_char.TabIndex = 42;
            this.cm_char.Text = "Characters";
            this.cm_char.UseVisualStyleBackColor = false;
            this.cm_char.CheckedChanged += new System.EventHandler(this.cm_char_CheckedChanged);
            // 
            // c_default
            // 
            this.c_default.AutoSize = true;
            this.c_default.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_default.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_default.Enabled = false;
            this.c_default.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_default.ForeColor = System.Drawing.Color.White;
            this.c_default.Location = new System.Drawing.Point(269, 150);
            this.c_default.Name = "c_default";
            this.c_default.Size = new System.Drawing.Size(133, 17);
            this.c_default.TabIndex = 46;
            this.c_default.Text = "Starting Equipment";
            this.c_default.UseVisualStyleBackColor = false;
            this.c_default.CheckedChanged += new System.EventHandler(this.c_default_CheckedChanged);
            this.c_default.EnabledChanged += new System.EventHandler(this.c_default_EnabledChanged);
            // 
            // c_basestats
            // 
            this.c_basestats.AutoSize = true;
            this.c_basestats.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_basestats.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_basestats.Enabled = false;
            this.c_basestats.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_basestats.ForeColor = System.Drawing.Color.White;
            this.c_basestats.Location = new System.Drawing.Point(269, 127);
            this.c_basestats.Name = "c_basestats";
            this.c_basestats.Size = new System.Drawing.Size(87, 17);
            this.c_basestats.TabIndex = 47;
            this.c_basestats.Text = "Base Stats";
            this.c_basestats.UseVisualStyleBackColor = false;
            this.c_basestats.CheckedChanged += new System.EventHandler(this.c_basestats_CheckedChanged);
            // 
            // c_abilitygems
            // 
            this.c_abilitygems.AutoSize = true;
            this.c_abilitygems.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_abilitygems.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_abilitygems.Enabled = false;
            this.c_abilitygems.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_abilitygems.ForeColor = System.Drawing.Color.White;
            this.c_abilitygems.Location = new System.Drawing.Point(269, 104);
            this.c_abilitygems.Name = "c_abilitygems";
            this.c_abilitygems.Size = new System.Drawing.Size(118, 17);
            this.c_abilitygems.TabIndex = 48;
            this.c_abilitygems.Text = "Ability Gem Cost";
            this.c_abilitygems.UseVisualStyleBackColor = false;
            this.c_abilitygems.CheckedChanged += new System.EventHandler(this.c_abilitygems_CheckedChanged);
            // 
            // c_random_e
            // 
            this.c_random_e.AutoSize = true;
            this.c_random_e.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_random_e.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_random_e.Enabled = false;
            this.c_random_e.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_random_e.ForeColor = System.Drawing.Color.White;
            this.c_random_e.Location = new System.Drawing.Point(289, 196);
            this.c_random_e.Name = "c_random_e";
            this.c_random_e.Size = new System.Drawing.Size(112, 17);
            this.c_random_e.TabIndex = 50;
            this.c_random_e.Text = "random no sets";
            this.c_random_e.UseVisualStyleBackColor = false;
            this.c_random_e.CheckedChanged += new System.EventHandler(this.c_random_e_CheckedChanged);
            // 
            // c_main_e
            // 
            this.c_main_e.AutoSize = true;
            this.c_main_e.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_main_e.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_main_e.Enabled = false;
            this.c_main_e.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_main_e.ForeColor = System.Drawing.Color.White;
            this.c_main_e.Location = new System.Drawing.Point(289, 173);
            this.c_main_e.Name = "c_main_e";
            this.c_main_e.Size = new System.Drawing.Size(133, 17);
            this.c_main_e.TabIndex = 51;
            this.c_main_e.Text = "keep original types";
            this.c_main_e.UseVisualStyleBackColor = false;
            this.c_main_e.CheckedChanged += new System.EventHandler(this.c_main_e_CheckedChanged);
            // 
            // c_all_e
            // 
            this.c_all_e.AutoSize = true;
            this.c_all_e.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_all_e.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_all_e.Enabled = false;
            this.c_all_e.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_all_e.ForeColor = System.Drawing.Color.White;
            this.c_all_e.Location = new System.Drawing.Point(289, 220);
            this.c_all_e.Name = "c_all_e";
            this.c_all_e.Size = new System.Drawing.Size(101, 17);
            this.c_all_e.TabIndex = 53;
            this.c_all_e.Text = "share all sets";
            this.c_all_e.UseVisualStyleBackColor = false;
            this.c_all_e.CheckedChanged += new System.EventHandler(this.c_all_e_CheckedChanged);
            // 
            // button_rand
            // 
            this.button_rand.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button_rand.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.button_rand.FlatAppearance.BorderSize = 3;
            this.button_rand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button_rand.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_rand.ForeColor = System.Drawing.Color.White;
            this.button_rand.Location = new System.Drawing.Point(635, 365);
            this.button_rand.Margin = new System.Windows.Forms.Padding(2);
            this.button_rand.Name = "button_rand";
            this.button_rand.Size = new System.Drawing.Size(176, 50);
            this.button_rand.TabIndex = 2;
            this.button_rand.Text = "Buy for 333 Gil";
            this.button_rand.UseVisualStyleBackColor = false;
            this.button_rand.Click += new System.EventHandler(this.button_rand_Click);
            // 
            // cm_entrances
            // 
            this.cm_entrances.AutoSize = true;
            this.cm_entrances.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_entrances.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_entrances.Enabled = false;
            this.cm_entrances.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_entrances.ForeColor = System.Drawing.Color.White;
            this.cm_entrances.Location = new System.Drawing.Point(667, 293);
            this.cm_entrances.Name = "cm_entrances";
            this.cm_entrances.Size = new System.Drawing.Size(144, 22);
            this.cm_entrances.TabIndex = 56;
            this.cm_entrances.Text = "Field Entrances";
            this.cm_entrances.UseVisualStyleBackColor = false;
            this.cm_entrances.CheckedChanged += new System.EventHandler(this.cm_entrances_CheckedChanged);
            // 
            // cm_enemies
            // 
            this.cm_enemies.AutoSize = true;
            this.cm_enemies.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_enemies.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_enemies.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_enemies.ForeColor = System.Drawing.Color.White;
            this.cm_enemies.Location = new System.Drawing.Point(462, 76);
            this.cm_enemies.Name = "cm_enemies";
            this.cm_enemies.Size = new System.Drawing.Size(92, 22);
            this.cm_enemies.TabIndex = 60;
            this.cm_enemies.Text = "Enemies";
            this.cm_enemies.UseVisualStyleBackColor = false;
            this.cm_enemies.CheckedChanged += new System.EventHandler(this.cm_enemies_CheckedChanged);
            // 
            // c_esteals
            // 
            this.c_esteals.AutoSize = true;
            this.c_esteals.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_esteals.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_esteals.Enabled = false;
            this.c_esteals.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_esteals.ForeColor = System.Drawing.Color.White;
            this.c_esteals.Location = new System.Drawing.Point(483, 104);
            this.c_esteals.Name = "c_esteals";
            this.c_esteals.Size = new System.Drawing.Size(89, 17);
            this.c_esteals.TabIndex = 61;
            this.c_esteals.Text = "Item Steals";
            this.c_esteals.UseVisualStyleBackColor = false;
            this.c_esteals.CheckedChanged += new System.EventHandler(this.c_esteals_CheckedChanged);
            // 
            // c_edrops
            // 
            this.c_edrops.AutoSize = true;
            this.c_edrops.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_edrops.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_edrops.Enabled = false;
            this.c_edrops.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_edrops.ForeColor = System.Drawing.Color.White;
            this.c_edrops.Location = new System.Drawing.Point(483, 127);
            this.c_edrops.Name = "c_edrops";
            this.c_edrops.Size = new System.Drawing.Size(87, 17);
            this.c_edrops.TabIndex = 62;
            this.c_edrops.Text = "Item Drops";
            this.c_edrops.UseVisualStyleBackColor = false;
            this.c_edrops.CheckedChanged += new System.EventHandler(this.c_edrops_CheckedChanged);
            // 
            // c_es1
            // 
            this.c_es1.AutoSize = true;
            this.c_es1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_es1.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_es1.Enabled = false;
            this.c_es1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_es1.ForeColor = System.Drawing.Color.White;
            this.c_es1.Location = new System.Drawing.Point(483, 174);
            this.c_es1.Name = "c_es1";
            this.c_es1.Size = new System.Drawing.Size(139, 17);
            this.c_es1.TabIndex = 63;
            this.c_es1.Text = "Blue Magic Learned";
            this.c_es1.UseVisualStyleBackColor = false;
            this.c_es1.CheckedChanged += new System.EventHandler(this.c_es1_CheckedChanged);
            // 
            // c_es4
            // 
            this.c_es4.AutoSize = true;
            this.c_es4.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_es4.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_es4.Enabled = false;
            this.c_es4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_es4.ForeColor = System.Drawing.Color.White;
            this.c_es4.Location = new System.Drawing.Point(685, 152);
            this.c_es4.Name = "c_es4";
            this.c_es4.Size = new System.Drawing.Size(126, 17);
            this.c_es4.TabIndex = 66;
            this.c_es4.Text = "Card\'s Base Stats";
            this.c_es4.UseVisualStyleBackColor = false;
            this.c_es4.CheckedChanged += new System.EventHandler(this.c_es4_CheckedChanged);
            // 
            // c_ed4
            // 
            this.c_ed4.AutoSize = true;
            this.c_ed4.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_ed4.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_ed4.Enabled = false;
            this.c_ed4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_ed4.ForeColor = System.Drawing.Color.White;
            this.c_ed4.Location = new System.Drawing.Point(685, 128);
            this.c_ed4.Name = "c_ed4";
            this.c_ed4.Size = new System.Drawing.Size(103, 17);
            this.c_ed4.TabIndex = 70;
            this.c_ed4.Text = "Enemy Decks";
            this.c_ed4.UseVisualStyleBackColor = false;
            this.c_ed4.CheckedChanged += new System.EventHandler(this.c_ed4_CheckedChanged);
            // 
            // c_ed1
            // 
            this.c_ed1.AutoSize = true;
            this.c_ed1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_ed1.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_ed1.Enabled = false;
            this.c_ed1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_ed1.ForeColor = System.Drawing.Color.White;
            this.c_ed1.Location = new System.Drawing.Point(483, 150);
            this.c_ed1.Name = "c_ed1";
            this.c_ed1.Size = new System.Drawing.Size(83, 17);
            this.c_ed1.TabIndex = 67;
            this.c_ed1.Text = "Card Drop";
            this.c_ed1.UseVisualStyleBackColor = false;
            this.c_ed1.CheckedChanged += new System.EventHandler(this.c_ed1_CheckedChanged);
            // 
            // cm_treasure
            // 
            this.cm_treasure.AutoSize = true;
            this.cm_treasure.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_treasure.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_treasure.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_treasure.ForeColor = System.Drawing.Color.White;
            this.cm_treasure.Location = new System.Drawing.Point(64, 76);
            this.cm_treasure.Name = "cm_treasure";
            this.cm_treasure.Size = new System.Drawing.Size(140, 22);
            this.cm_treasure.TabIndex = 74;
            this.cm_treasure.Text = "Treasure Items";
            this.cm_treasure.UseVisualStyleBackColor = false;
            this.cm_treasure.CheckedChanged += new System.EventHandler(this.cm_treasure_CheckedChanged);
            // 
            // cm_stiltzkin
            // 
            this.cm_stiltzkin.AutoSize = true;
            this.cm_stiltzkin.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_stiltzkin.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_stiltzkin.Enabled = false;
            this.cm_stiltzkin.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_stiltzkin.ForeColor = System.Drawing.Color.White;
            this.cm_stiltzkin.Location = new System.Drawing.Point(667, 321);
            this.cm_stiltzkin.Name = "cm_stiltzkin";
            this.cm_stiltzkin.Size = new System.Drawing.Size(134, 22);
            this.cm_stiltzkin.TabIndex = 75;
            this.cm_stiltzkin.Text = "Stiltzkin\'s Bag";
            this.cm_stiltzkin.UseVisualStyleBackColor = false;
            this.cm_stiltzkin.CheckedChanged += new System.EventHandler(this.cm_stiltzkin_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label2.Location = new System.Drawing.Point(462, 347);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(142, 16);
            this.label2.TabIndex = 81;
            this.label2.Text = "Randomize Version";
            // 
            // Serial_But
            // 
            this.Serial_But.AutoSize = true;
            this.Serial_But.BackColor = System.Drawing.SystemColors.ControlDark;
            this.Serial_But.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.Serial_But.Enabled = false;
            this.Serial_But.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Serial_But.ForeColor = System.Drawing.Color.White;
            this.Serial_But.Location = new System.Drawing.Point(324, 343);
            this.Serial_But.Name = "Serial_But";
            this.Serial_But.Size = new System.Drawing.Size(58, 17);
            this.Serial_But.TabIndex = 82;
            this.Serial_But.Text = "Serial";
            this.Serial_But.UseVisualStyleBackColor = false;
            this.Serial_But.CheckedChanged += new System.EventHandler(this.Serial_But_CheckedChanged);
            // 
            // checkBoxRecommended
            // 
            this.checkBoxRecommended.AutoSize = true;
            this.checkBoxRecommended.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkBoxRecommended.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.checkBoxRecommended.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxRecommended.ForeColor = System.Drawing.Color.White;
            this.checkBoxRecommended.Location = new System.Drawing.Point(488, 375);
            this.checkBoxRecommended.Name = "checkBoxRecommended";
            this.checkBoxRecommended.Size = new System.Drawing.Size(109, 17);
            this.checkBoxRecommended.TabIndex = 83;
            this.checkBoxRecommended.Text = "Recommended";
            this.checkBoxRecommended.UseVisualStyleBackColor = false;
            // 
            // checkBoxChaos
            // 
            this.checkBoxChaos.AutoSize = true;
            this.checkBoxChaos.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkBoxChaos.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.checkBoxChaos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxChaos.ForeColor = System.Drawing.Color.White;
            this.checkBoxChaos.Location = new System.Drawing.Point(488, 396);
            this.checkBoxChaos.Name = "checkBoxChaos";
            this.checkBoxChaos.Size = new System.Drawing.Size(116, 17);
            this.checkBoxChaos.TabIndex = 84;
            this.checkBoxChaos.Text = "Maximum Chaos";
            this.checkBoxChaos.UseVisualStyleBackColor = false;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkBox4.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.checkBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox4.ForeColor = System.Drawing.Color.White;
            this.checkBox4.Location = new System.Drawing.Point(659, 76);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(118, 22);
            this.checkBox4.TabIndex = 85;
            this.checkBox4.Text = "TetraMaster";
            this.checkBox4.UseVisualStyleBackColor = false;
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkBox5.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.checkBox5.Enabled = false;
            this.checkBox5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox5.ForeColor = System.Drawing.Color.White;
            this.checkBox5.Location = new System.Drawing.Point(685, 175);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(134, 17);
            this.checkBox5.TabIndex = 86;
            this.checkBox5.Text = "Card\'s Attack Type";
            this.checkBox5.UseVisualStyleBackColor = false;
            // 
            // checkBox6
            // 
            this.checkBox6.AutoSize = true;
            this.checkBox6.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkBox6.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.checkBox6.Enabled = false;
            this.checkBox6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox6.ForeColor = System.Drawing.Color.White;
            this.checkBox6.Location = new System.Drawing.Point(685, 104);
            this.checkBox6.Name = "checkBox6";
            this.checkBox6.Size = new System.Drawing.Size(117, 17);
            this.checkBox6.TabIndex = 87;
            this.checkBox6.Text = "Enemy Difficulty";
            this.checkBox6.UseVisualStyleBackColor = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(10, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 16);
            this.label3.TabIndex = 88;
            this.label3.Text = "FF9 Dir";
            // 
            // rand9er
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.ClientSize = new System.Drawing.Size(863, 503);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBox6);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBoxChaos);
            this.Controls.Add(this.checkBoxRecommended);
            this.Controls.Add(this.Serial_But);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cm_stiltzkin);
            this.Controls.Add(this.cm_treasure);
            this.Controls.Add(this.c_ed4);
            this.Controls.Add(this.c_ed1);
            this.Controls.Add(this.c_es4);
            this.Controls.Add(this.c_es1);
            this.Controls.Add(this.c_edrops);
            this.Controls.Add(this.c_esteals);
            this.Controls.Add(this.cm_enemies);
            this.Controls.Add(this.cm_entrances);
            this.Controls.Add(this.c_all_e);
            this.Controls.Add(this.c_main_e);
            this.Controls.Add(this.c_random_e);
            this.Controls.Add(this.c_abilitygems);
            this.Controls.Add(this.c_basestats);
            this.Controls.Add(this.c_default);
            this.Controls.Add(this.cm_char);
            this.Controls.Add(this.cm_itemshop);
            this.Controls.Add(this.cm_synth);
            this.Controls.Add(this.c_prices);
            this.Controls.Add(this.c_result);
            this.Controls.Add(this.c_require);
            this.Controls.Add(this.b_search);
            this.Controls.Add(this.b_restore);
            this.Controls.Add(this.tb_fl);
            this.Controls.Add(this.b_open);
            this.Controls.Add(this.b_rseed);
            this.Controls.Add(this.pbar_tree);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox_debug);
            this.Controls.Add(this.textBox_seed);
            this.Controls.Add(this.label_seed);
            this.Controls.Add(this.button_rand);
            this.Controls.Add(this.richTextBox_output);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "rand9er";
            this.Text = "FFIX Stiltzkin\'s bag 2.2";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox_output;
        private System.Windows.Forms.Button button_rand;
        private System.Windows.Forms.Label label_seed;
        private System.Windows.Forms.RichTextBox richTextBox_debug;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox c_result;
        private System.Windows.Forms.CheckBox c_require;
        private System.Windows.Forms.ProgressBar pbar_tree;
        private System.Windows.Forms.Button b_rseed;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button b_open;
        private System.Windows.Forms.TextBox tb_fl;
        private System.Windows.Forms.Button b_restore;
        private System.Windows.Forms.Button b_search;
        private System.Windows.Forms.CheckBox c_prices;
        private System.Windows.Forms.CheckBox cm_synth;
        private System.Windows.Forms.CheckBox cm_itemshop;
        private System.Windows.Forms.CheckBox cm_char;
        private System.Windows.Forms.CheckBox c_default;
        private System.Windows.Forms.CheckBox c_basestats;
        private System.Windows.Forms.CheckBox c_abilitygems;
        private System.Windows.Forms.CheckBox c_random_e;
        private System.Windows.Forms.CheckBox c_main_e;
        private System.Windows.Forms.CheckBox c_all_e;
        private System.Windows.Forms.CheckBox cm_entrances;
        private System.Windows.Forms.CheckBox cm_enemies;
        private System.Windows.Forms.CheckBox c_esteals;
        private System.Windows.Forms.CheckBox c_edrops;
        private System.Windows.Forms.CheckBox c_es1;
        private System.Windows.Forms.CheckBox c_es4;
        private System.Windows.Forms.CheckBox c_ed4;
        private System.Windows.Forms.CheckBox c_ed1;
        private System.Windows.Forms.CheckBox cm_treasure;
        private System.Windows.Forms.CheckBox cm_stiltzkin;
        public System.Windows.Forms.TextBox textBox_seed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox Serial_But;
        private System.Windows.Forms.CheckBox checkBoxRecommended;
        private System.Windows.Forms.CheckBox checkBoxChaos;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.CheckBox checkBox6;
        private System.Windows.Forms.Label label3;
    }
}

