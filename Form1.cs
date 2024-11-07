using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using sunucukontrol.Helpers;

namespace sunucukontrol
{
    public partial class Form1 : Form
    {
        private bool running = false;
        private Dictionary<string, bool> serverStatuses = new Dictionary<string, bool>();
        private Dictionary<string, long> serverPingTimes = new Dictionary<string, long>();
        // private DateTime lastOfflineTime = DateTime.Now;
        private Thread pingThread;
        private const int maxOfflineDurationMinutes = 5;
        private const int maxOfflineDurationMinutesLevel3 = 55;
        private string smtpServer = "smtp.office365.com";
        private int smtpPort = 587;
        private string senderEmail = "-";
        private string senderPassword = "-";
        private string recipientEmail = "-";
        private Dictionary<string, TextBox> ipTextBoxes = new Dictionary<string, TextBox>();
        private Dictionary<string, TextBox> descriptionTextBoxes = new Dictionary<string, TextBox>();
        private Dictionary<string, DateTime> lastEmailSentTimes = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> lastOfflineTime = new Dictionary<string, DateTime>();
        private Dictionary<string, TimeSpan> serverOfflineDurations = new Dictionary<string, TimeSpan>();
        private Dictionary<string, bool> serverspecial = new Dictionary<string, bool>();
        public Form1()
        {
            InitializeComponent();
            InitializeIpTextBoxes();
        }

        private void InitializeIpTextBoxes()
        {
            int maxTextBoxes = 15;
            for (int i = 1; i <= maxTextBoxes; i++)
            {
                string textBoxName = $"textBox{i}";
                ipTextBoxes.Add(textBoxName, Controls.Find(textBoxName, true).FirstOrDefault() as TextBox);

                string descriptionTextBoxName = $"textBoxDescription{i}";
                descriptionTextBoxes.Add(textBoxName, Controls.Find(descriptionTextBoxName, true).FirstOrDefault() as TextBox);

                ipTextBoxes[textBoxName].TextChanged += IpAddressTextChanged;
            }
        }

        private void IpAddressTextChanged(object sender, EventArgs e)
        {
            TextBox ipTextBox = (TextBox)sender;
            TextBox descriptionTextBox = descriptionTextBoxes[ipTextBox.Name];

            string ipAddress = ipTextBox.Text;
            string description = descriptionTextBox.Text;

            if (!IsValidIpAddress(ipAddress))
            {
                LogToConsole($"Ge�ersiz IP Adresi: {ipAddress}");
                return;
            }

            if (!serverStatuses.ContainsKey(ipAddress))
            {
                serverStatuses[ipAddress] = true;
                serverPingTimes[ipAddress] = 0;
                serverspecial[ipAddress] = true;
            }
        }

        private bool IsValidIpAddress(string ipAddress)
        {
            if (System.Net.IPAddress.TryParse(ipAddress, out System.Net.IPAddress parsedIpAddress))
            {
                return true;
            }
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FileHelper.LoadFromFile(ipTextBoxes, descriptionTextBoxes);

            labelStatus.Text = "Haz�r.";
            richTextBoxConsole.Text = "";
            EnableIpTextBoxes();

            foreach (var ipAddressEntry in ipTextBoxes)
            {
                serverStatuses[ipAddressEntry.Value.Text] = true;
                serverPingTimes[ipAddressEntry.Value.Text] = 0;
                serverspecial[ipAddressEntry.Value.Text] = true;
            }
        }
        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (!running)
            {
                running = true;
                pingThread = new Thread(new ThreadStart(PingServers));
                pingThread.Start();
                labelStatus.Text = "�al���yor...";
                DisableIpTextBoxes();
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            running = false;
            labelStatus.Text = "Durduruldu.";
            EnableIpTextBoxes();
        }

        private void DisableIpTextBoxes()
        {
            foreach (var textBox in ipTextBoxes.Values)
            {
                textBox.Enabled = false;
            }
        }

