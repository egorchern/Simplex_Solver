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
    /// Interaction logic for SimplexOutput.xaml
    /// </summary>
    public partial class SimplexOutput : Window
    {
        public SimplexOutput(List<List<double>>Constraints,List<List<string>> Letters,List<double>Objective,List<string>ObjectiveLetters,string MainLetter,int moreThans,int numOfSlacks)
        {
            InitializeComponent();
            view.ViewingMode = FlowDocumentReaderViewingMode.Scroll;
            ConstraintsL = Constraints;
            LettersL = Letters;
            ObjectiveL = Objective;
            ObjectiveLettersL = ObjectiveLetters;
            MainLetterL = MainLetter;
            if(moreThans > 0)
            {
                CreateTwoStageConstraint(moreThans,numOfSlacks);
            }
            InitializeBasicVariable();
            DisplayCurrentTable();
            
            if(moreThans > 0)
            {
                Perform_First_Stage();
            }
            else
            {
                Simplex();

            }


        }
        public void CreateTwoStageConstraint(int moreThans,int numOfSlacks) //Creates a special function that is used only in Two-Stage Simplex. Function minimizes the sum of artificial variables
        {
            List<List<double>> ConstrBuilder = new List<List<double>>();
            List<int> indexesOfAs = new List<int>();
            for(int i = 1; i <= moreThans; i++)
            {
                string toFind = $"a{i}";
                int indexOfToFind = LettersL[0].IndexOf(toFind);
                indexesOfAs.Add(indexOfToFind);
                int indexOfConstraint = 0;
                for(int q = 0; q < ConstraintsL.Count; q++)
                {
                    var cur = ConstraintsL[q];
                    if(cur[indexOfToFind] == 1)
                    {
                        indexOfConstraint = q;
                        break;
                    }
                }
                List<double> Re_arranged = new List<double>();
                var current = ConstraintsL[indexOfConstraint];
                for(int q = 0; q < current.Count; q++)
                {
                    if(q != indexOfToFind && q != current.Count - 1)
                    {
                        Re_arranged.Add(current[q] * -1);
                    }
                    else
                    {
                        Re_arranged.Add(current[q]);
                    }
                }
                ConstrBuilder.Add(Re_arranged);
            }
            
            
            List<double> SavingValues = new List<double>();
            for(int i = 0; i < ConstrBuilder[0].Count; i++)
            {
                SavingValues.Add(0);
            }

            for(int i = 0; i < ConstrBuilder.Count; i+=2)
            {
                var current_one = ConstrBuilder[i];
                if(i + 1 != ConstrBuilder.Count)
                {
                    var next_one = ConstrBuilder[i + 1];
                    for(int q = 0; q < current_one.Count; q++)
                    {

                        SavingValues[q] += current_one[q] + next_one[q];
                    }

                }
                else
                {
                    for(int q = 0; q < current_one.Count; q++)
                    {
                        SavingValues[q] += current_one[q];
                    }
                }
            }
            for(int i = 0; i < indexesOfAs.Count; i++)
            {
                int index_of_A = indexesOfAs[i];
                SavingValues[index_of_A] = 0;
            }
            if(SavingValues.Count == ObjectiveLettersL.Count)
            {
                SavingValues[SavingValues.Count - 1] *= -1;
            }
            else
            {
                SavingValues.Add(0);
            }
            SpecialTwoStageConstraint = SavingValues;

            
        }
        public void Perform_First_Stage()//Recursively applies the Simplex until the special function(SpecialTwoStageConstraint) is optimized. Then if the feasible region exists calls normal Simplex
        {
            int pivotColumn = FindIndexOfLeastPositiveTwoStage();


            if (pivotColumn != -1)
            {
                var Thetas = CalculateThetaValues(pivotColumn);
                int pivotRow = -1;
                for (int i = 0; i < Thetas.Count; i++)
                {
                    if (Thetas[i] >= 0)
                    {
                        if (pivotRow == -1)
                        {
                            pivotRow = i;
                        }
                        else
                        {
                            if (Thetas[i] < Thetas[pivotRow])
                            {
                                pivotRow = i;
                            }
                        }
                    }
                }
                double pivotVal = ConstraintsL[pivotRow][pivotColumn];
                BasicVariables[pivotRow] = LettersL[0][pivotColumn];
                var local = ConstraintsL[pivotRow];
                for (int i = 0; i < local.Count; i++)
                {
                    local[i] = local[i] / pivotVal;
                }
                ConstraintsL[pivotRow] = local;
                for (int i = 0; i < ConstraintsL.Count; i++)
                {
                    var currentConstraint = ConstraintsL[i];
                    if (i != pivotRow)
                    {
                        double adjustmentFactor = -currentConstraint[pivotColumn];
                        for (int q = 0; q < currentConstraint.Count; q++)
                        {
                            currentConstraint[q] = currentConstraint[q] + adjustmentFactor * ConstraintsL[pivotRow][q];

                        }
                        ConstraintsL[i] = currentConstraint;
                    }
                }
                var obj = ObjectiveL;
                double adjFactor = -obj[pivotColumn];
                for (int i = 0; i < obj.Count; i++)
                {
                    obj[i] = obj[i] + adjFactor * ConstraintsL[pivotRow][i];
                }
                ObjectiveL = obj;


                var two_stage = SpecialTwoStageConstraint;

                adjFactor = -two_stage[pivotColumn];
                for (int i = 0; i < two_stage.Count; i++)
                {
                    two_stage[i] = two_stage[i] + adjFactor * ConstraintsL[pivotRow][i];
                }
                SpecialTwoStageConstraint = two_stage;
                DisplayCurrentTable();
                Perform_First_Stage();

            }
            else
            {
                if(SpecialTwoStageConstraint.Last() != 0)
                {
                    MessageBox.Show("There is no feasible region for this problem. No optimal solutions");
                }
                else
                {
                    SpecialTwoStageConstraint.Clear();
                    Simplex();
                }
            }
        }
        
        public void InitializeBasicVariable()//Initializes basic variables
        {
            List<string> basicVars = new List<string>();
            foreach (var item in LettersL[0])
            {
                if (Regex.IsMatch(item, @"[r|a]\d") == true)
                {
                    basicVars.Add(item);
                }
            }
            BasicVariables = basicVars;
        }
        public void DisplayCurrentTable()//Creates a new table that contains the current state of all relevant variables like ConstaintsL
        {

            Border border = new Border();
            
            Table tbl1 = new Table();
            flow_dc.Blocks.Add(tbl1);
            tbl1.BorderBrush = Brushes.Black;
            tbl1.CellSpacing = 4;
            tbl1.BorderBrush = Brushes.Black;
            tbl1.BorderThickness = new Thickness(2,2,2,2);

            
            tbl1.RowGroups.Add(new TableRowGroup());

            
            tbl1.RowGroups[0].Rows.Add(new TableRow());
           TableRow currentRow = tbl1.RowGroups[0].Rows[0];

            
            currentRow.FontSize = 14;
            currentRow.FontWeight = FontWeights.Bold;

           
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("basic variable"))));

            for (int i = 0; i < LettersL[0].Count; i++)
            {
                if (LettersL[0][i] != "")
                {


                    currentRow.Cells.Add(new TableCell(new Paragraph(new Run(LettersL[0][i]))));
                }
            }
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run("value"))));
            

            for (int i = 0; i < ConstraintsL.Count; i++)
            {


                
                tbl1.RowGroups[0].Rows.Add(new TableRow());
                currentRow = tbl1.RowGroups[0].Rows[i + 1];

               
                currentRow.FontSize = 12;


                
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(BasicVariables[i]))));
                var curConstraint = ConstraintsL[i];
                for (int q = 0; q < curConstraint.Count; q++)
                {

                    currentRow.Cells.Add(new TableCell(new Paragraph(new Run(Convert.ToString(curConstraint[q])))));
                }

            }
            tbl1.RowGroups[0].Rows.Add(new TableRow());
            currentRow = tbl1.RowGroups[0].Rows[ConstraintsL.Count + 1];
            currentRow.Cells.Add(new TableCell(new Paragraph(new Run(MainLetterL))));
            foreach (var item in ObjectiveL)
            {
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run(Convert.ToString(item)))));
            }
            if (SpecialTwoStageConstraint.Count != 0)
            {


                tbl1.RowGroups[0].Rows.Add(new TableRow());
                currentRow = tbl1.RowGroups[0].Rows[ConstraintsL.Count + 2];
                currentRow.Cells.Add(new TableCell(new Paragraph(new Run("I"))));

                foreach (var item in SpecialTwoStageConstraint)
                {
                    currentRow.Cells.Add(new TableCell(new Paragraph(new Run(Convert.ToString(item)))));
                }
            }
            

        }
        public void Simplex()// Recursively applies the simplex algorithm until an optimal solution is reached
        {
            
            
            int pivotColumn = FindIndexOfLeastPositiveObjective();
            
            if (pivotColumn != -1)
            {
                var Thetas = CalculateThetaValues(pivotColumn);
                int pivotRow = -1;
                for (int i = 0; i < Thetas.Count; i++)
                {
                    if (Thetas[i] >= 0)
                    {
                        if (pivotRow == -1)
                        {
                            pivotRow = i;
                        }
                        else
                        {
                            if (Thetas[i] < Thetas[pivotRow])
                            {
                                pivotRow = i;
                            }
                        }
                    }
                }
                if (pivotRow != -1)
                {


                    double pivotVal = ConstraintsL[pivotRow][pivotColumn];
                    BasicVariables[pivotRow] = LettersL[0][pivotColumn];
                    var local = ConstraintsL[pivotRow];
                    for (int i = 0; i < local.Count; i++)
                    {
                        local[i] = local[i] / pivotVal;
                    }
                    ConstraintsL[pivotRow] = local;
                    for (int i = 0; i < ConstraintsL.Count; i++)
                    {
                        var currentConstraint = ConstraintsL[i];
                        if (i != pivotRow)
                        {
                            double adjustmentFactor = -currentConstraint[pivotColumn];
                            for (int q = 0; q < currentConstraint.Count; q++)
                            {
                                currentConstraint[q] = currentConstraint[q] + adjustmentFactor * ConstraintsL[pivotRow][q];

                            }
                            ConstraintsL[i] = currentConstraint;
                        }
                    }
                    var obj = ObjectiveL;
                    double adjFactor = -obj[pivotColumn];
                    for (int i = 0; i < obj.Count; i++)
                    {
                        obj[i] = obj[i] + adjFactor * ConstraintsL[pivotRow][i];
                    }
                    ObjectiveL = obj;
                    DisplayCurrentTable();
                    Simplex();
                }
                else
                {
                    MessageBox.Show("This problem has no optimal solution");
                }
                
            }
            else
            {

                string optimal_solution = "Optimal solution: (r1,s1,a1 are slack variables and are displayed for eduction purpouse only): ";
                for(int i = 0; i < BasicVariables.Count; i++)
                {
                    optimal_solution += $"\n{BasicVariables[i]} = {ConstraintsL[i].Last()}";
                }
                optimal_solution += $"\nThe optimal value of objective function\n{MainLetterL} = {ObjectiveL.Last()}";
                MessageBox.Show(optimal_solution);
            }
        }
        public List<double> CalculateThetaValues(int pivotColumn)
        {
            
            
            List<double> ThetaList = new List<double>();
            for(int i = 0; i < ConstraintsL.Count; i++)
            {
                ThetaList.Add(ConstraintsL[i][ConstraintsL[i].Count - 1] / ConstraintsL[i][pivotColumn]);

                
            }
            return ThetaList;
        }
        public int FindIndexOfLeastPositiveObjective()
        {
            int indexOfMin = -1;
            for (int i = 0; i < ObjectiveL.Count - 1; i++)
            {
                if (ObjectiveLettersL[i].StartsWith("a") == false)
                {


                    if (ObjectiveL[i] < 0)
                    {
                        if (indexOfMin == -1)
                        {
                            indexOfMin = i;
                        }
                        else
                        {
                            if (ObjectiveL[i] < ObjectiveL[indexOfMin])
                            {
                                indexOfMin = i;
                            }
                        }

                    }
                }
            }
            return indexOfMin;
        }
        public int FindIndexOfLeastPositiveTwoStage()
        {
            int indexOfMin = -1;
            for (int i = 0; i < SpecialTwoStageConstraint.Count -1 ; i++)
            {
                if (SpecialTwoStageConstraint[i] < 0)
                {
                    if (indexOfMin == -1)
                    {
                        indexOfMin = i;
                    }
                    else
                    {
                        if (SpecialTwoStageConstraint[i] < SpecialTwoStageConstraint[indexOfMin])
                        {
                            indexOfMin = i;
                        }
                    }

                }
            }
            return indexOfMin;
        }

        private static  List<string> BasicVariables = new List<string>();
        private static  List<List<double>> ConstraintsL = new List<List<double>>();
        private static List<List<string>> LettersL = new List<List<string>>();
        private static List<double> ObjectiveL = new List<double>();
        private static List<string> ObjectiveLettersL = new List<string>();
        private static string MainLetterL = "";
        private static List<double> SpecialTwoStageConstraint = new List<double>();
    }
}
