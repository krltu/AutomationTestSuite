using NUnit.Framework;
using NUnit.Framework.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V135.Page;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Security.Claims;
using System.Xml.Linq;

namespace AutomationTestSuite
{
    [TestFixture]
    public class FrontendTests
    {
        private ChromeDriver driver;
        private WebDriverWait wait;
        private readonly string baseUrl = "http://way2automation.com/way2auto_jquery/automation-practice-site.html";

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl(baseUrl);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [Test]
        public void InputAlertTest()
        {
            string name = "John Doe";

            //Scroll to bottom
            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");

            //Click on the alert card
            var alertCard = wait.Until(d =>
            {
                var el = d.FindElement(By.XPath("//a[@href='alert.php']/h2[text()='Alert']/parent::a"));
                return (el.Displayed && el.Enabled) ? el : null;
            });
            alertCard.Click();

            //Wait for the alert tab to open and switch to it
            wait.Until(d => d.WindowHandles.Count > 1);
            var newTab = driver.WindowHandles.Last();
            driver.SwitchTo().Window(newTab);

            //Wait for the input alert tab to be available and switch to it
            var inputAlertTab = wait.Until(d =>
            {
                var el = d.FindElement(By.XPath("//ul[@class='responsive-tabs']//a[contains(text(),'Input Alert')]"));
                return (el.Displayed && el.Enabled) ? el : null;
            });
            inputAlertTab.Click();


            // Switch to the correct frame (INPUT ALERT section)
            wait.Until(d => d.FindElements(By.TagName("iframe")).Count > 0);
            var iframe = wait.Until(d => d.FindElement(By.XPath("//iframe[contains(@src,'input-alert')]")));
            driver.SwitchTo().Frame(iframe);
            //driver.SwitchTo().Frame(1); //this works as well without the iframe

            // Wait for and click the input alert button
            var inputButton = wait.Until(d =>
            {
                var el = d.FindElement(By.XPath("//button[contains(text(),'Input box.')]"));
                //var el = d.FindElement(By.XPath("//button[text()='Click the button to demonstrate the Input box.']"));
                return (el.Displayed && el.Enabled) ? el : null;
            });
            inputButton.Click();

            // Handle input alert
            wait.Until(d => d.SwitchTo().Alert() != null);
            var inputAlert = driver.SwitchTo().Alert();
            inputAlert.SendKeys(name);
            inputAlert.Accept();

            // Wait for the result message to appear and display it
            var result = wait.Until(d =>
            {
                var el = d.FindElement(By.Id("demo"));
                return !string.IsNullOrEmpty(el.Text) ? el : null;
            });

            //name = "Silver Star"; //to test a potential fail

            Console.WriteLine("The resulting alert text is: '" + result.Text + "'\n");
            
            //Checking if the message contains the expected name
            if (result.Text.Contains(name))
            {
                Console.WriteLine("The confirmation message contains the expected name: " + name + "\n");
            }

            Assert.That(result.Text, Does.Contain(name), "Error.The confirmation message does not contain the input name.\n");
        }

        [Test]
        public void DatePickerFormatTest()
        {
            //Click on the date picker card
            var datePickerCard = driver.FindElement(By.XPath("//a[@href='datepicker.php']/h2[text()='Datepicker']/parent::a"));
            datePickerCard.Click();

            //Wait for the date picker tab to open and switch to it
            wait.Until(d => d.WindowHandles.Count > 1);
            var newTab = driver.WindowHandles.Last();
            driver.SwitchTo().Window(newTab);

            //Wait for the input alert tab to be available and switch to it
            var formatDateTab = wait.Until(d =>
            {
                var el = d.FindElement(By.XPath("//ul[@class='responsive-tabs']//a[contains(text(),'Format date')]"));
                return (el.Displayed && el.Enabled) ? el : null;
            });
            formatDateTab.Click();

            // Switch to the correct frame (INPUT ALERT section)
            wait.Until(d => d.FindElements(By.TagName("iframe")).Count > 0);
            var iframe = wait.Until(d => d.FindElement(By.XPath("//iframe[contains(@src,'atepicker/defult4')]")));
            driver.SwitchTo().Frame(iframe);

            //Select today from the date input edit field (via dropdown, highlighted value)
            var dateInput = wait.Until(driver =>
            {
                var element = driver.FindElement(By.Id("datepicker"));
                return (element != null && element.Displayed && element.Enabled) ? element : null;
            });
            dateInput.Click();

            var todayElement = wait.Until(driver =>
            {
                var element = driver.FindElement(By.CssSelector(".ui-datepicker-calendar .ui-state-highlight"));
                return (element != null && element.Displayed && element.Enabled) ? element : null;
            });
            todayElement.Click();

            //Pick the desired format(in this case, ISO 8601) from the format options dropdown
            var formatDropdown = wait.Until(driver =>
            {
                var element = driver.FindElement(By.Id("format"));
                return (element != null && element.Displayed && element.Enabled) ? element : null;
            });

            var selectElement = new SelectElement(formatDropdown);
            selectElement.SelectByText("ISO 8601 - yy-mm-dd");

            wait.Until(driver =>
            {
                var value = driver.FindElement(By.Id("datepicker")).GetAttribute("value");
                return !string.IsNullOrWhiteSpace(value);
            });

            //Check if the selected date matches the expected date
            string selectedDate = driver.FindElement(By.Id("datepicker")).GetAttribute("value");
            //string selectedDate = "2020-08-01"; //to test a potential fail
            string expectedDate = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            
            if (selectedDate == expectedDate)
            {
                Console.WriteLine("The selected date matches the expected one: " + expectedDate + "\n");
            }

            Assert.That(selectedDate, Is.EqualTo(expectedDate), $"Date mismatch\n");
        }

        [TearDown]
        public void TearDown()
        {
            driver.Quit();
        }
    }
}


