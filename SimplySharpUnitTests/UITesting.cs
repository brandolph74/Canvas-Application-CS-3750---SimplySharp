using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using System;
using NUnit.Framework;
using OpenQA.Selenium.Firefox;

namespace SimplySharpUnitTests
{
    public class UITest
    {
        string ourSimplySharpOnAzureURL = "https://lmssimplysharp.azurewebsites.net/";
        IWebDriver driver;
        WebDriverWait wait;

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver(Environment.CurrentDirectory);
            driver.Manage().Window.Maximize();
            driver.Url = ourSimplySharpOnAzureURL; //set the url to the website
        }
        /// <summary>
        /// Tests successful log in and ensures the user is logged in correctly
        /// </summary>
        [Test]
        public void UITestSuccesfulLogin()
        {
            wait = new(driver, TimeSpan.FromSeconds(10));

            //Find email input element
            IWebElement enterEmail = wait.Until(d => d.FindElement(By.Id("Input_Email")));
            string expectedUserName = "bwykt@email.com";  // A test userName.

            //Send test user name to UI
            enterEmail.SendKeys(expectedUserName);

            System.Threading.Thread.Sleep(1000);

            IWebElement enterPassword = driver.FindElement(By.Id("Input_Password"));
            enterPassword.SendKeys("Password");
            System.Threading.Thread.Sleep(2000);

            //Find the Log in button and click it
            driver.FindElement(By.XPath("//button[.='Log in']")).Click();
            System.Threading.Thread.Sleep(2000);

            //Find the account management page link and click it
            driver.FindElement(By.PartialLinkText("Hello,")).Click();
            System.Threading.Thread.Sleep(2000);

            //Find the user name on account management page and get its value
            IWebElement UserName = wait.Until(d => d.FindElement(By.Id("Username")));
            string actualUserName = UserName.GetAttribute("value");
            System.Threading.Thread.Sleep(2000);

            //Compare to ensure the user is logged in correctly
            Assert.IsTrue(expectedUserName == actualUserName);
        }

        /// <summary>
        /// Tests three different failed log in attempts.
        /// </summary>
        [Test]
        public void UITestFailedLogin()
        {
            //Attempt to log in with an unregistered user
            wait = new(driver, TimeSpan.FromSeconds(10));
            IWebElement enterWrongEmail = wait.Until(d => d.FindElement(By.Id("Input_Email")));
            string userName = "unregisteredUser@email.com";
            enterWrongEmail.SendKeys(userName);
            System.Threading.Thread.Sleep(1000);
            IWebElement enterCorrectPassword = driver.FindElement(By.Id("Input_Password"));
            enterCorrectPassword.SendKeys("Password");
            System.Threading.Thread.Sleep(2000);
            driver.FindElement(By.XPath("//button[.='Log in']")).Click();
            System.Threading.Thread.Sleep(2000);

            //"Refresh" and attempt to log in with incorrect password
            driver.Url = ourSimplySharpOnAzureURL;
            System.Threading.Thread.Sleep(2000);
            IWebElement enterCorrectEmail = wait.Until(d => d.FindElement(By.Id("Input_Email")));
            userName = "bwykt@email.com";
            enterCorrectEmail.SendKeys(userName);
            System.Threading.Thread.Sleep(1000);
            IWebElement enterWrongPassword = driver.FindElement(By.Id("Input_Password"));
            enterWrongPassword.SendKeys("IncorrectPassword");
            System.Threading.Thread.Sleep(2000);
            driver.FindElement(By.XPath("//button[.='Log in']")).Click();
            System.Threading.Thread.Sleep(2000);

            //"Refresh" and attempt to log in with blank fields
            driver.Url = ourSimplySharpOnAzureURL;
            System.Threading.Thread.Sleep(2000);
            driver.FindElement(By.XPath("//button[.='Log in']")).Click();
        }

