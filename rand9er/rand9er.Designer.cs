
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
            this.label_seed = new System.Windows.Forms.Label();
            this.textBox_seed = new System.Windows.Forms.TextBox();
            this.richTextBox_debug = new System.Windows.Forms.RichTextBox();
            this.b_rseed = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.b_open = new System.Windows.Forms.Button();
            this.tb_fl = new System.Windows.Forms.TextBox();
            this.b_remove = new System.Windows.Forms.Button();
            this.b_search = new System.Windows.Forms.Button();
            this.button_rand = new System.Windows.Forms.Button();
            this.cm_entrances = new System.Windows.Forms.CheckBox();
            this.cm_stiltzkin = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxRecommended = new System.Windows.Forms.CheckBox();
            this.checkBoxChaos = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.debugButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.enemyItemDrops = new System.Windows.Forms.Label();
            this.enemyItemSteals = new System.Windows.Forms.Label();
            this.enemyCardDrop = new System.Windows.Forms.Label();
            this.enemyBlueMagic = new System.Windows.Forms.Label();
            this.tmCardStats = new System.Windows.Forms.Label();
            this.tmCardOrder = new System.Windows.Forms.Label();
            this.tmDecks = new System.Windows.Forms.Label();
            this.charBaseStats = new System.Windows.Forms.Label();
            this.charSpeciality = new System.Windows.Forms.Label();
            this.charAbilities = new System.Windows.Forms.Label();
            this.charEquipment = new System.Windows.Forms.Label();
            this.itemsTreasure = new System.Windows.Forms.Label();
            this.itemsShops = new System.Windows.Forms.Label();
            this.itemsSynth = new System.Windows.Forms.Label();
            this.character = new System.Windows.Forms.CheckBox();
            this.item = new System.Windows.Forms.CheckBox();
            this.enemies = new System.Windows.Forms.CheckBox();
            this.tm = new System.Windows.Forms.CheckBox();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.itemsBonusSets = new System.Windows.Forms.Label();
            this.SuspendLayout();
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
            this.textBox_seed.Size = new System.Drawing.Size(481, 20);
            this.textBox_seed.TabIndex = 13;
            this.textBox_seed.TextChanged += new System.EventHandler(this.textBox_seed_TextChanged);
            // 
            // richTextBox_debug
            // 
            this.richTextBox_debug.Location = new System.Drawing.Point(583, 315);
            this.richTextBox_debug.Name = "richTextBox_debug";
            this.richTextBox_debug.ReadOnly = true;
            this.richTextBox_debug.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.richTextBox_debug.Size = new System.Drawing.Size(79, 37);
            this.richTextBox_debug.TabIndex = 14;
            this.richTextBox_debug.Text = "no bugs yet";
            this.richTextBox_debug.TextChanged += new System.EventHandler(this.richTextBox_debug_TextChanged);
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
            this.b_rseed.Location = new System.Drawing.Point(561, 11);
            this.b_rseed.Margin = new System.Windows.Forms.Padding(2);
            this.b_rseed.Name = "b_rseed";
            this.b_rseed.Size = new System.Drawing.Size(101, 22);
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
            this.b_open.Location = new System.Drawing.Point(504, 40);
            this.b_open.Margin = new System.Windows.Forms.Padding(2);
            this.b_open.Name = "b_open";
            this.b_open.Size = new System.Drawing.Size(158, 22);
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
            this.tb_fl.Size = new System.Drawing.Size(318, 20);
            this.tb_fl.TabIndex = 25;
            this.tb_fl.TextChanged += new System.EventHandler(this.tb_fl_TextChanged);
            // 
            // b_remove
            // 
            this.b_remove.AutoEllipsis = true;
            this.b_remove.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.b_remove.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.b_remove.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.b_remove.FlatAppearance.BorderSize = 3;
            this.b_remove.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.b_remove.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_remove.ForeColor = System.Drawing.Color.White;
            this.b_remove.Location = new System.Drawing.Point(201, 397);
            this.b_remove.Margin = new System.Windows.Forms.Padding(2);
            this.b_remove.Name = "b_remove";
            this.b_remove.Size = new System.Drawing.Size(270, 28);
            this.b_remove.TabIndex = 26;
            this.b_remove.Text = "Remove Stiltzkin\'s Bag from Mod Load Order";
            this.b_remove.UseVisualStyleBackColor = false;
            this.b_remove.Click += new System.EventHandler(this.b_restore_Click);
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
            this.b_search.Location = new System.Drawing.Point(398, 40);
            this.b_search.Name = "b_search";
            this.b_search.Size = new System.Drawing.Size(101, 22);
            this.b_search.TabIndex = 27;
            this.b_search.Text = "Auto Locate";
            this.b_search.UseVisualStyleBackColor = false;
            this.b_search.Click += new System.EventHandler(this.b_search_Click);
            // 
            // button_rand
            // 
            this.button_rand.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button_rand.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.button_rand.FlatAppearance.BorderSize = 3;
            this.button_rand.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button_rand.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_rand.ForeColor = System.Drawing.Color.White;
            this.button_rand.Location = new System.Drawing.Point(485, 375);
            this.button_rand.Margin = new System.Windows.Forms.Padding(2);
            this.button_rand.Name = "button_rand";
            this.button_rand.Size = new System.Drawing.Size(176, 50);
            this.button_rand.TabIndex = 2;
            this.button_rand.Text = "Stitzkin\'s Bag";
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
            this.cm_entrances.Location = new System.Drawing.Point(507, 221);
            this.cm_entrances.Name = "cm_entrances";
            this.cm_entrances.Size = new System.Drawing.Size(144, 22);
            this.cm_entrances.TabIndex = 56;
            this.cm_entrances.Text = "Field Entrances";
            this.cm_entrances.UseVisualStyleBackColor = false;
            this.cm_entrances.CheckedChanged += new System.EventHandler(this.cm_entrances_CheckedChanged);
            // 
            // cm_stiltzkin
            // 
            this.cm_stiltzkin.AutoSize = true;
            this.cm_stiltzkin.BackColor = System.Drawing.SystemColors.ControlDark;
            this.cm_stiltzkin.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.cm_stiltzkin.Enabled = false;
            this.cm_stiltzkin.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cm_stiltzkin.ForeColor = System.Drawing.Color.White;
            this.cm_stiltzkin.Location = new System.Drawing.Point(507, 249);
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
            this.label2.Location = new System.Drawing.Point(482, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(142, 16);
            this.label2.TabIndex = 81;
            this.label2.Text = "Randomize Version";
            // 
            // checkBoxRecommended
            // 
            this.checkBoxRecommended.AutoSize = true;
            this.checkBoxRecommended.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkBoxRecommended.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.checkBoxRecommended.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxRecommended.ForeColor = System.Drawing.Color.White;
            this.checkBoxRecommended.Location = new System.Drawing.Point(508, 116);
            this.checkBoxRecommended.Name = "checkBoxRecommended";
            this.checkBoxRecommended.Size = new System.Drawing.Size(109, 17);
            this.checkBoxRecommended.TabIndex = 83;
            this.checkBoxRecommended.Text = "Recommended";
            this.checkBoxRecommended.UseVisualStyleBackColor = false;
            this.checkBoxRecommended.CheckedChanged += new System.EventHandler(this.checkBoxRecommended_CheckedChanged);
            // 
            // checkBoxChaos
            // 
            this.checkBoxChaos.AutoSize = true;
            this.checkBoxChaos.BackColor = System.Drawing.SystemColors.ControlDark;
            this.checkBoxChaos.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.checkBoxChaos.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBoxChaos.ForeColor = System.Drawing.Color.White;
            this.checkBoxChaos.Location = new System.Drawing.Point(508, 137);
            this.checkBoxChaos.Name = "checkBoxChaos";
            this.checkBoxChaos.Size = new System.Drawing.Size(116, 17);
            this.checkBoxChaos.TabIndex = 84;
            this.checkBoxChaos.Text = "Maximum Chaos";
            this.checkBoxChaos.UseVisualStyleBackColor = false;
            this.checkBoxChaos.CheckedChanged += new System.EventHandler(this.checkBoxChaos_CheckedChanged);
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
            // debugButton
            // 
            this.debugButton.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.debugButton.BackgroundImage = global::rand9er.Properties.Resources.bgc2;
            this.debugButton.Enabled = false;
            this.debugButton.FlatAppearance.BorderSize = 3;
            this.debugButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.debugButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.debugButton.ForeColor = System.Drawing.Color.DarkSeaGreen;
            this.debugButton.Location = new System.Drawing.Point(496, 315);
            this.debugButton.Margin = new System.Windows.Forms.Padding(2);
            this.debugButton.Name = "debugButton";
            this.debugButton.Size = new System.Drawing.Size(82, 37);
            this.debugButton.TabIndex = 89;
            this.debugButton.Text = "Debug Funcs";
            this.debugButton.UseVisualStyleBackColor = false;
            this.debugButton.Click += new System.EventHandler(this.debugButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label1.Location = new System.Drawing.Point(482, 202);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 16);
            this.label1.TabIndex = 90;
            this.label1.Text = "RoadMap";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label4.Location = new System.Drawing.Point(36, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 16);
            this.label4.TabIndex = 91;
            this.label4.Text = "Features";
            // 
            // enemyItemDrops
            // 
            this.enemyItemDrops.AutoSize = true;
            this.enemyItemDrops.BackColor = System.Drawing.Color.Transparent;
            this.enemyItemDrops.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.enemyItemDrops.ForeColor = System.Drawing.Color.White;
            this.enemyItemDrops.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.enemyItemDrops.Location = new System.Drawing.Point(86, 290);
            this.enemyItemDrops.Name = "enemyItemDrops";
            this.enemyItemDrops.Size = new System.Drawing.Size(68, 13);
            this.enemyItemDrops.TabIndex = 107;
            this.enemyItemDrops.Text = "Item Drops";
            // 
            // enemyItemSteals
            // 
            this.enemyItemSteals.AutoSize = true;
            this.enemyItemSteals.BackColor = System.Drawing.Color.Transparent;
            this.enemyItemSteals.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.enemyItemSteals.ForeColor = System.Drawing.Color.White;
            this.enemyItemSteals.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.enemyItemSteals.Location = new System.Drawing.Point(86, 313);
            this.enemyItemSteals.Name = "enemyItemSteals";
            this.enemyItemSteals.Size = new System.Drawing.Size(70, 13);
            this.enemyItemSteals.TabIndex = 108;
            this.enemyItemSteals.Text = "Item Steals";
            // 
            // enemyCardDrop
            // 
            this.enemyCardDrop.AutoSize = true;
            this.enemyCardDrop.BackColor = System.Drawing.Color.Transparent;
            this.enemyCardDrop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.enemyCardDrop.ForeColor = System.Drawing.Color.White;
            this.enemyCardDrop.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.enemyCardDrop.Location = new System.Drawing.Point(86, 336);
            this.enemyCardDrop.Name = "enemyCardDrop";
            this.enemyCardDrop.Size = new System.Drawing.Size(64, 13);
            this.enemyCardDrop.TabIndex = 109;
            this.enemyCardDrop.Text = "Card Drop";
            // 
            // enemyBlueMagic
            // 
            this.enemyBlueMagic.AutoSize = true;
            this.enemyBlueMagic.BackColor = System.Drawing.Color.Transparent;
            this.enemyBlueMagic.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.enemyBlueMagic.ForeColor = System.Drawing.Color.White;
            this.enemyBlueMagic.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.enemyBlueMagic.Location = new System.Drawing.Point(84, 360);
            this.enemyBlueMagic.Name = "enemyBlueMagic";
            this.enemyBlueMagic.Size = new System.Drawing.Size(70, 13);
            this.enemyBlueMagic.TabIndex = 110;
            this.enemyBlueMagic.Text = "Blue Magic";
            // 
            // tmCardStats
            // 
            this.tmCardStats.AutoSize = true;
            this.tmCardStats.BackColor = System.Drawing.Color.Transparent;
            this.tmCardStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.tmCardStats.ForeColor = System.Drawing.Color.White;
            this.tmCardStats.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tmCardStats.Location = new System.Drawing.Point(289, 290);
            this.tmCardStats.Name = "tmCardStats";
            this.tmCardStats.Size = new System.Drawing.Size(66, 13);
            this.tmCardStats.TabIndex = 111;
            this.tmCardStats.Text = "Card Stats";
            // 
            // tmCardOrder
            // 
            this.tmCardOrder.AutoSize = true;
            this.tmCardOrder.BackColor = System.Drawing.Color.Transparent;
            this.tmCardOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.tmCardOrder.ForeColor = System.Drawing.Color.White;
            this.tmCardOrder.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tmCardOrder.Location = new System.Drawing.Point(289, 313);
            this.tmCardOrder.Name = "tmCardOrder";
            this.tmCardOrder.Size = new System.Drawing.Size(68, 13);
            this.tmCardOrder.TabIndex = 112;
            this.tmCardOrder.Text = "Card Order";
            // 
            // tmDecks
            // 
            this.tmDecks.AutoSize = true;
            this.tmDecks.BackColor = System.Drawing.Color.Transparent;
            this.tmDecks.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.tmDecks.ForeColor = System.Drawing.Color.White;
            this.tmDecks.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.tmDecks.Location = new System.Drawing.Point(289, 339);
            this.tmDecks.Name = "tmDecks";
            this.tmDecks.Size = new System.Drawing.Size(43, 13);
            this.tmDecks.TabIndex = 113;
            this.tmDecks.Text = "Decks";
            // 
            // charBaseStats
            // 
            this.charBaseStats.AutoSize = true;
            this.charBaseStats.BackColor = System.Drawing.Color.Transparent;
            this.charBaseStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.charBaseStats.ForeColor = System.Drawing.Color.White;
            this.charBaseStats.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.charBaseStats.Location = new System.Drawing.Point(84, 144);
            this.charBaseStats.Name = "charBaseStats";
            this.charBaseStats.Size = new System.Drawing.Size(68, 13);
            this.charBaseStats.TabIndex = 114;
            this.charBaseStats.Text = "Base Stats";
            // 
            // charSpeciality
            // 
            this.charSpeciality.AutoSize = true;
            this.charSpeciality.BackColor = System.Drawing.Color.Transparent;
            this.charSpeciality.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.charSpeciality.ForeColor = System.Drawing.Color.White;
            this.charSpeciality.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.charSpeciality.Location = new System.Drawing.Point(86, 167);
            this.charSpeciality.Name = "charSpeciality";
            this.charSpeciality.Size = new System.Drawing.Size(62, 13);
            this.charSpeciality.TabIndex = 115;
            this.charSpeciality.Text = "Speciality";
            // 
            // charAbilities
            // 
            this.charAbilities.AutoSize = true;
            this.charAbilities.BackColor = System.Drawing.Color.Transparent;
            this.charAbilities.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.charAbilities.ForeColor = System.Drawing.Color.White;
            this.charAbilities.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.charAbilities.Location = new System.Drawing.Point(86, 190);
            this.charAbilities.Name = "charAbilities";
            this.charAbilities.Size = new System.Drawing.Size(51, 13);
            this.charAbilities.TabIndex = 116;
            this.charAbilities.Text = "Abilities";
            // 
            // charEquipment
            // 
            this.charEquipment.AutoSize = true;
            this.charEquipment.BackColor = System.Drawing.Color.Transparent;
            this.charEquipment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.charEquipment.ForeColor = System.Drawing.Color.White;
            this.charEquipment.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.charEquipment.Location = new System.Drawing.Point(86, 214);
            this.charEquipment.Name = "charEquipment";
            this.charEquipment.Size = new System.Drawing.Size(66, 13);
            this.charEquipment.TabIndex = 117;
            this.charEquipment.Text = "Equipment";
            // 
            // itemsTreasure
            // 
            this.itemsTreasure.AutoSize = true;
            this.itemsTreasure.BackColor = System.Drawing.Color.Transparent;
            this.itemsTreasure.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.itemsTreasure.ForeColor = System.Drawing.Color.White;
            this.itemsTreasure.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.itemsTreasure.Location = new System.Drawing.Point(291, 144);
            this.itemsTreasure.Name = "itemsTreasure";
            this.itemsTreasure.Size = new System.Drawing.Size(57, 13);
            this.itemsTreasure.TabIndex = 118;
            this.itemsTreasure.Text = "Treasure";
            // 
            // itemsShops
            // 
            this.itemsShops.AutoSize = true;
            this.itemsShops.BackColor = System.Drawing.Color.Transparent;
            this.itemsShops.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.itemsShops.ForeColor = System.Drawing.Color.White;
            this.itemsShops.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.itemsShops.Location = new System.Drawing.Point(291, 167);
            this.itemsShops.Name = "itemsShops";
            this.itemsShops.Size = new System.Drawing.Size(42, 13);
            this.itemsShops.TabIndex = 119;
            this.itemsShops.Text = "Shops";
            // 
            // itemsSynth
            // 
            this.itemsSynth.AutoSize = true;
            this.itemsSynth.BackColor = System.Drawing.Color.Transparent;
            this.itemsSynth.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.itemsSynth.ForeColor = System.Drawing.Color.White;
            this.itemsSynth.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.itemsSynth.Location = new System.Drawing.Point(291, 190);
            this.itemsSynth.Name = "itemsSynth";
            this.itemsSynth.Size = new System.Drawing.Size(61, 13);
            this.itemsSynth.TabIndex = 120;
            this.itemsSynth.Text = "Synthesis";
            // 
            // character
            // 
            this.character.AutoSize = true;
            this.character.BackColor = System.Drawing.SystemColors.ControlDark;
            this.character.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.character.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.character.ForeColor = System.Drawing.Color.White;
            this.character.Location = new System.Drawing.Point(62, 116);
            this.character.Name = "character";
            this.character.Size = new System.Drawing.Size(101, 22);
            this.character.TabIndex = 121;
            this.character.Text = "Character";
            this.character.UseVisualStyleBackColor = false;
            this.character.CheckedChanged += new System.EventHandler(this.character_CheckedChanged);
            // 
            // item
            // 
            this.item.AutoSize = true;
            this.item.BackColor = System.Drawing.SystemColors.ControlDark;
            this.item.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.item.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.item.ForeColor = System.Drawing.Color.White;
            this.item.Location = new System.Drawing.Point(272, 116);
            this.item.Name = "item";
            this.item.Size = new System.Drawing.Size(68, 22);
            this.item.TabIndex = 122;
            this.item.Text = "Items";
            this.item.UseVisualStyleBackColor = false;
            this.item.CheckedChanged += new System.EventHandler(this.item_CheckedChanged_1);
            // 
            // enemies
            // 
            this.enemies.AutoSize = true;
            this.enemies.BackColor = System.Drawing.SystemColors.ControlDark;
            this.enemies.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.enemies.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.enemies.ForeColor = System.Drawing.Color.White;
            this.enemies.Location = new System.Drawing.Point(62, 262);
            this.enemies.Name = "enemies";
            this.enemies.Size = new System.Drawing.Size(92, 22);
            this.enemies.TabIndex = 123;
            this.enemies.Text = "Enemies";
            this.enemies.UseVisualStyleBackColor = false;
            this.enemies.CheckedChanged += new System.EventHandler(this.enemies_CheckedChanged);
            // 
            // tm
            // 
            this.tm.AutoSize = true;
            this.tm.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tm.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.tm.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tm.ForeColor = System.Drawing.Color.White;
            this.tm.Location = new System.Drawing.Point(272, 262);
            this.tm.Name = "tm";
            this.tm.Size = new System.Drawing.Size(118, 22);
            this.tm.TabIndex = 124;
            this.tm.Text = "Tetramaster";
            this.tm.UseVisualStyleBackColor = false;
            this.tm.CheckedChanged += new System.EventHandler(this.tm_CheckedChanged_1);
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(13, 439);
            this.progressBar2.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(651, 13);
            this.progressBar2.TabIndex = 125;
            // 
            // itemsBonusSets
            // 
            this.itemsBonusSets.AutoSize = true;
            this.itemsBonusSets.BackColor = System.Drawing.Color.Transparent;
            this.itemsBonusSets.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.itemsBonusSets.ForeColor = System.Drawing.Color.White;
            this.itemsBonusSets.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.itemsBonusSets.Location = new System.Drawing.Point(291, 214);
            this.itemsBonusSets.Name = "itemsBonusSets";
            this.itemsBonusSets.Size = new System.Drawing.Size(71, 13);
            this.itemsBonusSets.TabIndex = 126;
            this.itemsBonusSets.Text = "Bonus Sets";
            // 
            // rand9er
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::rand9er.Properties.Resources.bgc;
            this.ClientSize = new System.Drawing.Size(678, 473);
            this.Controls.Add(this.itemsBonusSets);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.tm);
            this.Controls.Add(this.enemies);
            this.Controls.Add(this.item);
            this.Controls.Add(this.character);
            this.Controls.Add(this.itemsSynth);
            this.Controls.Add(this.itemsShops);
            this.Controls.Add(this.itemsTreasure);
            this.Controls.Add(this.charEquipment);
            this.Controls.Add(this.charAbilities);
            this.Controls.Add(this.charSpeciality);
            this.Controls.Add(this.charBaseStats);
            this.Controls.Add(this.tmDecks);
            this.Controls.Add(this.tmCardOrder);
            this.Controls.Add(this.tmCardStats);
            this.Controls.Add(this.enemyBlueMagic);
            this.Controls.Add(this.enemyCardDrop);
            this.Controls.Add(this.enemyItemSteals);
            this.Controls.Add(this.enemyItemDrops);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.debugButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBoxChaos);
            this.Controls.Add(this.checkBoxRecommended);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cm_stiltzkin);
            this.Controls.Add(this.cm_entrances);
            this.Controls.Add(this.b_search);
            this.Controls.Add(this.b_remove);
            this.Controls.Add(this.tb_fl);
            this.Controls.Add(this.b_open);
            this.Controls.Add(this.b_rseed);
            this.Controls.Add(this.richTextBox_debug);
            this.Controls.Add(this.textBox_seed);
            this.Controls.Add(this.label_seed);
            this.Controls.Add(this.button_rand);
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
        private System.Windows.Forms.Button button_rand;
        private System.Windows.Forms.Label label_seed;
        private System.Windows.Forms.RichTextBox richTextBox_debug;
        private System.Windows.Forms.Button b_rseed;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button b_open;
        private System.Windows.Forms.TextBox tb_fl;
        private System.Windows.Forms.Button b_remove;
        private System.Windows.Forms.Button b_search;
        private System.Windows.Forms.CheckBox cm_entrances;
        private System.Windows.Forms.CheckBox cm_stiltzkin;
        public System.Windows.Forms.TextBox textBox_seed;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBoxRecommended;
        private System.Windows.Forms.CheckBox checkBoxChaos;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button debugButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label enemyItemDrops;
        private System.Windows.Forms.Label enemyItemSteals;
        private System.Windows.Forms.Label enemyCardDrop;
        private System.Windows.Forms.Label enemyBlueMagic;
        private System.Windows.Forms.Label tmCardStats;
        private System.Windows.Forms.Label tmCardOrder;
        private System.Windows.Forms.Label tmDecks;
        private System.Windows.Forms.Label charBaseStats;
        private System.Windows.Forms.Label charSpeciality;
        private System.Windows.Forms.Label charAbilities;
        private System.Windows.Forms.Label charEquipment;
        private System.Windows.Forms.Label itemsTreasure;
        private System.Windows.Forms.Label itemsShops;
        private System.Windows.Forms.Label itemsSynth;
        private System.Windows.Forms.CheckBox character;
        private System.Windows.Forms.CheckBox item;
        private System.Windows.Forms.CheckBox enemies;
        private System.Windows.Forms.CheckBox tm;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.Label itemsBonusSets;
    }
}