        private void EnableIpTextBoxes()
        {
            foreach (var textBox in ipTextBoxes.Values)
            {
                textBox.Enabled = true;
            }
        }
        private async void PingServers()
        {
            while (running)
            {
                foreach (var ipAddressEntry in ipTextBoxes)
                {
                    string ipAddress = ipAddressEntry.Value.Text;
                    TextBox descriptionTextBox = descriptionTextBoxes[ipAddressEntry.Key];
                    string description = descriptionTextBox.Text;

                    if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(description))
                    {
                        continue;
                    }

                    if (!IsValidIpAddress(ipAddress))
                    {
                        LogToConsole($"Ge�ersiz IP Adresi: {ipAddress}");
                        continue;
                    }

                    long pingTime = await PingIpAddressInternal(ipAddress, description);
                    LogToConsole($"Sunucu ({description}, {ipAddress}) P�NG {pingTime}.");
                    if (pingTime == -1)
                    {
                        if (serverStatuses[ipAddress])
                        {
                            serverspecial[ipAddress] = false;
                            await SendEmail(ipAddress, description, true);
                            serverStatuses[ipAddress] = false;
                            lastOfflineTime[ipAddress] = DateTime.Now;
                            LogToConsole($"Sunucu ({description}, {ipAddress}) �evrimd��� durumda.");
                            LogToConsole($"({lastOfflineTime[ipAddress]}, {ipAddress})");
                        }
                        else
                        {
                            serverspecial[ipAddress] = false;
                            if (!serverOfflineDurations.ContainsKey(ipAddress))
                            {
                                serverOfflineDurations[ipAddress] = TimeSpan.Zero;
                            }

                            DateTime now = DateTime.Now;
                            TimeSpan offlineDuration = now - lastOfflineTime[ipAddress];
                            serverOfflineDurations[ipAddress] = offlineDuration;
                            LogToConsole($"({ipAddress}, {serverOfflineDurations[ipAddress].TotalMinutes}, ping: {pingTime})11");
                            if (serverOfflineDurations[ipAddress].TotalMinutes >= maxOfflineDurationMinutes && serverOfflineDurations[ipAddress].TotalMinutes < maxOfflineDurationMinutesLevel3)
                            {
                                if (!lastEmailSentTimes.ContainsKey(ipAddress) || (now - lastEmailSentTimes[ipAddress]).TotalMinutes >= maxOfflineDurationMinutesLevel3)
                                {
                                    await SendEmail(ipAddress, description, false, offlineDuration, pingTime);
                                    lastEmailSentTimes[ipAddress] = now;
                                }
                                LogToConsole($"({ipAddress}, {serverOfflineDurations[ipAddress].TotalMinutes})22");
                                LogToConsole($"Sunucu ({description}, {ipAddress}) uzun s�redir �evrimd��� durumda. Ping S�resi: {pingTime} ms");
                            }
                            else if (serverOfflineDurations[ipAddress].TotalMinutes >= maxOfflineDurationMinutesLevel3)
                            {
                                if (!lastEmailSentTimes.ContainsKey(ipAddress) || (now - lastEmailSentTimes[ipAddress]).TotalMinutes >= maxOfflineDurationMinutesLevel3)
                                {
                                    LogToConsole($"({ipAddress}, {serverOfflineDurations[ipAddress].TotalMinutes})33");
                                    await SendEmail(ipAddress, description, false, offlineDuration, pingTime);
                                    lastEmailSentTimes[ipAddress] = now;
                                    LogToConsole($"Sunucu ({description}, {ipAddress}) �ok uzun s�redir �evrimd��� durumda. Ping S�resi: {pingTime} ms");
                                    LogToConsole($"({lastOfflineTime[ipAddress]})");
                                }
                            }
                        }
                    }
                    else
                    {
                        LogToConsole("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        if (!serverStatuses[ipAddress])
                        {
                            await SendEmail(ipAddress, description, false, null, pingTime, true);
                            serverStatuses[ipAddress] = true; // Sunucu tekrar �evrimi�i oldu�unda giri�i kald�r
                            lastOfflineTime.Remove(ipAddress);
                            serverspecial[ipAddress] = true;
                            LogToConsole($"Sunucu ({ipAddress}) tekrar �evrimi�i durumda. Ping S�resi: {pingTime} ms");
                        }

                        // Sunucu �evrimi�i oldu�unda, s�zl�kten giri�i kald�r.
                        if (serverOfflineDurations.ContainsKey(ipAddress))
                        {
                            serverOfflineDurations.Remove(ipAddress);
                        }
                    }
                }

                LogToConsole("Sunucular kontrol edildi.");
                await Task.Delay(1000);
            }
        }
        private async Task<long> PingIpAddressInternal(string ipAddress, string description)
        {
            long pingTime = -1;
            try
            {
                Ping ping = new Ping();
                DateTime offlineStartTime = DateTime.Now;
                PingReply reply2 = await ping.SendPingAsync(ipAddress, 2000);
                if (reply2 != null && reply2.Status == IPStatus.Success)
                {
                    pingTime = reply2.RoundtripTime;
                    serverspecial[ipAddress] = true;
                    LogToConsole($"2 Kontrol! Statuses:{serverStatuses[ipAddress]}, {ipAddress}, ping:{pingTime}");
                    return pingTime;
                }
                while (serverspecial[ipAddress] == true && pingTime == -1 && (DateTime.Now - offlineStartTime).TotalSeconds < 30)
                {
                    LogToConsole($"�zel Kontrol! Statuses:{serverStatuses[ipAddress]}, {ipAddress}");
                    PingReply reply = await ping.SendPingAsync(ipAddress, 2000);
                    if (reply != null && reply.Status == IPStatus.Success)
                    {
                        pingTime = reply.RoundtripTime;
                        LogToConsole($"2 Kontrol! Statuses:{serverStatuses[ipAddress]}, {ipAddress}, ping:{pingTime}");
                        return pingTime;
                    }
                    await Task.Delay(1000);
                }
            }
            catch (PingException)
            {

            }
            return pingTime;
        }
        private async Task SendEmail(string ipAddress, string description, bool isFirstLevel, TimeSpan? offlineDuration = null, long pingTime = 0, bool isServerOnlineNotification = false)
        {
            try
            {
                string emailSubject = "";
                string emailBody = "";

                if (isFirstLevel)
                {
                    emailSubject = $"{description} �evrimd��� Durumda";
                    emailBody = $"Sunucu ({description}, {ipAddress}) �evrimd��� durumda (SEV�YE 1).Sunucuya 60 saniye boyunca ping at�ld� fakat cevap al�namad�. Bu bir anl�k kesinti de�ildir! Tarih: {DateTime.Now}\n";
                }
                else
                {
                    if (isServerOnlineNotification)
                    {
                        emailSubject = $"{description} Tekrar �evrimi�i Durumda";
                        emailBody = $"Sunucu ({description}, {ipAddress}) tekrar �evrimi�i durumda. Tarih: {DateTime.Now}\n";
                    }
                    else
                    {
                        if (offlineDuration != null && offlineDuration.Value.TotalMinutes >= maxOfflineDurationMinutesLevel3)
                        {
                            emailSubject = $"{description} KR�T�K HATA SEV�YE 3";
                            emailBody = $"Sunucu ({description}, {ipAddress}) �ok uzun s�redir �evrimd��� durumda (SEV�YE 3).Bu ciddi bir ar�zad�r. L�tfen sunucunun POWER(var ise UPS) veya ETHERNET ba�lant�lar�n� fiziksel olarak kontrol ediniz! Tarih: {DateTime.Now}\n";
                        }
                        else
                        {
                            emailSubject = $"{description}HATA SEV�YE 2";
                            emailBody = $"Sunucu ({description}, {ipAddress}) uzun s�redir �evrimd��� durumda (SEV�YE 2). Sunucuya 5 dakikadir ula��lam�yor l�tfen gerekli kontrolleri yap�n�z!  Tarih: {DateTime.Now}\n";
                        }
                        if (pingTime >= 0)
                        {
                            emailBody += $"Ping S�resi: {pingTime} ms\n";
                        }
                    }
                }

                SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort);
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderEmail);
                mailMessage.To.Add(recipientEmail);
                // mailMessage.To.Add(new MailAddress("ahmet.ayardi@noil.com.tr"));
                // mailMessage.To.Add(new MailAddress("engin.aksu@noil.com.tr"));
                mailMessage.Subject = emailSubject;
                mailMessage.Body = emailBody;

                smtpClient.Send(mailMessage);

                if (isServerOnlineNotification)
                {
                    LogToConsole("Sunucu tekrar �evrimi�i durumda.");
                }
                else
                {
                    LogToConsole("E-posta g�nderildi.");
                }
            }
            catch (Exception ex)
            {
                LogToConsole("E-posta g�nderilirken bir hata olu�tu: " + ex.Message);
            }
        }

        private void LogToConsole(string message)
        {
            if (richTextBoxConsole.InvokeRequired)
            {
                richTextBoxConsole.Invoke(new MethodInvoker(delegate
                {
                    string timestamp = DateTime.Now.ToString("[HH:mm:ss] ");
                    richTextBoxConsole.AppendText(timestamp + message + Environment.NewLine);
                    richTextBoxConsole.ScrollToCaret();
                }));
            }
            else
            {
                string timestamp = DateTime.Now.ToString("[HH:mm:ss] ");
                richTextBoxConsole.AppendText(timestamp + message + Environment.NewLine);
                richTextBoxConsole.ScrollToCaret();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            FileHelper.SaveToFile(ipTextBoxes, descriptionTextBoxes);
        }
    }
}