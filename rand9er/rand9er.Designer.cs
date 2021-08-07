
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
            this.radio_shopitems = new System.Windows.Forms.RadioButton();
            this.radio_synthesis = new System.Windows.Forms.RadioButton();
            this.radio_defaultequipment = new System.Windows.Forms.RadioButton();
            this.radio_basestats = new System.Windows.Forms.RadioButton();
            this.radio_abilitygems = new System.Windows.Forms.RadioButton();
            this.radio_levels = new System.Windows.Forms.RadioButton();
            this.label_item = new System.Windows.Forms.Label();
            this.label_char = new System.Windows.Forms.Label();
            this.label_output = new System.Windows.Forms.Label();
            this.label_seed = new System.Windows.Forms.Label();
            this.textBox_seed = new System.Windows.Forms.TextBox();
            this.richTextBox_debug = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // richTextBox_output
            // 
            this.richTextBox_output.ForeColor = System.Drawing.SystemColors.InfoText;
            this.richTextBox_output.Location = new System.Drawing.Point(152, 83);
            this.richTextBox_output.Margin = new System.Windows.Forms.Padding(2);
            this.richTextBox_output.Name = "richTextBox_output";
            this.richTextBox_output.ReadOnly = true;
            this.richTextBox_output.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.richTextBox_output.Size = new System.Drawing.Size(204, 233);
            this.richTextBox_output.TabIndex = 1;
            this.richTextBox_output.Text = "int getRandomNumber() {\n     return 4; // chosen by fair dice roll.\n             " +
    "       // guaranteed to be random.\n}";
            this.richTextBox_output.WordWrap = false;
            // 
            // button_rand
            // 
            this.button_rand.Location = new System.Drawing.Point(26, 268);
            this.button_rand.Margin = new System.Windows.Forms.Padding(2);
            this.button_rand.Name = "button_rand";
            this.button_rand.Size = new System.Drawing.Size(95, 48);
            this.button_rand.TabIndex = 2;
            this.button_rand.Text = "quantum randomizer button";
            this.button_rand.UseVisualStyleBackColor = true;
            this.button_rand.Click += new System.EventHandler(this.button_rand_Click);
            // 
            // radio_shopitems
            // 
            this.radio_shopitems.AutoSize = true;
            this.radio_shopitems.Location = new System.Drawing.Point(26, 83);
            this.radio_shopitems.Margin = new System.Windows.Forms.Padding(2);
            this.radio_shopitems.Name = "radio_shopitems";
            this.radio_shopitems.Size = new System.Drawing.Size(78, 17);
            this.radio_shopitems.TabIndex = 3;
            this.radio_shopitems.TabStop = true;
            this.radio_shopitems.Text = "Shop Items";
            this.radio_shopitems.UseVisualStyleBackColor = true;
            this.radio_shopitems.CheckedChanged += new System.EventHandler(this.radio_shopitems_CheckedChanged);
            // 
            // radio_synthesis
            // 
            this.radio_synthesis.AutoSize = true;
            this.radio_synthesis.Location = new System.Drawing.Point(26, 104);
            this.radio_synthesis.Margin = new System.Windows.Forms.Padding(2);
            this.radio_synthesis.Name = "radio_synthesis";
            this.radio_synthesis.Size = new System.Drawing.Size(70, 17);
            this.radio_synthesis.TabIndex = 4;
            this.radio_synthesis.TabStop = true;
            this.radio_synthesis.Text = "Synthesis";
            this.radio_synthesis.UseVisualStyleBackColor = true;
            this.radio_synthesis.CheckedChanged += new System.EventHandler(this.radio_synthesis_CheckedChanged);
            // 
            // radio_defaultequipment
            // 
            this.radio_defaultequipment.AutoSize = true;
            this.radio_defaultequipment.Location = new System.Drawing.Point(26, 168);
            this.radio_defaultequipment.Margin = new System.Windows.Forms.Padding(2);
            this.radio_defaultequipment.Name = "radio_defaultequipment";
            this.radio_defaultequipment.Size = new System.Drawing.Size(109, 17);
            this.radio_defaultequipment.TabIndex = 5;
            this.radio_defaultequipment.TabStop = true;
            this.radio_defaultequipment.Text = "default equipment";
            this.radio_defaultequipment.UseVisualStyleBackColor = true;
            this.radio_defaultequipment.CheckedChanged += new System.EventHandler(this.radio_defaultequipment_CheckedChanged);
            // 
            // radio_basestats
            // 
            this.radio_basestats.AutoSize = true;
            this.radio_basestats.Location = new System.Drawing.Point(26, 190);
            this.radio_basestats.Name = "radio_basestats";
            this.radio_basestats.Size = new System.Drawing.Size(73, 17);
            this.radio_basestats.TabIndex = 6;
            this.radio_basestats.TabStop = true;
            this.radio_basestats.Text = "base stats";
            this.radio_basestats.UseVisualStyleBackColor = true;
            this.radio_basestats.CheckedChanged += new System.EventHandler(this.radio_basestats_CheckedChanged);
            // 
            // radio_abilitygems
            // 
            this.radio_abilitygems.AutoSize = true;
            this.radio_abilitygems.Location = new System.Drawing.Point(26, 213);
            this.radio_abilitygems.Name = "radio_abilitygems";
            this.radio_abilitygems.Size = new System.Drawing.Size(80, 17);
            this.radio_abilitygems.TabIndex = 7;
            this.radio_abilitygems.TabStop = true;
            this.radio_abilitygems.Text = "Ability gems";
            this.radio_abilitygems.UseVisualStyleBackColor = true;
            this.radio_abilitygems.CheckedChanged += new System.EventHandler(this.radio_abilitygems_CheckedChanged);
            // 
            // radio_levels
            // 
            this.radio_levels.AutoSize = true;
            this.radio_levels.Location = new System.Drawing.Point(26, 236);
            this.radio_levels.Name = "radio_levels";
            this.radio_levels.Size = new System.Drawing.Size(121, 17);
            this.radio_levels.TabIndex = 8;
            this.radio_levels.TabStop = true;
            this.radio_levels.Text = "random level curve?";
            this.radio_levels.UseVisualStyleBackColor = true;
            this.radio_levels.CheckedChanged += new System.EventHandler(this.radio_levels_CheckedChanged);
            // 
            // label_item
            // 
            this.label_item.AutoSize = true;
            this.label_item.Location = new System.Drawing.Point(13, 59);
            this.label_item.Name = "label_item";
            this.label_item.Size = new System.Drawing.Size(32, 13);
            this.label_item.TabIndex = 9;
            this.label_item.Text = "Items";
            // 
            // label_char
            // 
            this.label_char.AutoSize = true;
            this.label_char.Location = new System.Drawing.Point(13, 144);
            this.label_char.Name = "label_char";
            this.label_char.Size = new System.Drawing.Size(29, 13);
            this.label_char.TabIndex = 10;
            this.label_char.Text = "Char";
            // 
            // label_output
            // 
            this.label_output.AutoSize = true;
            this.label_output.Location = new System.Drawing.Point(149, 59);
            this.label_output.Name = "label_output";
            this.label_output.Size = new System.Drawing.Size(39, 13);
            this.label_output.TabIndex = 11;
            this.label_output.Text = "Output";
            // 
            // label_seed
            // 
            this.label_seed.AutoSize = true;
            this.label_seed.Location = new System.Drawing.Point(16, 21);
            this.label_seed.Name = "label_seed";
            this.label_seed.Size = new System.Drawing.Size(39, 13);
            this.label_seed.TabIndex = 12;
            this.label_seed.Text = "SEED:";
            // 
            // textBox_seed
            // 
            this.textBox_seed.Location = new System.Drawing.Point(61, 18);
            this.textBox_seed.Name = "textBox_seed";
            this.textBox_seed.Size = new System.Drawing.Size(295, 20);
            this.textBox_seed.TabIndex = 13;
            // 
            // richTextBox_debug
            // 
            this.richTextBox_debug.Location = new System.Drawing.Point(379, 41);
            this.richTextBox_debug.Name = "richTextBox_debug";
            this.richTextBox_debug.Size = new System.Drawing.Size(100, 275);
            this.richTextBox_debug.TabIndex = 14;
            this.richTextBox_debug.Text = "no bugs yet";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(379, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Debug";
            // 
            // rand9er
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(491, 330);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.richTextBox_debug);
            this.Controls.Add(this.textBox_seed);
            this.Controls.Add(this.label_seed);
            this.Controls.Add(this.label_output);
            this.Controls.Add(this.label_char);
            this.Controls.Add(this.label_item);
            this.Controls.Add(this.radio_levels);
            this.Controls.Add(this.radio_abilitygems);
            this.Controls.Add(this.radio_basestats);
            this.Controls.Add(this.radio_defaultequipment);
            this.Controls.Add(this.radio_synthesis);
            this.Controls.Add(this.radio_shopitems);
            this.Controls.Add(this.button_rand);
            this.Controls.Add(this.richTextBox_output);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "rand9er";
            this.Text = "FFIX Randomizer Assistant";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RichTextBox richTextBox_output;
        private System.Windows.Forms.Button button_rand;
        private System.Windows.Forms.RadioButton radio_shopitems;
        private System.Windows.Forms.RadioButton radio_synthesis;
        private System.Windows.Forms.RadioButton radio_defaultequipment;
        private System.Windows.Forms.RadioButton radio_basestats;
        private System.Windows.Forms.RadioButton radio_abilitygems;
        private System.Windows.Forms.RadioButton radio_levels;
        private System.Windows.Forms.Label label_item;
        private System.Windows.Forms.Label label_char;
        private System.Windows.Forms.Label label_output;
        private System.Windows.Forms.Label label_seed;
        private System.Windows.Forms.TextBox textBox_seed;
        private System.Windows.Forms.RichTextBox richTextBox_debug;
        private System.Windows.Forms.Label label1;
    }
}

