/**
 * 
  Author:    Daniel Pak 
  Date:      Nov 13,  2020
  Course:    CS 4540, University of Utah, School of Computing
  Copyright: CS 4540, and Daniel Pak - This work may not be copied for use in Academic Coursework.

  I, Daniel Pak, certify that I wrote this code from scratch and did not copy it in part or whole from
  another source.  Any references used in the completion of the assignment are cited in my README file.

  File Contents

     Form1

 */

using CsvHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WebScraper
{
    public partial class Form1 : Form
    {
        private ChromeDriver _driver;

        private string year;
        private string yearToDigit;
        private string semester;
        private string semesterToDigit;
        private bool driverFlag;

        private List<List<string>> tempData;
        private List<Data> records;

        public Form1()
        {
            InitializeComponent();
            saveBtn.Enabled = false;
            driverFlag = false;
            records = new List<Data>();

        }

        /// <summary>
        /// Search item by user input: Year, Semester, and total number of scrape
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
                  
            try
            {

                if (driverFlag == false)
                {
                    _driver = new ChromeDriver();
                    _driver.Manage().Window.Maximize();
                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);


                    driverFlag = true;
                }

                Cursor.Current = Cursors.WaitCursor;

                discriptionBtn.Enabled = false;
                string url = "https://student.apps.utah.edu/uofu/stu/ClassSchedules/main/1" + yearToDigit + semesterToDigit + "/seating_availability.html?subject=CS";
                _driver.Navigate().GoToUrl(url);

                var table = _driver.FindElementById("seatingAvailabilityTable");
                var body = table.FindElement(By.TagName("tbody"));
                var courses = body.FindElements(By.TagName("tr"));

                tempData = new List<List<string>>();

                foreach (var course in courses)
                {
                    //Course Enrollments
                    var courseElements = course.FindElements(By.TagName("td"));
                    var catalog = courseElements.ElementAt(2);
                    var subjet = courseElements.ElementAt(1);


                    var courseDepartment = subjet.FindElement(By.TagName("a")).Text;
                    var courseSection = courseElements.ElementAt(3).Text;
                    var catalogNumber = catalog.FindElement(By.TagName("a")).Text;
                    var courseName = course.FindElement(By.ClassName("tablesaw-priority-2")).Text;
                    var courseEnrolled = courseElements.ElementAt(7).Text;

                    //Only consider the 001 section of each course.
                    if (courseSection == "001")
                    {
                        //Only consider courses from 1000-7000
                        if (1000 <= Int32.Parse(catalogNumber) && Int32.Parse(catalogNumber) <= 7000)
                        {
                            //Do not consider any course which has a title with the words "Seminar" or "Special Topics" in it.
                            if (!courseName.Contains("Seminar") && !courseName.Contains("Special Topics"))
                            {
                                //check total Items number
                                if (tempData.Count == total_Items.Value)
                                {
                                    break;
                                }

                                var s = new List<string>
                                {
                                    courseDepartment.Trim(),
                                    catalogNumber.Trim(),
                                    courseName.Trim(),
                                    courseEnrolled.Trim(),
                                    semester.Trim(),
                                    year.Trim()
                                };

                                tempData.Add(s);

                            }
                        }
                    }
                    
                }

                goToCatalog();

                foreach (var item in tempData)
                {
                    
                    _driver.FindElementById("Search").SendKeys(item[0] + item[1]);
                    var courseUrls = _driver.FindElement(By.TagName("tbody"));
                    courseUrls.FindElement(By.TagName("a")).Click();

   
                    //discription and Credit
                    string creditNumber = "";
                    string discriptionWords = "";
                    var tempStore = _driver.FindElements(By.ClassName("noBreak"));
                    foreach (var a in tempStore)
                    {
                        if (a.FindElement(By.ClassName("_3qov3mur")).Text.Contains("Description"))
                        {
                            
                            var discript1 = a.FindElement(By.ClassName("iHFbKrta"));
                            discriptionWords = discript1.FindElement(By.TagName("div")).Text.Trim();

                        }
                        if (a.FindElement(By.ClassName("_3qov3mur")).Text.Contains("Credits"))
                        {
                            
                            var credit1 = a.FindElement(By.ClassName("iHFbKrta"));
                            creditNumber = credit1.FindElement(By.TagName("div")).Text.Trim();

                        }

                    }

                    var d = new Data
                    {
                        Department = item[0],
                        Number = item[1],
                        Credit = creditNumber,
                        Title = item[2],
                        Enrolled = item[3],
                        Semester = item[4],
                        Year = item[5],
                        Description = discriptionWords.Trim(),
                    };

                    //store data
                    dataGrid.Rows.Add(d.Department + " " + d.Number, d.Title, d.Credit, d.Enrolled, d.Semester, d.Year);
                    records.Add(d);



                    //goback to search bar
                    var goback = _driver.FindElement(By.ClassName("_93GXH6fZ"));
                    goback.FindElement(By.TagName("button")).Click();
                    
                }
                discriptionBtn.Enabled = true;
                Cursor.Current = Cursors.Default;
                saveBtn.Enabled = true;

            }
            catch(Exception)
            {
                displayTextBox.Text = "TIME OUT!!!";
                saveBtn.Enabled = false;
                button1.Enabled = true;
                discriptionBtn.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
            


            

        }

       /// <summary>
       /// Save dat as CSV file
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void saveBtn_Click(object sender, EventArgs e)
        {


            
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog() { Filter = "CSV|*.csv", ValidateNames = true })
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //Save datagridview to csv file
                    using (var sw = new StreamWriter(saveFileDialog1.FileName))
                    {
                        var writer = new CsvWriter(sw, CultureInfo.InvariantCulture);
                        writer.WriteRecords(records);
                        
                    }
                    MessageBox.Show("Your data has been successfully saved.", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        /// <summary>
        /// go to Catalog page
        /// </summary>
        private void goToCatalog()
        {
            _driver.Navigate().GoToUrl("https://catalog.utah.edu");
            var nav = _driver.FindElement(By.ClassName("c-MW5eJp"));
            var nav_elements = nav.FindElements(By.TagName("li"));
            nav_elements.ElementAt(2).Click();
        }

        /// <summary>
        /// Year input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void courseYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            year = (string)courseYear.SelectedItem;

            yearToDigit = year.Substring(2);

            
        }

        /// <summary>
        /// Search a course description by course number
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
          

            try
            {
                Cursor.Current = Cursors.WaitCursor;
                var input = inputCourseNumber.Text;
                discriptionBtn.Enabled = false;

                //Run a driver, if it ins't running.
                if (driverFlag == false)
                {
                    _driver = new ChromeDriver();
                    _driver.Manage().Window.Maximize();
                    _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

                    driverFlag = true;
                }

                button1.Enabled = false;
                goToCatalog();

                //Search course by course number
                _driver.FindElementById("Search").SendKeys("CS" + input);
                var courseUrls = _driver.FindElement(By.TagName("tbody"));
                courseUrls.FindElement(By.TagName("a")).Click();



                //discription
                var tempStore = _driver.FindElements(By.ClassName("noBreak"));
                foreach (var a in tempStore)
                {
                    if(a.FindElement(By.ClassName("_3qov3mur")).Text.Contains("Description"))
                    {
                        //displayTextBox.Text = a.FindElement(By.ClassName("_3qov3mur")).Text;
                        var discript1 = a.FindElement(By.ClassName("iHFbKrta"));
                        var discriptionwords = discript1.FindElement(By.TagName("div"));

                        displayTextBox.Text = discriptionwords.Text;

                    }
                }

               

                button1.Enabled = true;
                Cursor.Current = Cursors.Default;
                discriptionBtn.Enabled = true;

            }
            catch (Exception)
            {

                displayTextBox.Text = "TIME OUT!!!"; 
                button1.Enabled = true;
                saveBtn.Enabled = false;
                discriptionBtn.Enabled = true;
                Cursor.Current = Cursors.Default;

            }
          


        }

        /// <summary>
        /// Quit the program and close all windows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitBtn_Click(object sender, EventArgs e)
        {
            if(driverFlag == true)
            {
                _driver.Close();
                _driver.Quit();
                
                this.Close();
            } else
            {
                this.Close();
            }

        }

        /// <summary>
        /// input Semester
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void courseSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            semester = (string)courseSemester.SelectedItem;
            
            if(semester == "SPRING")
            {
                semesterToDigit = "4";
            } 
            else if(semester == "SUMMER")
            {
                semesterToDigit = "6";
            }
            else
            {
                semesterToDigit = "8";
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        
    }


    /// <summary>
    /// Data structure of Course enrollment.
    /// </summary>
    public class Data
    {
        public string Department { get; set; }
        public string Number { get; set; }
        public string Credit { get; set; }
        public string Title { get; set; }
        public string Enrolled { get; set; }
        public string Semester { get; set; }
        public string Year { get; set; }
        public string Description { get; set; }
    }
}
