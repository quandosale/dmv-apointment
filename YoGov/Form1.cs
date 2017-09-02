using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Mail;

namespace YoGov
{
    public partial class Form1 : Form
    {
        private const string statusSeperator = "----------------------------------------------------------------";
        private const string URL = "https://www.dmv.ca.gov/wasapp/foa/clear.do?goTo=driveTest&localeName=en";
        private ChromeUtil mChrome;

        private Dictionary<string, string> mResult = new Dictionary<string, string>();

        private Thread mainProcessingThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void Run()
        {
            UpdateStatus("Start Processing...");
            mChrome = new ChromeUtil();

            while(true)
            {
                string messageBody = "";
                for (int i = 0; i < lstLocation.Items.Count; i++)
                {
                    UpdateStatus(statusSeperator);
                    mChrome.GoToUrl(URL);

                    UpdateStatus("Checking Availability in " + lstLocation.Items[i].ToString());
                    Thread.Sleep(3000);

                    string locationOptions = mChrome.FindById("officeId").GetAttribute("innerHTML");
                    int locationIndex = -1;
                    int locationCount = Regex.Matches(locationOptions, "<option").Count - 1;

                    for (int j = 0; j < locationCount; j++)
                    {
                        string xPath = "//select[@id='officeId']/option[" + (j + 1).ToString() + "]";
                        string _location = mChrome.FindByXPath(xPath).Text;

                        if (_location == lstLocation.Items[i].ToString())
                        {
                            locationIndex = j;
                        }
                    }

                    if (locationIndex == -1) continue;
                    mChrome.SelectOptionByIndex("//select[@id='officeId']", locationIndex);

                    mChrome.SetTextByID("first_name", txtFirstName.Text);
                    mChrome.SetTextByID("last_name", txtLastName.Text);
                    mChrome.SetTextByID("dl_number", txtPermitNo.Text);
                    mChrome.SetTextByName("birthMonth", txtBirthMon.Text);
                    mChrome.SetTextByName("birthDay", txtBirthDay.Text);
                    mChrome.SetTextByName("birthYear", txtBirthYear.Text);
                    mChrome.SetTextByName("telArea", txtPhoneArea.Text);
                    mChrome.SetTextByName("telPrefix", txtPhonePrefix.Text);
                    mChrome.SetTextByName("telSuffix", txtPhoneSuffix.Text);
                    mChrome.FindById("DT").Click();

                    mChrome.FindByAttr("input", "type", "submit", 1).Click();

                    try
                    {
                        string location = mChrome.FindByXPath("//td[@data-title='Office']/p").Text;
                        string datetime = mChrome.FindByXPath("//td[@data-title='Appointment']/p[2]/strong").Text;

                        string value;
                        bool isNew = false;
                        if (mResult.TryGetValue(location, out value))
                        {
                            if (value != datetime)
                            {
                                mResult[location] = value;
                                isNew = true;
                            }
                        }
                        else
                        {
                            mResult.Add(location, datetime);
                            isNew = true;
                        }
                        if(isNew)
                        {
                            UpdateStatus("Location: \n" + location);
                            UpdateStatus("DateTime: \n" + datetime);
                            messageBody += "<strong>Location: </strong>" + location + "<br>" + "<strong>DateTime: </strong>" + datetime + "<br><br>";
                        }
                    }
                    catch
                    {

                    }
                }
                if (messageBody != "")
                    alertNewEntry(messageBody);
                Thread.Sleep(Convert.ToInt32(checkInterval.Value) * 60000);
            }
        }

        private void btnAddLocation_Click(object sender, EventArgs e)
        {
            string newLocation = cmbLocation.SelectedItem.ToString();
            if(lstLocation.FindString(newLocation) == -1)
                lstLocation.Items.Add(cmbLocation.SelectedItem.ToString());
        }

        private void delAddLocation_Click(object sender, EventArgs e)
        {
            int selectedIndex = lstLocation.SelectedIndex;
            if(selectedIndex != -1)
                lstLocation.Items.RemoveAt(lstLocation.SelectedIndex);
        }

        private void alertNewEntry(string message)
        {
            UpdateStatus(statusSeperator);
            UpdateStatus("New Entry Found:");

            try
            {
                SmtpClient client = new SmtpClient();
                client.Port = 465;
                client.Host = "smtp.mail.yahoo.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential("wbitsale68@yahoo.com", "thisisme!Q@W#E");

                MailMessage mm = new MailMessage("donotreply@yogov.org", txtEmail.Text, "New Entries Found", message);
                mm.IsBodyHtml = true;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.Send(mm);
            } catch (Exception ex)
            {
                UpdateStatus(ex.ToString());
            }
        } 

        private void btnStart_Click(object sender, EventArgs e)
        {
            if(MyUtil.CheckAvailability("yogov"))
            {
                mainProcessingThread = new Thread(new ThreadStart(Run));
                mainProcessingThread.Start();
            }
        }

        private void UpdateStatus(String status)
        {
            try
            {
                txtLog.Text += status + "\n";
            }
            catch
            {

            }
        }

        private void statusChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void Stop()
        {
            if (mainProcessingThread != null) mainProcessingThread.Abort();
            if (mChrome != null) mChrome.Quit();
        }

        private void onClose(object sender, FormClosedEventArgs e)
        {
            Stop();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            UpdateStatus(statusSeperator);
            UpdateStatus("Stopped...");
            Stop();
        }
    }
}
