using System.Windows.Forms;

namespace ExchangeApp
{
    /// <summary>
    /// Config form, with very simple input fields.
    /// </summary>
    public partial class FormConfig : Form
    {
        public FormConfig()
        {
            InitializeComponent();
        }

        public string ExchangeUrl
        {
            get => exchangeUrlText.Text;
            set => exchangeUrlText.Text = value;
        }

        public string User
        {
            get => userText.Text;
            set => userText.Text = value;
        }

        public string Password
        {
            get => passwordText.Text;
            set => passwordText.Text = value;
        }

        public bool ShowMeetingIds
        {
            get => showMeetingIdsCheckbox.Checked;
            set => showMeetingIdsCheckbox.Checked = value;
        }
    }
}