        [Test]
        public void UITestInstructorCanAddClass()
        {
            System.Threading.Thread.Sleep(3000); // Sleep for 3 secs so we can see the class before deleting
            //Log in as an instructor
            wait = new(driver, TimeSpan.FromSeconds(10));
            IWebElement enterEmail = wait.Until(d => d.FindElement(By.Id("Input_Email")));
            enterEmail.SendKeys("bwykt@email.com");
            IWebElement enterPassword = driver.FindElement(By.Id("Input_Password"));
            enterPassword.SendKeys("Password");
            driver.FindElement(By.XPath("//button[.='Log in']")).Click();

            // Go to Courses page to add a new CS 9999 class
            wait.Until(d => d.FindElement(By.LinkText("Courses"))).Click();
            driver.FindElement(By.LinkText("Add New Class")).Click();

            // Fill out the relevant info and add the class
            IWebElement department = wait.Until(d => d.FindElement(By.Id("Department")));
            department.SendKeys("Computer Science");
            IWebElement classId = driver.FindElement(By.Id("ClassId"));
            classId.SendKeys("CS 9999");
            IWebElement className = driver.FindElement(By.Id("ClassName"));
            className.SendKeys("Diliya UI Test");
            IWebElement credits = driver.FindElement(By.Id("Credits"));
            credits.SendKeys("4");
            IWebElement capacity = driver.FindElement(By.Id("Capacity"));
            capacity.SendKeys("30");
            IWebElement location = driver.FindElement(By.Id("Location"));
            location.SendKeys("WSU Main C106");
            driver.FindElement(By.Id("2")).Click(); // for Meeting Days select Tuesday
            driver.FindElement(By.Id("4")).Click(); // and Thursday
            IWebElement startTime = driver.FindElement(By.Id("StartTime"));
            startTime.SendKeys("05:30PM");
            IWebElement endTime = driver.FindElement(By.Id("EndTime"));
            endTime.SendKeys("07:20PM");
            driver.FindElement(By.CssSelector("input[type='submit']")).Click();
            System.Threading.Thread.Sleep(3000); // Sleep for 3 secs so we can see the class before deleting

            //Delete the newly added class by finding its row in a table of Classes and clicking Delete link at the end of the row:
            var table = wait.Until(d => d.FindElement(By.TagName("table")));
            var rows = table.FindElements(By.TagName("tr"));
            foreach (var row in rows)
            {
                if (row.Text.Substring(0, 7) == "CS 9999")
                {
                    var rowTds = row.FindElements(By.TagName("td"));
                    if (rowTds[8].Text == "Delete")
                    {
                        rowTds[8].Click(); // Delete is the 9th element in the table, but it is 0 based

                        //Now we have left Index.cshtml view for Classes and went to Delete.cshtml
                        // click the Delete button (it's the input type submit)
                        wait.Until(d => d.FindElement(By.CssSelector("input[type='submit']"))).Click();
                    }
                }
            }
        }


        [Test]
        public void UITestStudentCanRegister()
        {
            System.Threading.Thread.Sleep(3000); // Sleep 

            //Log in as a Student 
            wait = new(driver, TimeSpan.FromSeconds(10));
            IWebElement enterEmail = wait.Until(d => d.FindElement(By.Id("Input_Email")));
            enterEmail.SendKeys("teststudent@mail.com");
            IWebElement enterPassword = driver.FindElement(By.Id("Input_Password"));
            enterPassword.SendKeys("1234Pass");
            driver.FindElement(By.XPath("//button[.='Log in']")).Click();

            //Go to Registration Page 
            wait.Until(d => d.FindElement(By.LinkText("Register"))).Click();

            //Click Register for the first class in the list
            IWebElement RegisterButton = driver.FindElement(By.XPath("//table[@class='table table-hover']//tbody//tr//td//a[@class ='btn btn-primary']"));
            RegisterButton.Click();

            System.Threading.Thread.Sleep(1000); // Sleep 
            //Agree to class registration 
            driver.FindElement(By.ClassName("btn-success")).Click();
            System.Threading.Thread.Sleep(1000); // Sleep 

            //Go back to Dashboard then back to registration
            wait.Until(d => d.FindElement(By.LinkText("Dashboard"))).Click();
            System.Threading.Thread.Sleep(1000); // Sleep 
            wait.Until(d => d.FindElement(By.LinkText("Register"))).Click();

            //Drop class
            IWebElement RegisterButton2 = driver.FindElement(By.XPath("//table[@class='table table-hover']//tbody//tr//td//a[@class ='btn btn-primary']"));
            RegisterButton2.Click();
            System.Threading.Thread.Sleep(2000); // Sleep 
            driver.FindElement(By.ClassName("btn-danger")).Click();

            //And show dashboard one last time
            wait.Until(d => d.FindElement(By.LinkText("Dashboard"))).Click();

        }
    }
}
