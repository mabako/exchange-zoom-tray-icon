namespace ExchangeApp
{
    partial class FormConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormConfig));
            this.save = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.exchangeUrlText = new System.Windows.Forms.TextBox();
            this.exchangeUrlLabel = new System.Windows.Forms.Label();
            this.userLabel = new System.Windows.Forms.Label();
            this.userText = new System.Windows.Forms.TextBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // save
            // 
            resources.ApplyResources(this.save, "save");
            this.save.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.save.Name = "save";
            this.save.UseVisualStyleBackColor = true;
            // 
            // cancel
            // 
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // exchangeUrlText
            // 
            resources.ApplyResources(this.exchangeUrlText, "exchangeUrlText");
            this.exchangeUrlText.Name = "exchangeUrlText";
            // 
            // exchangeUrlLabel
            // 
            resources.ApplyResources(this.exchangeUrlLabel, "exchangeUrlLabel");
            this.exchangeUrlLabel.Name = "exchangeUrlLabel";
            // 
            // userLabel
            // 
            resources.ApplyResources(this.userLabel, "userLabel");
            this.userLabel.Name = "userLabel";
            // 
            // userText
            // 
            resources.ApplyResources(this.userText, "userText");
            this.userText.Name = "userText";
            // 
            // passwordLabel
            // 
            resources.ApplyResources(this.passwordLabel, "passwordLabel");
            this.passwordLabel.Name = "passwordLabel";
            // 
            // passwordText
            // 
            resources.ApplyResources(this.passwordText, "passwordText");
            this.passwordText.Name = "passwordText";
            this.passwordText.UseSystemPasswordChar = true;
            // 
            // FormConfig
            // 
            this.AcceptButton = this.save;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.Controls.Add(this.passwordLabel);
            this.Controls.Add(this.passwordText);
            this.Controls.Add(this.userLabel);
            this.Controls.Add(this.userText);
            this.Controls.Add(this.exchangeUrlLabel);
            this.Controls.Add(this.exchangeUrlText);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.save);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormConfig";
            this.ShowIcon = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label exchangeUrlLabel;
        private System.Windows.Forms.TextBox exchangeUrlText;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox passwordText;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.Label userLabel;
        private System.Windows.Forms.TextBox userText;

        #endregion
    }
}
