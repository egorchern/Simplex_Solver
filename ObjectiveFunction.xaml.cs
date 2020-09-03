using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace SimplexMethod
{
    /// <summary>
    /// Interaction logic for ObjectiveFunction.xaml
    /// </summary>
    public partial class ObjectiveFunction : Window
    {
        public ObjectiveFunction()
        {
            InitializeComponent();
            submit_btn.Click += Submit_btn_Click;
        }

        private void Submit_btn_Click(object sender, RoutedEventArgs e) //Check if the objective function is in correct format and if not display error message box
        {
            string objCandidate = objectiveFunction_txt.Text;
            objCandidate = objCandidate.Replace(" ", "").ToLower();
            if (Regex.IsMatch(objCandidate, @"^[a-z]=([+-]?[0-9]+(\.[0-9]+)?[a-z])+$"))
            {
                ObjFunction = objCandidate;
                this.Close();
            }
            else
            {
                MessageBox.Show("Invalid objective function entered");
            }
        }
        public string ObjFunction
        {
            get { return Obj; }
            set { Obj = value; }
        }
        private string Obj;
       
            
            

        
    }
}
