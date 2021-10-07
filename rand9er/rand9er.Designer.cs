
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
            this.c_medicshops = new System.Windows.Forms.CheckBox();
            this.c_medicitems = new System.Windows.Forms.CheckBox();
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
            this.c_safe = new System.Windows.Forms.CheckBox();
            this.c_random = new System.Windows.Forms.CheckBox();
            this.c_max = new System.Windows.Forms.CheckBox();
            this.c_default = new System.Windows.Forms.CheckBox();
            this.c_basestats = new System.Windows.Forms.CheckBox();
            this.c_abilitygems = new System.Windows.Forms.CheckBox();
            this.c_random_e = new System.Windows.Forms.CheckBox();
            this.c_main_e = new System.Windows.Forms.CheckBox();
            this.c_all_e = new System.Windows.Forms.CheckBox();
            this.button_rand = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.cm_entrances = new System.Windows.Forms.CheckBox();
            this.c_nologic = new System.Windows.Forms.CheckBox();
            this.c_allpaths = new System.Windows.Forms.CheckBox();
            this.cm_enemies = new System.Windows.Forms.CheckBox();
            this.c_esteals = new System.Windows.Forms.CheckBox();
            this.c_edrops = new System.Windows.Forms.CheckBox();
            this.c_es1 = new System.Windows.Forms.CheckBox();
            this.c_es2 = new System.Windows.Forms.CheckBox();
            this.c_es3 = new System.Windows.Forms.CheckBox();
            this.c_es4 = new System.Windows.Forms.CheckBox();
            this.c_ed4 = new System.Windows.Forms.CheckBox();
            this.c_ed3 = new System.Windows.Forms.CheckBox();
            this.c_ed2 = new System.Windows.Forms.CheckBox();
            this.c_ed1 = new System.Windows.Forms.CheckBox();
            this.l_shopm = new System.Windows.Forms.Label();
            this.c_mogdetect = new System.Windows.Forms.CheckBox();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox_output
            // 
            this.richTextBox_output.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox_output.ForeColor = System.Drawing.SystemColors.InfoText;
            this.richTextBox_output.Location = new System.Drawing.Point(206, 348);
            this.richTextBox_output.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox_output.Name = "richTextBox_output";
            this.richTextBox_output.ReadOnly = true;
            this.richTextBox_output.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox_output.Size = new System.Drawing.Size(182, 91);
            this.richTextBox_output.TabIndex = 1;
            this.richTextBox_output.Text = "int getRandomNumber() {\n     return 4; // chosen by fair dice roll.\n             " +
    "       // guaranteed to be random.\n}";
            this.richTextBox_output.WordWrap = false;
            this.richTextBox_output.TextChanged += new System.EventHandler(this.richTextBox_output_TextChanged);
            // 
            // label_seed
            // 
            this.label_seed.AutoSize = true;
            this.label_seed.BackColor = System.Drawing.SystemColors.ControlText;
            this.label_seed.Font = new System.Drawing.Font("Lucida Sans", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_seed.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label_seed.Location = new System.Drawing.Point(10, 14);
            this.label_seed.Name = "label_seed";
            this.label_seed.Size = new System.Drawing.Size(41, 15);
            this.label_seed.TabIndex = 12;
            this.label_seed.Text = "SEED";
            // 
            // textBox_seed
            // 
            this.textBox_seed.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_seed.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_seed.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_seed.Location = new System.Drawing.Point(57, 11);
            this.textBox_seed.Name = "textBox_seed";
            this.textBox_seed.Size = new System.Drawing.Size(404, 20);
            this.textBox_seed.TabIndex = 13;
            // 
            // richTextBox_debug
            // 
            this.richTextBox_debug.Location = new System.Drawing.Point(393, 216);
            this.richTextBox_debug.Name = "richTextBox_debug";
            this.richTextBox_debug.ReadOnly = true;
            this.richTextBox_debug.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox_debug.Size = new System.Drawing.Size(174, 58);
            this.richTextBox_debug.TabIndex = 14;
            this.richTextBox_debug.Text = "no bugs yet";
            this.richTextBox_debug.TextChanged += new System.EventHandler(this.richTextBox_debug_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.label1.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.label1.Image = global::rand9er.Properties.Resources.bgc;
            this.label1.Location = new System.Drawing.Point(523, 318);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 12);
            this.label1.TabIndex = 15;
            this.label1.Text = "Debug";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // c_medicshops
            // 
            this.c_medicshops.AutoSize = true;
            this.c_medicshops.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_medicshops.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_medicshops.Enabled = false;
            this.c_medicshops.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_medicshops.ForeColor = System.Drawing.Color.White;
            this.c_medicshops.Location = new System.Drawing.Point(26, 223);
            this.c_medicshops.Margin = new System.Windows.Forms.Padding(2);
            this.c_medicshops.Name = "c_medicshops";
            this.c_medicshops.Size = new System.Drawing.Size(154, 16);
            this.c_medicshops.TabIndex = 24;
            this.c_medicshops.Text = "override medic shops\r\n";
            this.c_medicshops.UseVisualStyleBackColor = false;
            this.c_medicshops.CheckedChanged += new System.EventHandler(this.c_medicshops_CheckedChanged);
            // 
            // c_medicitems
            // 
            this.c_medicitems.AutoSize = true;
            this.c_medicitems.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_medicitems.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_medicitems.Enabled = false;
            this.c_medicitems.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_medicitems.ForeColor = System.Drawing.Color.White;
            this.c_medicitems.Location = new System.Drawing.Point(26, 191);
            this.c_medicitems.Margin = new System.Windows.Forms.Padding(2);
            this.c_medicitems.Name = "c_medicitems";
            this.c_medicitems.Size = new System.Drawing.Size(145, 28);
            this.c_medicitems.TabIndex = 22;
            this.c_medicitems.Text = "include medic items\r\n(5.9% chance)";
            this.c_medicitems.UseVisualStyleBackColor = false;
            this.c_medicitems.CheckedChanged += new System.EventHandler(this.c_medicitems_CheckedChanged);
            // 
            // c_prices
            // 
            this.c_prices.AutoSize = true;
            this.c_prices.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_prices.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_prices.Enabled = false;
            this.c_prices.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_prices.ForeColor = System.Drawing.Color.White;
            this.c_prices.Location = new System.Drawing.Point(192, 145);
            this.c_prices.Name = "c_prices";
            this.c_prices.Size = new System.Drawing.Size(59, 16);
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
            this.c_result.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_result.ForeColor = System.Drawing.Color.White;
            this.c_result.Location = new System.Drawing.Point(192, 122);
            this.c_result.Margin = new System.Windows.Forms.Padding(2);
            this.c_result.Name = "c_result";
            this.c_result.Size = new System.Drawing.Size(62, 16);
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
            this.c_require.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_require.ForeColor = System.Drawing.Color.White;
            this.c_require.Location = new System.Drawing.Point(192, 99);
            this.c_require.Margin = new System.Windows.Forms.Padding(2);
            this.c_require.Name = "c_require";
            this.c_require.Size = new System.Drawing.Size(106, 16);
            this.c_require.TabIndex = 26;
            this.c_require.Text = "Requirements";
            this.c_require.UseVisualStyleBackColor = false;
            this.c_require.CheckedChanged += new System.EventHandler(this.c_require_CheckedChanged);
            // 
            // pbar_tree
            // 
            this.pbar_tree.Location = new System.Drawing.Point(392, 429);
            this.pbar_tree.Margin = new System.Windows.Forms.Padding(2);
            this.pbar_tree.Name = "pbar_tree";
            this.pbar_tree.Size = new System.Drawing.Size(175, 10);
            this.pbar_tree.TabIndex = 22;
            this.pbar_tree.Click += new System.EventHandler(this.pbar_tree_Click);
            // 
            // b_rseed
            // 
            this.b_rseed.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.b_rseed.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("b_rseed.BackgroundImage")));
            this.b_rseed.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_rseed.FlatAppearance.BorderSize = 3;
            this.b_rseed.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_rseed.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_rseed.ForeColor = System.Drawing.Color.White;
            this.b_rseed.Location = new System.Drawing.Point(466, 10);
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
            this.b_open.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.b_open.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("b_open.BackgroundImage")));
            this.b_open.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_open.FlatAppearance.BorderSize = 3;
            this.b_open.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_open.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_open.ForeColor = System.Drawing.Color.White;
            this.b_open.Location = new System.Drawing.Point(11, 36);
            this.b_open.Margin = new System.Windows.Forms.Padding(2);
            this.b_open.Name = "b_open";
            this.b_open.Size = new System.Drawing.Size(131, 20);
            this.b_open.TabIndex = 24;
            this.b_open.Text = "Set FFIX location";
            this.b_open.UseVisualStyleBackColor = false;
            this.b_open.Click += new System.EventHandler(this.b_open_Click);
            // 
            // tb_fl
            // 
            this.tb_fl.BackColor = System.Drawing.SystemColors.ControlLight;
            this.tb_fl.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tb_fl.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tb_fl.Location = new System.Drawing.Point(146, 36);
            this.tb_fl.Margin = new System.Windows.Forms.Padding(2);
            this.tb_fl.Name = "tb_fl";
            this.tb_fl.Size = new System.Drawing.Size(315, 20);
            this.tb_fl.TabIndex = 25;
            this.tb_fl.TextChanged += new System.EventHandler(this.tb_fl_TextChanged);
            // 
            // b_restore
            // 
            this.b_restore.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.b_restore.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.b_restore.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_restore.FlatAppearance.BorderSize = 3;
            this.b_restore.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_restore.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_restore.ForeColor = System.Drawing.Color.White;
            this.b_restore.Location = new System.Drawing.Point(393, 335);
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
            this.b_search.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.b_search.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("b_search.BackgroundImage")));
            this.b_search.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_search.FlatAppearance.BorderSize = 3;
            this.b_search.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_search.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_search.ForeColor = System.Drawing.Color.White;
            this.b_search.Location = new System.Drawing.Point(466, 33);
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
            this.cm_synth.Font = new System.Drawing.Font("Lucida Sans", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_synth.ForeColor = System.Drawing.Color.White;
            this.cm_synth.Location = new System.Drawing.Point(182, 72);
            this.cm_synth.Name = "cm_synth";
            this.cm_synth.Size = new System.Drawing.Size(99, 21);
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
            this.cm_itemshop.Font = new System.Drawing.Font("Lucida Sans", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_itemshop.ForeColor = System.Drawing.Color.White;
            this.cm_itemshop.Location = new System.Drawing.Point(13, 72);
            this.cm_itemshop.Name = "cm_itemshop";
            this.cm_itemshop.Size = new System.Drawing.Size(112, 21);
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
            this.cm_char.Font = new System.Drawing.Font("Lucida Sans", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_char.ForeColor = System.Drawing.Color.White;
            this.cm_char.Location = new System.Drawing.Point(13, 270);
            this.cm_char.Name = "cm_char";
            this.cm_char.Size = new System.Drawing.Size(112, 21);
            this.cm_char.TabIndex = 42;
            this.cm_char.Text = "Characters";
            this.cm_char.UseVisualStyleBackColor = false;
            this.cm_char.CheckedChanged += new System.EventHandler(this.cm_char_CheckedChanged);
            // 
            // c_safe
            // 
            this.c_safe.AutoSize = true;
            this.c_safe.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_safe.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_safe.Enabled = false;
            this.c_safe.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_safe.ForeColor = System.Drawing.Color.White;
            this.c_safe.Location = new System.Drawing.Point(24, 99);
            this.c_safe.Name = "c_safe";
            this.c_safe.Size = new System.Drawing.Size(50, 16);
            this.c_safe.TabIndex = 43;
            this.c_safe.Text = "Safe";
            this.c_safe.UseVisualStyleBackColor = false;
            this.c_safe.CheckedChanged += new System.EventHandler(this.c_safe_CheckedChanged);
            // 
            // c_random
            // 
            this.c_random.AutoSize = true;
            this.c_random.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_random.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_random.Enabled = false;
            this.c_random.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_random.ForeColor = System.Drawing.Color.White;
            this.c_random.Location = new System.Drawing.Point(24, 122);
            this.c_random.Name = "c_random";
            this.c_random.Size = new System.Drawing.Size(72, 16);
            this.c_random.TabIndex = 44;
            this.c_random.Text = "Random";
            this.c_random.UseVisualStyleBackColor = false;
            this.c_random.CheckedChanged += new System.EventHandler(this.c_random_CheckedChanged);
            this.c_random.EnabledChanged += new System.EventHandler(this.c_random_EnabledChanged);
            // 
            // c_max
            // 
            this.c_max.AutoSize = true;
            this.c_max.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_max.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_max.Enabled = false;
            this.c_max.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_max.ForeColor = System.Drawing.Color.White;
            this.c_max.Location = new System.Drawing.Point(24, 145);
            this.c_max.Name = "c_max";
            this.c_max.Size = new System.Drawing.Size(49, 16);
            this.c_max.TabIndex = 45;
            this.c_max.Text = "Max";
            this.c_max.UseVisualStyleBackColor = false;
            this.c_max.CheckedChanged += new System.EventHandler(this.c_max_CheckedChanged);
            this.c_max.EnabledChanged += new System.EventHandler(this.c_max_EnabledChanged);
            // 
            // c_default
            // 
            this.c_default.AutoSize = true;
            this.c_default.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_default.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_default.Enabled = false;
            this.c_default.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_default.ForeColor = System.Drawing.Color.White;
            this.c_default.Location = new System.Drawing.Point(26, 345);
            this.c_default.Name = "c_default";
            this.c_default.Size = new System.Drawing.Size(142, 16);
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
            this.c_basestats.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_basestats.ForeColor = System.Drawing.Color.White;
            this.c_basestats.Location = new System.Drawing.Point(26, 322);
            this.c_basestats.Name = "c_basestats";
            this.c_basestats.Size = new System.Drawing.Size(87, 16);
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
            this.c_abilitygems.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_abilitygems.ForeColor = System.Drawing.Color.White;
            this.c_abilitygems.Location = new System.Drawing.Point(26, 299);
            this.c_abilitygems.Name = "c_abilitygems";
            this.c_abilitygems.Size = new System.Drawing.Size(108, 16);
            this.c_abilitygems.TabIndex = 48;
            this.c_abilitygems.Text = "Abilitys\' Gems";
            this.c_abilitygems.UseVisualStyleBackColor = false;
            this.c_abilitygems.CheckedChanged += new System.EventHandler(this.c_abilitygems_CheckedChanged);
            // 
            // c_random_e
            // 
            this.c_random_e.AutoSize = true;
            this.c_random_e.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_random_e.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_random_e.Enabled = false;
            this.c_random_e.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_random_e.ForeColor = System.Drawing.Color.White;
            this.c_random_e.Location = new System.Drawing.Point(37, 391);
            this.c_random_e.Name = "c_random_e";
            this.c_random_e.Size = new System.Drawing.Size(126, 16);
            this.c_random_e.TabIndex = 50;
            this.c_random_e.Text = "random no types";
            this.c_random_e.UseVisualStyleBackColor = false;
            this.c_random_e.CheckedChanged += new System.EventHandler(this.c_random_e_CheckedChanged);
            // 
            // c_main_e
            // 
            this.c_main_e.AutoSize = true;
            this.c_main_e.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_main_e.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_main_e.Enabled = false;
            this.c_main_e.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_main_e.ForeColor = System.Drawing.Color.White;
            this.c_main_e.Location = new System.Drawing.Point(37, 368);
            this.c_main_e.Name = "c_main_e";
            this.c_main_e.Size = new System.Drawing.Size(114, 16);
            this.c_main_e.TabIndex = 51;
            this.c_main_e.Text = "maintain types";
            this.c_main_e.UseVisualStyleBackColor = false;
            this.c_main_e.CheckedChanged += new System.EventHandler(this.c_main_e_CheckedChanged);
            // 
            // c_all_e
            // 
            this.c_all_e.AutoSize = true;
            this.c_all_e.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_all_e.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_all_e.Enabled = false;
            this.c_all_e.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_all_e.ForeColor = System.Drawing.Color.White;
            this.c_all_e.Location = new System.Drawing.Point(37, 415);
            this.c_all_e.Name = "c_all_e";
            this.c_all_e.Size = new System.Drawing.Size(113, 16);
            this.c_all_e.TabIndex = 53;
            this.c_all_e.Text = "share all types";
            this.c_all_e.UseVisualStyleBackColor = false;
            this.c_all_e.CheckedChanged += new System.EventHandler(this.c_all_e_CheckedChanged);
            // 
            // button_rand
            // 
            this.button_rand.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.button_rand.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.button_rand.FlatAppearance.BorderSize = 3;
            this.button_rand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button_rand.Font = new System.Drawing.Font("Lucida Sans", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_rand.ForeColor = System.Drawing.Color.White;
            this.button_rand.Location = new System.Drawing.Point(392, 375);
            this.button_rand.Margin = new System.Windows.Forms.Padding(2);
            this.button_rand.Name = "button_rand";
            this.button_rand.Size = new System.Drawing.Size(176, 50);
            this.button_rand.TabIndex = 2;
            this.button_rand.Text = "Buy for 333 Gil";
            this.button_rand.UseVisualStyleBackColor = false;
            this.button_rand.Click += new System.EventHandler(this.button_rand_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = global::rand9er.Properties.Resources.bg;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Location = new System.Drawing.Point(-8, -31);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(596, 484);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 54;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkBox1.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.checkBox1.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1.ForeColor = System.Drawing.Color.White;
            this.checkBox1.Location = new System.Drawing.Point(393, 280);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(168, 28);
            this.checkBox1.TabIndex = 55;
            this.checkBox1.Text = "Use Data files from the \r\nFFIX location as source";
            this.checkBox1.UseVisualStyleBackColor = false;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // cm_entrances
            // 
            this.cm_entrances.AutoSize = true;
            this.cm_entrances.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_entrances.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_entrances.Font = new System.Drawing.Font("Lucida Sans", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_entrances.ForeColor = System.Drawing.Color.White;
            this.cm_entrances.Location = new System.Drawing.Point(204, 270);
            this.cm_entrances.Name = "cm_entrances";
            this.cm_entrances.Size = new System.Drawing.Size(143, 21);
            this.cm_entrances.TabIndex = 56;
            this.cm_entrances.Text = "Field Entrances";
            this.cm_entrances.UseVisualStyleBackColor = false;
            this.cm_entrances.CheckedChanged += new System.EventHandler(this.cm_entrances_CheckedChanged);
            // 
            // c_nologic
            // 
            this.c_nologic.AutoSize = true;
            this.c_nologic.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_nologic.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_nologic.Enabled = false;
            this.c_nologic.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_nologic.ForeColor = System.Drawing.Color.White;
            this.c_nologic.Location = new System.Drawing.Point(214, 322);
            this.c_nologic.Name = "c_nologic";
            this.c_nologic.Size = new System.Drawing.Size(77, 16);
            this.c_nologic.TabIndex = 57;
            this.c_nologic.Text = "No Logic";
            this.c_nologic.UseVisualStyleBackColor = false;
            this.c_nologic.CheckedChanged += new System.EventHandler(this.c_nologic_CheckedChanged);
            // 
            // c_allpaths
            // 
            this.c_allpaths.AutoSize = true;
            this.c_allpaths.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_allpaths.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_allpaths.Enabled = false;
            this.c_allpaths.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_allpaths.ForeColor = System.Drawing.Color.White;
            this.c_allpaths.Location = new System.Drawing.Point(214, 299);
            this.c_allpaths.Name = "c_allpaths";
            this.c_allpaths.Size = new System.Drawing.Size(128, 16);
            this.c_allpaths.TabIndex = 58;
            this.c_allpaths.Text = "All Paths Possible";
            this.c_allpaths.UseVisualStyleBackColor = false;
            this.c_allpaths.CheckedChanged += new System.EventHandler(this.c_allpaths_CheckedChanged);
            // 
            // cm_enemies
            // 
            this.cm_enemies.AutoSize = true;
            this.cm_enemies.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_enemies.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_enemies.Font = new System.Drawing.Font("Lucida Sans", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_enemies.ForeColor = System.Drawing.Color.White;
            this.cm_enemies.Location = new System.Drawing.Point(392, 72);
            this.cm_enemies.Name = "cm_enemies";
            this.cm_enemies.Size = new System.Drawing.Size(89, 21);
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
            this.c_esteals.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_esteals.ForeColor = System.Drawing.Color.White;
            this.c_esteals.Location = new System.Drawing.Point(337, 99);
            this.c_esteals.Name = "c_esteals";
            this.c_esteals.Size = new System.Drawing.Size(92, 16);
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
            this.c_edrops.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_edrops.ForeColor = System.Drawing.Color.White;
            this.c_edrops.Location = new System.Drawing.Point(447, 99);
            this.c_edrops.Name = "c_edrops";
            this.c_edrops.Size = new System.Drawing.Size(91, 16);
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
            this.c_es1.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_es1.ForeColor = System.Drawing.Color.White;
            this.c_es1.Location = new System.Drawing.Point(345, 122);
            this.c_es1.Name = "c_es1";
            this.c_es1.Size = new System.Drawing.Size(101, 16);
            this.c_es1.TabIndex = 63;
            this.c_es1.Text = "Slot 1   100%";
            this.c_es1.UseVisualStyleBackColor = false;
            this.c_es1.CheckedChanged += new System.EventHandler(this.c_es1_CheckedChanged);
            // 
            // c_es2
            // 
            this.c_es2.AutoSize = true;
            this.c_es2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_es2.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_es2.Enabled = false;
            this.c_es2.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_es2.ForeColor = System.Drawing.Color.White;
            this.c_es2.Location = new System.Drawing.Point(345, 145);
            this.c_es2.Name = "c_es2";
            this.c_es2.Size = new System.Drawing.Size(94, 16);
            this.c_es2.TabIndex = 64;
            this.c_es2.Text = "Slot 2   25%";
            this.c_es2.UseVisualStyleBackColor = false;
            this.c_es2.CheckedChanged += new System.EventHandler(this.c_es2_CheckedChanged);
            // 
            // c_es3
            // 
            this.c_es3.AutoSize = true;
            this.c_es3.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_es3.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_es3.Enabled = false;
            this.c_es3.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_es3.ForeColor = System.Drawing.Color.White;
            this.c_es3.Location = new System.Drawing.Point(345, 168);
            this.c_es3.Name = "c_es3";
            this.c_es3.Size = new System.Drawing.Size(109, 16);
            this.c_es3.TabIndex = 65;
            this.c_es3.Text = "Slot 3   6.25% ";
            this.c_es3.UseVisualStyleBackColor = false;
            this.c_es3.CheckedChanged += new System.EventHandler(this.c_es3_CheckedChanged);
            // 
            // c_es4
            // 
            this.c_es4.AutoSize = true;
            this.c_es4.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_es4.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_es4.Enabled = false;
            this.c_es4.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_es4.ForeColor = System.Drawing.Color.White;
            this.c_es4.Location = new System.Drawing.Point(345, 191);
            this.c_es4.Name = "c_es4";
            this.c_es4.Size = new System.Drawing.Size(98, 16);
            this.c_es4.TabIndex = 66;
            this.c_es4.Text = "Slot 4   0.3%";
            this.c_es4.UseVisualStyleBackColor = false;
            this.c_es4.CheckedChanged += new System.EventHandler(this.c_es4_CheckedChanged);
            // 
            // c_ed4
            // 
            this.c_ed4.AutoSize = true;
            this.c_ed4.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_ed4.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_ed4.Enabled = false;
            this.c_ed4.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_ed4.ForeColor = System.Drawing.Color.White;
            this.c_ed4.Location = new System.Drawing.Point(459, 191);
            this.c_ed4.Name = "c_ed4";
            this.c_ed4.Size = new System.Drawing.Size(98, 16);
            this.c_ed4.TabIndex = 70;
            this.c_ed4.Text = "Slot 4   0.3%";
            this.c_ed4.UseVisualStyleBackColor = false;
            this.c_ed4.CheckedChanged += new System.EventHandler(this.c_ed4_CheckedChanged);
            // 
            // c_ed3
            // 
            this.c_ed3.AutoSize = true;
            this.c_ed3.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_ed3.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_ed3.Enabled = false;
            this.c_ed3.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_ed3.ForeColor = System.Drawing.Color.White;
            this.c_ed3.Location = new System.Drawing.Point(459, 168);
            this.c_ed3.Name = "c_ed3";
            this.c_ed3.Size = new System.Drawing.Size(109, 16);
            this.c_ed3.TabIndex = 69;
            this.c_ed3.Text = "Slot 3   6.25% ";
            this.c_ed3.UseVisualStyleBackColor = false;
            this.c_ed3.CheckedChanged += new System.EventHandler(this.c_ed3_CheckedChanged);
            // 
            // c_ed2
            // 
            this.c_ed2.AutoSize = true;
            this.c_ed2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_ed2.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_ed2.Enabled = false;
            this.c_ed2.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_ed2.ForeColor = System.Drawing.Color.White;
            this.c_ed2.Location = new System.Drawing.Point(459, 145);
            this.c_ed2.Name = "c_ed2";
            this.c_ed2.Size = new System.Drawing.Size(94, 16);
            this.c_ed2.TabIndex = 68;
            this.c_ed2.Text = "Slot 2   25%";
            this.c_ed2.UseVisualStyleBackColor = false;
            this.c_ed2.CheckedChanged += new System.EventHandler(this.c_ed2_CheckedChanged);
            // 
            // c_ed1
            // 
            this.c_ed1.AutoSize = true;
            this.c_ed1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.c_ed1.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_ed1.Enabled = false;
            this.c_ed1.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_ed1.ForeColor = System.Drawing.Color.White;
            this.c_ed1.Location = new System.Drawing.Point(459, 122);
            this.c_ed1.Name = "c_ed1";
            this.c_ed1.Size = new System.Drawing.Size(101, 16);
            this.c_ed1.TabIndex = 67;
            this.c_ed1.Text = "Slot 1   100%";
            this.c_ed1.UseVisualStyleBackColor = false;
            this.c_ed1.CheckedChanged += new System.EventHandler(this.c_ed1_CheckedChanged);
            // 
            // l_shopm
            // 
            this.l_shopm.AutoSize = true;
            this.l_shopm.BackColor = System.Drawing.SystemColors.ControlDark;
            this.l_shopm.Font = new System.Drawing.Font("Lucida Sans", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.l_shopm.ForeColor = System.Drawing.Color.White;
            this.l_shopm.Image = global::rand9er.Properties.Resources.sbag3;
            this.l_shopm.Location = new System.Drawing.Point(12, 168);
            this.l_shopm.Name = "l_shopm";
            this.l_shopm.Size = new System.Drawing.Size(109, 15);
            this.l_shopm.TabIndex = 71;
            this.l_shopm.Text = "Shop Modifiers";
            this.l_shopm.Click += new System.EventHandler(this.label2_Click);
            // 
            // c_mogdetect
            // 
            this.c_mogdetect.AutoSize = true;
            this.c_mogdetect.BackColor = System.Drawing.SystemColors.Control;
            this.c_mogdetect.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.c_mogdetect.Enabled = false;
            this.c_mogdetect.Font = new System.Drawing.Font("Lucida Sans", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.c_mogdetect.ForeColor = System.Drawing.Color.White;
            this.c_mogdetect.Location = new System.Drawing.Point(392, 314);
            this.c_mogdetect.Name = "c_mogdetect";
            this.c_mogdetect.Size = new System.Drawing.Size(95, 16);
            this.c_mogdetect.TabIndex = 72;
            this.c_mogdetect.Text = "Stock Game";
            this.c_mogdetect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.c_mogdetect.UseVisualStyleBackColor = false;
            this.c_mogdetect.CheckedChanged += new System.EventHandler(this.c_mogdetect_CheckedChanged);
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(185, 167);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox1.Size = new System.Drawing.Size(154, 97);
            this.richTextBox1.TabIndex = 73;
            this.richTextBox1.Text = "";
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // rand9er
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 446);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.c_mogdetect);
            this.Controls.Add(this.l_shopm);
            this.Controls.Add(this.c_ed4);
            this.Controls.Add(this.c_ed3);
            this.Controls.Add(this.c_ed2);
            this.Controls.Add(this.c_ed1);
            this.Controls.Add(this.c_es4);
            this.Controls.Add(this.c_es3);
            this.Controls.Add(this.c_es2);
            this.Controls.Add(this.c_es1);
            this.Controls.Add(this.c_edrops);
            this.Controls.Add(this.c_esteals);
            this.Controls.Add(this.cm_enemies);
            this.Controls.Add(this.c_allpaths);
            this.Controls.Add(this.c_nologic);
            this.Controls.Add(this.cm_entrances);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.c_all_e);
            this.Controls.Add(this.c_main_e);
            this.Controls.Add(this.c_random_e);
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
            this.Controls.Add(this.pictureBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "rand9er";
            this.Text = "Stiltzkin\'s bag 1.19    a Final Fantasy IX randomizer tool";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox_output;
        private System.Windows.Forms.Button button_rand;
        private System.Windows.Forms.Label label_seed;
        private System.Windows.Forms.TextBox textBox_seed;
        private System.Windows.Forms.RichTextBox richTextBox_debug;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox c_medicitems;
        private System.Windows.Forms.CheckBox c_medicshops;
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
        private System.Windows.Forms.CheckBox c_safe;
        private System.Windows.Forms.CheckBox c_random;
        private System.Windows.Forms.CheckBox c_max;
        private System.Windows.Forms.CheckBox c_default;
        private System.Windows.Forms.CheckBox c_basestats;
        private System.Windows.Forms.CheckBox c_abilitygems;
        private System.Windows.Forms.CheckBox c_random_e;
        private System.Windows.Forms.CheckBox c_main_e;
        private System.Windows.Forms.CheckBox c_all_e;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox cm_entrances;
        private System.Windows.Forms.CheckBox c_nologic;
        private System.Windows.Forms.CheckBox c_allpaths;
        private System.Windows.Forms.CheckBox cm_enemies;
        private System.Windows.Forms.CheckBox c_esteals;
        private System.Windows.Forms.CheckBox c_edrops;
        private System.Windows.Forms.CheckBox c_es1;
        private System.Windows.Forms.CheckBox c_es2;
        private System.Windows.Forms.CheckBox c_es3;
        private System.Windows.Forms.CheckBox c_es4;
        private System.Windows.Forms.CheckBox c_ed4;
        private System.Windows.Forms.CheckBox c_ed3;
        private System.Windows.Forms.CheckBox c_ed2;
        private System.Windows.Forms.CheckBox c_ed1;
        private System.Windows.Forms.Label l_shopm;
        private System.Windows.Forms.CheckBox c_mogdetect;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

