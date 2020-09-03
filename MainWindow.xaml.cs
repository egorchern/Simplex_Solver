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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace SimplexMethod
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ObjectiveFunction instance = new ObjectiveFunction();
            instance.ShowDialog();
           
            objFunction = instance.ObjFunction;
            objFunction_label.Content = "Your objective function: \n" + objFunction;
            addConstraint_btn.Click += AddConstraint_btn_Click;
            deleteConstraint_btn.Click += DeleteConstraint_btn_Click;
            solve_btn.Click += Solve_btn_Click;
            
        }

        private void Solve_btn_Click(object sender, RoutedEventArgs e) 
        {

          /*
          Event handler that is raised if solve button is clicked, Calls CheckFormatConstraint on every constraint in order to check if they are in correct format;
          If all constraints are in correct format call ExtractInfo subroutine; If at least one constraint is not in correct format - display error message box
          */
            bool AllConstraintsInRightFormat = true;
            for (int i = 0; i < constraints_lst.Items.Count; i++)
            {
                TextBox currentSelection = constraints_lst.Items[i] as TextBox;
                string currentConstraint = currentSelection.Text;
                currentConstraint = currentConstraint.Replace(" ", "").ToLower();
                bool constraintPassed = CheckIfConstraintIsCorrect(currentConstraint);
                if (constraintPassed == false)
                {
                    AllConstraintsInRightFormat = false;
                    MessageBox.Show("At least one constraint is not in the right format!");
                    break;
                }

            }
            if (AllConstraintsInRightFormat == true)
            {
                ExtractInfo();
                for(int i = 0; i < ObjectiveQuontifiers.Count; i++)
                {
                    ObjectiveQuontifiers[i] *= -1;
                }
                int moreThans = GetNumOfMoreThanConstraints();
                int numofSlacks = GetNumOfSlacks();
                SimplexOutput simplexOutput = new SimplexOutput(ConstraintsQuontifiers, lettersConstraints, ObjectiveQuontifiers, ObjectiveLetters, MainObjLetter,moreThans,numofSlacks);
                simplexOutput.Show();
                this.Close();

            }
        }
        public static bool CheckIfConstraintIsCorrect(string str) //Uses Regular Expressions to check if the constraint is in the correct format
        {
            
            if (Regex.IsMatch(str, @"^([+-]?[0-9]+(\.[0-9]+)?[a-z])+[<>]=[+-]?[0-9]+(\.[0-9]+)*$"))
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        public  void ExtractInfo() //Extracts the numerical information about constraints and the objective function and also stores the letters in the separate list from numerical values next to letters
        {
            int numofSlacks = GetNumOfSlacks();
            int moreThans = GetNumOfMoreThanConstraints();
            int lessThans = numofSlacks - moreThans * 2;
            for (int i = 0; i < constraints_lst.Items.Count; i++)
            {
                TextBox currentSelection = constraints_lst.Items[i] as TextBox;
                string currentConstraint = currentSelection.Text;
                currentConstraint = currentConstraint.Replace(" ", "").ToLower();
                bool morethanConst = ClassifyConstraintIsMoreThan(currentConstraint);
                MatchCollection match = Regex.Matches(currentConstraint, @"(?<factor>[+-]?[0-9]+(\.[0-9]+)*)(?<letter>[a-z])?");
                List<double> CurConstraint = new List<double>();
                List<string> letters = new List<string>();
                
                
                foreach (Match x in match)
                {
                    double temp = Convert.ToDouble(x.Groups["factor"].Value);
                    string letter = x.Groups["letter"].Value;
                    CurConstraint.Add(temp);
                    letters.Add(letter);

                    
                }
                if (morethanConst == false)
                {
                    

                    for (int q = 1; q <= numofSlacks - moreThans * 2; q++)
                    {
                        if (q == Rcounter)
                        {
                            CurConstraint.Insert(CurConstraint.Count - 1, 1);
                            letters.Insert(letters.Count - 1, $"r{q}");
                        }
                        else
                        {
                            CurConstraint.Insert(CurConstraint.Count - 1, 0);
                            letters.Insert(letters.Count - 1, $"r{q}");
                        }
                    }
                    Rcounter++;
                    for (int r = 1; r <= moreThans; r++)
                    {
                        
                            CurConstraint.Insert(CurConstraint.Count - 1, 0);
                            letters.Insert(letters.Count - 1, $"s{r}");
                            CurConstraint.Insert(CurConstraint.Count - 1, 0);
                            letters.Insert(letters.Count - 1, $"a{r}");


                    }
                    
                }
                else
                {
                    
                    for(int q = 1; q <= lessThans; q++)
                    {
                        CurConstraint.Insert(CurConstraint.Count - 1, 0);
                        letters.Insert(letters.Count - 1, $"r{q}");
                    }
                    for(int q = 1; q<= moreThans; q++)
                    {
                        if(q == Scounter)
                        {
                            CurConstraint.Insert(CurConstraint.Count - 1, -1);
                            letters.Insert(letters.Count - 1, $"s{q}");
                            CurConstraint.Insert(CurConstraint.Count - 1, 1);
                            letters.Insert(letters.Count - 1, $"a{q}");
                        }
                        else
                        {
                            CurConstraint.Insert(CurConstraint.Count - 1, 0);
                            letters.Insert(letters.Count - 1, $"s{q}");
                            CurConstraint.Insert(CurConstraint.Count - 1, 0);
                            letters.Insert(letters.Count - 1, $"a{q}");
                        }
                    }
                    Scounter++;
                }
                ConstraintsQuontifiers.Add(CurConstraint);
                lettersConstraints.Add(letters);
                
                
                
            }
            string currentObj = objFunction;
            
            MatchCollection match1 = Regex.Matches(currentObj, @"(?<factor>[+-]?[0-9]+(\.[0-9]+)*)(?<letter>[a-z])?");
            List<double> CurConstraint1 = new List<double>();
            List<string> letters1 = new List<string>();
            
            foreach (Match x in match1)
            {
                double temp1 = Convert.ToDouble(x.Groups["factor"].Value);
                string letter1 = x.Groups["letter"].Value;
                CurConstraint1.Add(temp1);
                letters1.Add(letter1);


            }
            CurConstraint1.Add(0);
            letters1.Add("");
            MainObjLetter = Convert.ToString(currentObj[0]);

             

            for (int q = 1; q <= numofSlacks - moreThans * 2; q++)
            {
                
               
                    CurConstraint1.Insert(CurConstraint1.Count - 1, 0);
                    letters1.Insert(letters1.Count - 1, $"r{q}");
                
            }
            
            for (int r = 1; r <= moreThans; r++)
            {

                CurConstraint1.Insert(CurConstraint1.Count - 1, 0);
                letters1.Insert(letters1.Count - 1, $"s{r}");
                CurConstraint1.Insert(CurConstraint1.Count - 1, 0);
                letters1.Insert(letters1.Count - 1, $"a{r}");


            }
            ObjectiveQuontifiers = CurConstraint1;
            ObjectiveLetters = letters1;
            

        }
        public bool ClassifyConstraintIsMoreThan(string str)
        {
            Match match = Regex.Match(str, @"^([+-]?[0-9]+(\.[0-9]+)*[a-z])+(?<sign>[<>])=[+-]?[0-9]+(\.[0-9]+)*$");
            if(match.Success == true)
            {
                var temp = match.Groups["sign"].Value;
                switch (temp)
                {
                    case "<":
                        return false;
                    case ">":
                        return true;
                        
                }
                
            }
            return false;
        }
        public int GetNumOfSlacks() //Returns the number of slack variables that have to be used.
        {
            int counterOfSlacks = 0;
            for(int i = 0; i < constraints_lst.Items.Count; i++)
            {
                TextBox text = constraints_lst.Items[i] as TextBox;
                string constraint = text.Text;
                if(ClassifyConstraintIsMoreThan(constraint) == false)
                {
                    counterOfSlacks++;
                }
                else
                {
                    counterOfSlacks += 2;
                }
            }
            return counterOfSlacks;
        }
        public int GetNumOfMoreThanConstraints() //Return the number of constraints that have more than signs in them
        {
            int counter = 0;
            for(int i = 0; i < constraints_lst.Items.Count; i++)
            {
                TextBox box = constraints_lst.Items[i] as TextBox;
                string str = box.Text;
                if(ClassifyConstraintIsMoreThan(str) == true)
                {
                    counter++;
                }
            }
            return counter;
        }
            
            
            
        
        private void DeleteConstraint_btn_Click(object sender, RoutedEventArgs e) // Delete the last fild from the ListBox. This removes one constraint field
        {
            if (constraints_lst.Items.Count != 0)
            {


                constraints_lst.Items.RemoveAt(constraints_lst.Items.Count - 1);
            }
            else
            {
                MessageBox.Show("No field left to delete!");
            }
            
        }

        private void AddConstraint_btn_Click(object sender, RoutedEventArgs e) // Add another field to the ListBox. Which in effect adds a new constraint field
        {
            TextBox textBox = new TextBox();
            textBox.Width = 473;
            textBox.FontSize = 20;
            textBox.FontWeight = FontWeights.Bold;
            constraints_lst.Items.Add(textBox);
        } 





        private static string objFunction;
        private static List<List<double>> ConstraintsQuontifiers = new List<List<double>>();
        private static List<List<string>> lettersConstraints = new List<List<string>>();
        private static List<double> ObjectiveQuontifiers = new List<double>();
        private static List<string> ObjectiveLetters = new List<string>();
        private static string MainObjLetter = "";
        private static int Rcounter = 1;
        private static int Scounter = 1;
        
        
    }
}
