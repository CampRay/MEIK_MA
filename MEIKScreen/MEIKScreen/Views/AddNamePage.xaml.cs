using MEIKScreen.Common;
using MEIKScreen.Model;
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

namespace MEIKScreen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class AddNamePage : Window
    {
        private ObservableCollection<User> _userList;
        public AddNamePage(ObservableCollection<User> userList)
        {
            InitializeComponent();
            this.txtName.Focus();
            this._userList = userList;
        }                        

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.txtName.Text))
            {
                MessageBox.Show(this, App.Current.FindResource("Message_11").ToString());
                this.txtName.Focus();
            }
            else if (this.txtName.Text.IndexOf(';') >= 0 || this.txtName.Text.IndexOf('|') >= 0 )
            {
                MessageBox.Show(this, App.Current.FindResource("Message_29").ToString());
                this.txtName.Focus();
            }
            else if (this.txtLicense.Text.IndexOf(';') >= 0 || this.txtLicense.Text.IndexOf('|') >= 0)
            {
                MessageBox.Show(this, App.Current.FindResource("Message_29").ToString());
                this.txtLicense.Focus();
            }
            else
            {
                foreach (var item in _userList)
                {
                    if (item.Name.Equals(this.txtName.Text))
                    {
                        MessageBox.Show(this, App.Current.FindResource("Message_12").ToString());
                        return;
                    }
                }
                User user = new User();
                user.Name = this.txtName.Text;
                user.License = this.txtLicense.Text;
                _userList.Add(user);
                this.Close();       
            }

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }                
        
    }
}
