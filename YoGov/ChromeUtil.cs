﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace YoGov
{
    class ChromeUtil
    {
        private ChromeDriverService mDriverService;
        private ChromeDriver mNavigator;

        public ChromeUtil()
        {
            mDriverService = ChromeDriverService.CreateDefaultService();
            mDriverService.HideCommandPromptWindow = true;
            ChromeOptions option = new ChromeOptions();
            option.AddArguments("disable-infobars");               //disable test automation message
            option.AddArguments("--disable-notifications");        //disable notifications
            option.AddArguments("--disable-web-security");         //disable save password windows
            option.AddUserProfilePreference("credentials_enable_service", false);
            option.AddUserProfilePreference("disable-popup-blocking", "true");
            //option.AddArgument("--window-position=-32000,-32000");
            mNavigator = new ChromeDriver(mDriverService, option);
        }

        public void GoToUrl(string url)
        {
            mNavigator.Navigate().GoToUrl(url);
        }

        public IWebElement FindById(string id)
        {
            try
            {
                return mNavigator.FindElement(By.Id(id));
            } catch
            {
                return null;
            }
        }

        public IWebElement FindByAttr(string tag, string attr, string value, int index)
        {
            return mNavigator.FindElement(By.XPath("//" + tag + "[@" + attr + "='" + value + "'][" + index + "]"));
        }

        public IWebElement FindByClassName(string tag, string className, int index)
        {
            return mNavigator.FindElement(By.XPath("//" + tag + "[@class" + "='" + className + "'][" + index + "]"));
        }

        public IWebElement FindByXPath(string path)
        {
            try
            {
                return mNavigator.FindElement(By.XPath(path));
            } catch
            {
                return null;
            }
        }

        public bool SelectOptionByIndex(string path, int index)
        {
            IWebElement ele = FindByXPath(path);
            if (ele != null)
            {
                SelectElement selectElement = new SelectElement(ele);
                selectElement.SelectByIndex(index);
                return true;
            }
            return false;
        }

        public bool SetTextByID(string id, string value)
        {
            try
            {
                string script = "document.getElementById('" + id + "').value=`" + value + "`";
                mNavigator.ExecuteScript(script);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SetTextByName(string name, string value)
        {
            try
            {
                string script = "document.getElementsByName('" + name + "')[0].value=`" + value + "`";
                mNavigator.ExecuteScript(script);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool TryCloseAlert()
        {
            try
            {
                IAlert confirmation = mNavigator.SwitchTo().Alert();
                confirmation.Dismiss();
                return true;
            } catch
            {
                return false;
            } 
        }

        public void Quit()
        {
            mNavigator.Quit();
        }
    }
}
