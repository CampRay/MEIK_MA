using MEIKReport.Common;
using MEIKReport.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace MEIKReport
{
    public partial class AddEmailPage : Window
    {
        private ObservableCollection<string> _emailList;
        public AddEmailPage(ObservableCollection<string> emailList)
        {
            InitializeComponent();
            this.txtEmail.Focus();
            this._emailList = emailList;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtEmail.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_78").ToString());
                this.txtEmail.Focus();
            }
            else if (this.txtEmail.Text.IndexOf(';') >= 0)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_29").ToString());
                this.txtEmail.Focus();
            }
            else if (this.txtEmail.Text.IndexOf('@') < 0)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_79").ToString());
                this.txtEmail.Focus();
            }
            else
            {
                foreach (var item in _emailList)
                {
                    if (item.Equals(this.txtEmail.Text))
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_80").ToString());
                        return;
                    }
                }
                _emailList.Add(this.txtEmail.Text);
                this.Close();
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
