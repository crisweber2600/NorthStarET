//using Microsoft.AspNet.FileProviders;
//using Microsoft.Extensions.OptionsModel;
using EntityDto.DTO.Admin.Simple;
using SendGrid;
using SendGrid.Helpers.Mail;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NorthStar4.API.Infrastructure
{
    public static class EmailHandler
    {
        private const string EMAIL_HEADER = @"<html  dir = ltr>
                                             <head>
                                             </head>
                                             <body style='font-family:Segoe UI, Tahoma;'>
                                                <div style='width:600px;margin-left:auto;margin-right:auto;'>
                                                    <img src='[server]/assets/img/NorthStar_Logo.png' />

                                                    <div style='font-size:16px;margin-top:10px;margin-bottom:5px;font-weight:bold'>[subject]</div>
                                                    <div style='font-size:13px;margin-top:10px;margin-bottom:10px;'>Dear [fullname],</div>
                                                    <div style='font-size:12px;'>
                                                    [subjectdetails]
                                                    </div>
        
                                            ";
        private const string EMAIL_FOOTER = @"      <div style='font-size:8px;color:#666666;margin-top:15px;margin-bottom:10px'>© 2016-2017 North Star Educational Tools, LLC</div>
                                                </div>
                                             </body>
                                            </html>
                                            ";
        private const string EMAIL_NEW_USER_BODY = @"<span style='display:inline-block;font-size:14px;font-weight:bold;width:100px'>User Name:</span>
                                                    <span style='font-size:14px;'>[username]</span>
                                                    <div style='margin:1px'>&nbsp;</div>
                                                    <span style='display:inline-block;font-size:14px;font-weight:bold;width:100px'>Password:</span>
                                                    <span style='font-size:14px;'>[password]</span>";
        private const string EMAIL_NEW_USERNAME_BODY = @"<span style='display:inline-block;font-size:14px;font-weight:bold;width:150px;padding-top:20px;'>New User Name:</span>
                                                        <span style='font-size:14px;'>[username]</span>
                                                        ";
        private const string EMAIL_PASSWORD_RESET_LINK = @"<span style='display:inline-block;font-size:14px;font-weight:bold;'>Please click on the following link to reset your password:</span>
                                                            <div style='margin:1px'>&nbsp;</div>
                                                            <span style='font-size:14px;'>[link]</span>";

        private const string ROLLOVER_SUMMARY_MESSAGE = @"<div style='font-size:16px;margin-top:10px;margin-bottom:15px;'>[message]</div>";

        private const string TABLE_SUMMARY = @"<div style='font-size:16px;margin-top:10px;margin-bottom:5px;font-weight:bold'>[message]</div>";
        private const string TABLE_HEADER = @"<table style='width:100%;border-collapse: collapse;border-spacing: 0;line-height: 1.46153846;text-align: left;'>
                                                <thead>
                                                    <th style='padding: 2px 2px;padding: 2px 2px;vertical-align: middle;border-bottom: 2px solid #e9ecf0;'>[data]</th>
                                                </thead><tbody>";
        private const string TABLE_ROW = @"<tr><td style='vertical-align: middle;    border-top: 1px solid #e9ecf0;'>[data]</td>";
        private const string TABLE_END = "</tbody></table>";
        //

        //public EmailHandler(IOptions<AppSettings> appSettings)
        //{
        //    AppSettings = appSettings;
        //}
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //public static void SendPDFBatch(string username, string filename)
        //{

        //    try
        //    {
        //        StringBuilder emailText = new StringBuilder();
        //        string headerText = String.Empty;
        //        string footerText = String.Empty;
        //        string bodyText = String.Empty;

        //        headerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_HEADER));
        //        headerText = headerText.Replace("[server]", ConfigurationManager.AppSettings["siteUrlBase"]);
        //        headerText = headerText.Replace("[fullname]", username);
        //        headerText = headerText.Replace("[subject]", "Your North Star Print Batch File");
        //        headerText = headerText.Replace("[subjectdetails]",
        //                                        "The batch that you created in North Star is ready for viewing. If the file is less than 5MB, it is attached.");

        //        bodyText =
        //            String.Format(
        //                "Please click <a href='{0}/utilities/batch-printing.aspx'>here</a> to view all of your print batches.",
        //                ConfigurationManager.AppSettings["siteUrlBase"]);

        //        emailText.Append(headerText);
        //        emailText.Append(bodyText);
        //        emailText.Append(footerText);

        //        //SmtpClient client = new SmtpClient();
        //        var message = SendGrid.GenerateInstance();
        //        message.AddTo(username);
        //        //message.AddTo("northstar.shannon@gmail.com");
        //        message.From = new MailAddress("support@northstaret.net");
        //        message.Html = emailText.ToString();
        //        message.Subject = "Your North Star Print Batch File";

        //        FileInfo info = new FileInfo((GetPDFDirectory() + "\\" + filename));

        //        if (info.Length < 5242880)
        //        {
        //            message.AddAttachment(GetPDFDirectory() + "\\" + filename);
        //        }

        //        var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential("Hayaku77", "dammit77"),
        //                                                      "smtp.sendgrid.net", 25);
        //        transportInstance.Deliver(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        // log exception
        //        log.Error("Error occurred while sending PDF Batch email: " + ex.Message);
        //    }
        //}

        //public static void SendCSVBatch(string username, string filename)
        //{

        //    try
        //    {
        //        StringBuilder emailText = new StringBuilder();
        //        string headerText = String.Empty;
        //        string footerText = String.Empty;
        //        string bodyText = String.Empty;

        //        headerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_HEADER));
        //        headerText = headerText.Replace("[server]", ConfigurationManager.AppSettings["siteUrlBase"]);
        //        headerText = headerText.Replace("[fullname]", username);
        //        headerText = headerText.Replace("[subject]", "Your North Star Export Batch File");
        //        headerText = headerText.Replace("[subjectdetails]",
        //                                        "The batch that you created in North Star is ready for viewing. If the file is less than 5MB, it is attached.");

        //        bodyText =
        //            String.Format(
        //                "Please click <a href='{0}/utilities/data-export.aspx'>here</a> to view all of your export batches.",
        //                ConfigurationManager.AppSettings["siteUrlBase"]);

        //        emailText.Append(headerText);
        //        emailText.Append(bodyText);
        //        emailText.Append(footerText);

        //        //SmtpClient client = new SmtpClient();
        //        var message = SendGrid.GenerateInstance();
        //        message.AddTo(username);
        //        //message.AddTo("northstar.shannon@gmail.com");
        //        message.From = new MailAddress("support@northstaret.net");
        //        message.Html = emailText.ToString();
        //        message.Subject = "Your North Star Export Batch File";

        //        FileInfo info = new FileInfo((GetExportDirectory() + "\\" + filename));

        //        if (info.Length < 5242880)
        //        {
        //            message.AddAttachment(GetExportDirectory() + "\\" + filename);
        //        }

        //        var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential("Hayaku77", "dammit77"),
        //                                                      "smtp.sendgrid.net", 25);
        //        transportInstance.Deliver(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        // log exception
        //        log.Error("Error occurred while sending Export Batch email: " + ex.Message);
        //    }
        //}

        //public static void SendGenericEmail(string to, string toName, string ccAddress, string subject,
        //                                    string subjectDetail, string bodyText)
        //{
        //    try
        //    {
        //        StringBuilder emailText = new StringBuilder();
        //        string headerText = String.Empty;
        //        string footerText = String.Empty;

        //        headerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_HEADER));
        //        headerText = headerText.Replace("[server]", ConfigurationManager.AppSettings["siteUrlBase"]);
        //        headerText = headerText.Replace("[fullname]", toName);
        //        headerText = headerText.Replace("[subject]", subject);
        //        headerText = headerText.Replace("[subjectdetails]", subjectDetail);

        //        emailText.Append(headerText);
        //        emailText.Append(bodyText);
        //        emailText.Append(footerText);

        //        //SmtpClient client = new SmtpClient();
        //        var message = SendGrid.GenerateInstance();
        //        message.AddTo(to);
        //        //message.AddTo("northstar.shannon@gmail.com");
        //        message.From = new MailAddress("support@northstaret.net");
        //        message.Html = emailText.ToString();
        //        message.Subject = subject;
        //        message.AddCc(ccAddress);
        //        message.AddBcc("support@northstaret.net");

        //        var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential("Hayaku77", "dammit77"),
        //                                                      "smtp.sendgrid.net", 25);
        //        transportInstance.Deliver(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        // log exception
        //        log.Error("Error occurred while sending Generic email: " + ex.Message);
        //    }
        //}

        public static void SendUserPasswordEmail(string newPassword, string email, string username, string ccAddress,
                                                 string fullName, string rootUrl)
        {

            try
            {
                StringBuilder emailText = new StringBuilder();
                string headerText = String.Empty;
                string footerText = String.Empty;
                string bodyText = String.Empty;

                headerText = EMAIL_HEADER;
                headerText = headerText.Replace("[server]", rootUrl);
                headerText = headerText.Replace("[fullname]", fullName);
                headerText = headerText.Replace("[subject]", "New User Notification");
                headerText = headerText.Replace("[subjectdetails]",
                                                "A new user account was created for you in North Star Educational Tools.  Your login information is provided below.");

                bodyText = EMAIL_NEW_USER_BODY;
                bodyText = bodyText.Replace("[username]", username);
                bodyText = bodyText.Replace("[password]", newPassword);

                emailText.Append(headerText);
                emailText.Append(bodyText);
                emailText.Append(footerText);

                //SmtpClient client = new SmtpClient();
                //var message = new SendGridMessage();
                //message.AddTo(email);
                //message.AddTo("northstar.shannon@gmail.com");
                //message.From = new MailAddress("support@northstaret.net");
                //message.Html = emailText.ToString();
                //message.Subject = "New North Star User";
                // message.AddTo(ccAddress);  TODO: temporary fake CC below
                //message.AddTo(ccAddress);
                //message.EnableBcc("support@northstaret.net");

                //var credentials = new System.Net.NetworkCredential("Hayaku77", "dammit77");
                //var transportWeb = new Web(credentials);
                //transportWeb.Deliver(message);
                                
                // new sendgrid API
                var client = new SendGridAPIClient(ConfigurationManager.AppSettings["SendGridApiKey"]);
                var personalization = new Personalization();

                personalization.AddTo(new Email(username));
                personalization.AddTo(new Email(ccAddress));
                //Email sendTo = new Email("northstar.shannon@gmail.com," + ccAddress);
                Email from = new Email("support@northstaret.net");
                Content mailContent = new Content("text/html", emailText.ToString());
                var message = new Mail();
                message.Contents = new List<Content>() { mailContent };
                message.From = from;
                message.Subject = "New North Star User";
                message.AddPersonalization(personalization);
                
                dynamic response = client.client.mail.send.post(requestBody: message.Get()).GetAwaiter().GetResult();
                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var responseMsg = response.Body.ReadAsStringAsync().Result;
                    Log.Error($"Unable to send email: {responseMsg}");
                }
            }
            catch (Exception ex)
            {
                // log exception
                Log.Error("Error occurred while sending User Password email: " + ex.Message);
            }
        }



        public static void SendNewUsernameEmail(string email, string fullName, string rootUrl)
        {

            try
            {
                StringBuilder emailText = new StringBuilder();
                string headerText = String.Empty;
                string footerText = String.Empty;
                string bodyText = String.Empty;

                headerText = EMAIL_HEADER;
                headerText = headerText.Replace("[server]", rootUrl);
                headerText = headerText.Replace("[fullname]", fullName);
                headerText = headerText.Replace("[subject]", "New Username");
                headerText = headerText.Replace("[subjectdetails]",
                                                "Your username has been changed in North Star Educational Tools.  Your new username is provided below.");

                bodyText = EMAIL_NEW_USERNAME_BODY;
                bodyText = bodyText.Replace("[username]", email);

                emailText.Append(headerText);
                emailText.Append(bodyText);
                emailText.Append(footerText);

                //SmtpClient client = new SmtpClient();
                //var message = new SendGridMessage();
                ////message.AddTo(email);
                //message.AddTo("northstar.shannon@gmail.com");
                //message.From = new MailAddress("support@northstaret.net");
                //message.Html = emailText.ToString();
                //message.Subject = "North Star Username Change";
                //message.EnableBcc("support@northstaret.net");

                //var credentials = new System.Net.NetworkCredential("Hayaku77", "dammit77");
                //var transportWeb = new Web(credentials);
                //transportWeb.Deliver(message);


                // new sendgrid API
                var client = new SendGridAPIClient(ConfigurationManager.AppSettings["SendGridApiKey"]);
                var personalization = new Personalization();

                personalization.AddTo(new Email("northstar.shannon@gmail.com"));
                personalization.AddTo(new Email(email));
                Content mailContent = new Content("text/html", emailText.ToString());
                Email from = new Email("support@northstaret.net");
                var message = new Mail();
                message.Contents = new List<Content>() { mailContent };
                message.From = from;
                message.Subject = "North Star Username Change";
                message.AddPersonalization(personalization);

                dynamic response = client.client.mail.send.post(requestBody: message.Get()).GetAwaiter().GetResult();
                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var responseMsg = response.Body.ReadAsStringAsync().Result;
                    Log.Error($"Unable to send email: {responseMsg}");
                }
            }
            catch (Exception ex)
            {
                // log exception
                Log.Error("Error occurred while sending New Username email: " + ex.Message);
            }
        }

        public static void SendAutomatedRolloverEmail(string email, string rootUrl, AutomaticRolloverOutputDto_Log log)
        {

            try
            {
                var forceLoad = Boolean.Parse(ConfigurationManager.AppSettings["ForceLoad"]);
                var hardStopCount = forceLoad ? 1000 : 20;
                var duplicateCount = forceLoad ? 1000 : 100;
                var integrityCount = forceLoad ? 1000 : 50;

                StringBuilder emailText = new StringBuilder();
                string headerText = String.Empty;
                string footerText = String.Empty;
                string bodyText = String.Empty;

                headerText = EMAIL_HEADER;
                headerText = headerText.Replace("[server]", rootUrl);
                headerText = headerText.Replace("[fullname]", "User");
                headerText = headerText.Replace("[subject]", "Automated Rollover Status");
                headerText = headerText.Replace("[subjectdetails]",
                                                "Your rollover file has been processed.  The status of the rollover appears below.");

                // overall message
                bodyText = ROLLOVER_SUMMARY_MESSAGE.Replace("[message]", log.OverallStatusMessage);

                // see if there are hard stop errors
                if(log.HardStopErrors.Count > 0)
                {
                    bodyText += TABLE_SUMMARY.Replace("[message]", "Errors that prevented the rollover from completing. (only first " + hardStopCount +  " errors shown)");
                    bodyText += TABLE_HEADER.Replace("[data]", "Error");

                    for(var i = 0; i < hardStopCount && i < log.HardStopErrors.Count; i++)
                    {
                        bodyText += TABLE_ROW.Replace("[data]", log.HardStopErrors[i]);
                    }
                    bodyText += TABLE_END;
                }

                // integrity errors
                if (log.IntegrityErrors.Count > 0)
                {
                    bodyText += TABLE_SUMMARY.Replace("[message]", "The following errors are records that have invalid data.  They have been skipped and not imported.  As long as there are fewer than " + integrityCount + ", the rollover will be processed.(only first " + integrityCount + " errors shown)");
                    bodyText += TABLE_HEADER.Replace("[data]", "Error");

                    for (var i = 0; i < integrityCount && i < log.IntegrityErrors.Count; i++)
                    {
                        bodyText += TABLE_ROW.Replace("[data]", log.IntegrityErrors[i]);
                    }
                    bodyText += TABLE_END;
                }


                // duplication errors
                if (log.DuplicationErrors.Count > 0)
                {
                    bodyText += TABLE_SUMMARY.Replace("[message]", "The following errors may lead to duplication of either teacher or student data.  As long as there are fewer than " + duplicateCount + ", the rollover will be processed.(only first " + duplicateCount + " errors shown)");
                    bodyText += TABLE_HEADER.Replace("[data]", "Error");

                    for (var i = 0; i < duplicateCount && i < log.DuplicationErrors.Count; i++)
                    {
                        bodyText += TABLE_ROW.Replace("[data]", log.DuplicationErrors[i]);
                    }
                    bodyText += TABLE_END;
                }

                // user account errors
                if (log.UserAccountErrors.Count > 0)
                {
                    bodyText += TABLE_SUMMARY.Replace("[message]", "The following errors may have prevented staff accounts from being created or modified successfully.  Please forward this email to NorthStar support.(only first " + hardStopCount +" errors shown)");
                    bodyText += TABLE_HEADER.Replace("[data]", "Error");

                    for (var i = 0; i < hardStopCount && i < log.UserAccountErrors.Count; i++)
                    {
                        bodyText += TABLE_ROW.Replace("[data]", log.UserAccountErrors[i]);
                    }
                    bodyText += TABLE_END;
                }


                //bodyText += TABLE_HEADER;
                //bodyText.Replace("[col1]", )
                //bodyText = bodyText.Replace("[username]", email);

                emailText.Append(headerText);
                emailText.Append(bodyText);
                emailText.Append(footerText);

                //SmtpClient client = new SmtpClient();
                //var message = new SendGridMessage();
                //var split = ConfigurationManager.AppSettings["RolloverEmail"].Split(Char.Parse(",")).ToList();
                //var toAddresses = new List<string>() { ConfigurationManager.AppSettings["AdminEmail"]};
                //toAddresses.AddRange(split);
                ////message.AddTo(ConfigurationManager.AppSettings["RolloverEmail"],);
                ////message.AddTo(ConfigurationManager.AppSettings["AdminEmail"]);
                //message.AddTo(toAddresses);
                //message.From = new MailAddress("support@northstaret.net");
                //message.Html = emailText.ToString();
                //message.Subject = "North Star Automated Rollover Status";
                //message.EnableBcc("support@northstaret.net");

                //var credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["SendGridUser"], ConfigurationManager.AppSettings["SendGridPassword"]);
                //var transportWeb = new Web(credentials);
                //transportWeb.Deliver(message);


                // new sendgrid API
                var personalization = new Personalization();
                var split = ConfigurationManager.AppSettings["RolloverEmail"].Split(Char.Parse(",")).ToList();
                split.ForEach(p => personalization.AddTo(new Email(p)));

                var client = new SendGridAPIClient(ConfigurationManager.AppSettings["SendGridApiKey"]);
                //Email sendTo = new Email(ConfigurationManager.AppSettings["RolloverEmail"]);
                Email from = new Email("support@northstaret.net");
                Content mailContent = new Content("text/html", emailText.ToString());
                //var message = new Mail(from, "North Star Automated Rollover Status", null, mailContent);
                var message = new Mail();
                message.Contents = new List<Content>() { mailContent };
                message.From = from;
                message.Subject = "North Star Automated Rollover Status";
                message.AddPersonalization(personalization);

                dynamic response = client.client.mail.send.post(requestBody: message.Get()).GetAwaiter().GetResult();
                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var responseMsg = response.Body.ReadAsStringAsync().Result;
                    Log.Error($"Unable to send email: {responseMsg}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred while sending rollover status email: " + ex.Message);
                // log exception
                Log.Error("Error occurred while sending rollover status email: " + ex.Message);
            }
        }

        //public static void ResetPasswordEmail(string newPassword, string email, string username, string ccAddress,
        //                                      string fullName)
        //{
        //    try
        //    {
        //        StringBuilder emailText = new StringBuilder();
        //        string headerText = String.Empty;
        //        string footerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_FOOTER));
        //        string bodyText = String.Empty;

        //        headerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_HEADER));
        //        headerText = headerText.Replace("[server]", ConfigurationManager.AppSettings["siteUrlBase"]);
        //        headerText = headerText.Replace("[fullname]", fullName);
        //        headerText = headerText.Replace("[subject]", "Password Reset Notification");
        //        headerText = headerText.Replace("[subjectdetails]",
        //                                        "Your password has been reset in North Star Educational Tools.  Your login information is provided below.");

        //        bodyText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_NEW_USER_BODY));
        //        bodyText = bodyText.Replace("[username]", username);
        //        bodyText = bodyText.Replace("[password]", newPassword);

        //        emailText.Append(headerText);
        //        emailText.Append(bodyText);
        //        emailText.Append(footerText);

        //        //SmtpClient client = new SmtpClient();
        //        var message = SendGrid.GenerateInstance();
        //        message.AddTo(email);
        //        message.From = new MailAddress("support@northstaret.net");
        //        message.Html = emailText.ToString();
        //        message.Subject = "North Star Password Reset";
        //        message.AddBcc(ccAddress);
        //        message.AddBcc("support@northstaret.net");

        //        var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential("Hayaku77", "dammit77"),
        //                                                      "smtp.sendgrid.net", 25);
        //        transportInstance.Deliver(message);

        //    }
        //    catch (Exception ex)
        //    {
        //        // log exception
        //        log.Error("Error occurred while sending Password Reset email: " + ex.Message);
        //    }
        //}


        public static void SendGenericEmail(string to, string toName, string ccAddress, string subject,
                                        string subjectDetail, string bodyText, string rootUrl)
        {
            try
            {
                StringBuilder emailText = new StringBuilder();
                string headerText = String.Empty;
                string footerText = String.Empty;

                headerText = EMAIL_HEADER;
                headerText = headerText.Replace("[server]", rootUrl);
                headerText = headerText.Replace("[fullname]", toName);
                headerText = headerText.Replace("[subject]", subject);
                headerText = headerText.Replace("[subjectdetails]", subjectDetail);

                emailText.Append(headerText);
                emailText.Append(bodyText);
                emailText.Append(footerText);

                //var message = new SendGridMessage();
                //message.AddTo(to);
                ////message.AddTo("northstar.shannon@gmail.com");
                //message.From = new MailAddress("support@northstaret.net");
                //message.Html = emailText.ToString();
                //message.Subject = subject;
                ////message.AddCc(ccAddress);
                ////message.AddBcc("support@northstaret.net");

                //var credentials = new System.Net.NetworkCredential("Hayaku77", "dammit77");
                //var transportWeb = new Web(credentials);
                //transportWeb.Deliver(message);


                // new sendgrid API
                var client = new SendGridAPIClient(ConfigurationManager.AppSettings["SendGridApiKey"]);
                Email sendTo = new Email(to);
                Email from = new Email("support@northstaret.net");
                Content mailContent = new Content("text/html", emailText.ToString());
                var message = new Mail(from, subject, sendTo, mailContent);

                dynamic response = client.client.mail.send.post(requestBody: message.Get()).GetAwaiter().GetResult();
                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var responseMsg = response.Body.ReadAsStringAsync().Result;
                    Log.Error($"Unable to send email: {responseMsg}");
                }
            }
            catch (Exception ex)
            {
                // log exception
                Log.Error("Error occurred while sending Generic email: " + ex.Message);
            }
        }

        public static void PasswordResetLinkEmail(string email, string username, string uid, string rootUrl)
        {
            try
            {
                StringBuilder emailText = new StringBuilder();
                string headerText = String.Empty;
                string footerText = EMAIL_FOOTER;
                string bodyText = String.Empty;

                headerText = EMAIL_HEADER;
                headerText = headerText.Replace("[server]", rootUrl);//.AppSettings["siteUrlBase"]);
                headerText = headerText.Replace("[fullname]", email);
                headerText = headerText.Replace("[subject]", "Password Reset Link");
                headerText = headerText.Replace("[subjectdetails]",
                                                "");

                bodyText = EMAIL_PASSWORD_RESET_LINK;
                bodyText = bodyText.Replace("[link]", String.Format("<a href='{0}/#/reset-password-from-link/{1}'>[Click Here to Reset Password]</a>", rootUrl, uid));

                emailText.Append(headerText);
                emailText.Append(bodyText);
                emailText.Append(footerText);

                //SmtpClient client = new SmtpClient();
                var client = new SendGridAPIClient(ConfigurationManager.AppSettings["SendGridApiKey"]);
                Email to = new Email(email);
                Email from = new Email("support@northstaret.net");
                Content mailContent = new Content("text/html", emailText.ToString());
                var message = new Mail(from, "North Star Password Reset", to, mailContent);

                dynamic response = client.client.mail.send.post(requestBody: message.Get()).GetAwaiter().GetResult();

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var responseMsg = response.Body.ReadAsStringAsync().Result;
                    Log.Error($"Unable to send email: {responseMsg}");
                }
                //message.s(email);
                //message.AddTo("support@northstaret.net");
                //message.From = new Email("support@northstaret.net");

                //message.AddBcc("support@northstaret.net");
                //message.Html = emailText.ToString();
                //message.Subject = "North Star Password Reset";

                //var credentials = new System.Net.NetworkCredential("Hayaku77", "dammit77");
                //var transportWeb = new Web(credentials);
                //transportWeb.Deliver(message);
                //var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential("Hayaku77", "dammit77"),
                //                                              "smtp.sendgrid.net", 25);
                //transportInstance.Deliver(message);

            }
            catch (Exception ex)
            {
                // log exception
                Log.Error("Error occurred while sending Reset Link email: " + ex.Message);
            }
        }

        //private static string GetEMailTempatesDirectory()
        //{

        //    string eMailTemplatesDirectory = ConfigurationManager.AppSettings["emailTemplatePath"];


        //    return eMailTemplatesDirectory;
        //}

        //private static string GetEmailTemplate(string templateName)
        //{
        //    //var fileinfo = new EmbeddedFileProvider(typeof(EmailHandler).GetTypeInfo().Assembly, "NorthStar4.API");
        //    //var template = new Lazy<string>(() =>
        //    //{
        //    //    using (var stream = fileinfo.GetFileInfo("EmailTemplates." + templateName).CreateReadStream())
        //    //    using (var streamReader = new StreamReader(stream))
        //    //    {
        //    //        return streamReader.ReadToEnd();
        //    //    }
        //    //});

        //    return "";//template.Value;
        //}

        

        //private static string GetPDFDirectory()
        //{

        //    string pdfDirectory = ConfigurationManager.AppSettings["pdfPath"];


        //    return pdfDirectory;
        //}

        //private static string GetExportDirectory()
        //{

        //    string pdfDirectory = ConfigurationManager.AppSettings["exportpath"];


        //    return pdfDirectory;
        //}

        //public static void SendTeamMeetingConclusionNotices(int staffId, int TeamMeetingID, EFDistrictRepository repository)
        //{
        //    try
        //    {
        //        Staff staff = repository.Context.Staffs.First(p => p.ID == staffId);
        //        TeamMeeting tm = repository.Context.TeamMeetings.First(p => p.ID == TeamMeetingID);
        //        QueryStringHelper qsHelper = new QueryStringHelper(ConfigurationManager.AppSettings["siteUrlBase"] + "/rti/attend-team-meeting.aspx");
        //        qsHelper.SetParameter(ConstantHelper.FIELD_FROMEMAIL, "1");
        //        qsHelper.SetParameter(ConstantHelper.FIELD_TEAMMEETINGID, TeamMeetingID.ToString());

        //        StringBuilder emailText = new StringBuilder();
        //        string headerText = String.Empty;
        //        string footerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_FOOTER));

        //        headerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_HEADER));
        //        headerText = headerText.Replace("[server]", ConfigurationManager.AppSettings["siteUrlBase"]);
        //        headerText = headerText.Replace("[fullname]", staff.FullName);
        //        headerText = headerText.Replace("[subject]", "Team Meeting Concluded");
        //        headerText = headerText.Replace("[subjectdetails]", String.Format("FYI, the Team Meeting <b>{0}</b> at this date and time: <b><i>{1}</i></b> has <i>concluded</i>.", tm.Title, tm.MeetingTime));

        //        emailText.Append(headerText);
        //        emailText.Append(String.Format("<br><br>You can access this Team Meeting <a href='{0}'>HERE</a> to view any notes that were taken during the meeting.", qsHelper.All));
        //        emailText.Append(footerText);

        //        //SmtpClient client = new SmtpClient();
        //        var message = SendGrid.GenerateInstance();
        //        message.AddTo(staff.LoweredUserName);
        //        // message.AddTo("northstar.shannon@gmail.com");
        //        message.From = new MailAddress("support@northstaret.net");
        //        //message.From = new MailAddress(repository.CurrentStaff.Email);
        //        message.Html = emailText.ToString();
        //        message.Subject = "Team Meeting Concluded";
        //        message.AddCc(repository.CurrentStaff.Email);
        //        message.AddBcc("support@northstaret.net");

        //        var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential("Hayaku77", "dammit77"),
        //                                                      "smtp.sendgrid.net", 25);
        //        transportInstance.Deliver(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        // log exception
        //        log.Error("Error occurred while sending Team Meeting Notice email: " + ex.Message);
        //    }

        //}

        //public static void SendTeamMeetingNotices(int staffId, int TeamMeetingID, EFDistrictRepository repository,
        //                                          string controlText)
        //{
        //    try
        //    {
        //        Staff staff = repository.Context.Staffs.First(p => p.ID == staffId);
        //        TeamMeeting tm = repository.Context.TeamMeetings.First(p => p.ID == TeamMeetingID);


        //        controlText = Regex.Replace(controlText, "<script.*?</script>", "",
        //                                    RegexOptions.Singleline | RegexOptions.IgnoreCase);




        //        StringBuilder emailText = new StringBuilder();
        //        string headerText = String.Empty;
        //        string footerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_FOOTER));

        //        headerText = File.ReadAllText(Path.Combine(GetEMailTempatesDirectory(), EMAIL_HEADER));
        //        headerText = headerText.Replace("[server]", ConfigurationManager.AppSettings["siteUrlBase"]);
        //        headerText = headerText.Replace("[fullname]", staff.FullName);
        //        headerText = headerText.Replace("[subject]", "Team Meeting Invitation");
        //        headerText = headerText.Replace("[subjectdetails]", String.Format("You have been invited to the Team Meeting <b>{0}</b> at this date and time: <b><i>{1}</i></b>", tm.Title, tm.MeetingTime));

        //        emailText.Append(headerText);
        //        emailText.Append(controlText);
        //        emailText.Append(footerText);

        //        //SmtpClient client = new SmtpClient();
        //        var message = new SendGridMessage();
        //        message.AddTo(staff.LoweredUserName);
        //        message.ReplyTo = new MailAddress[] { new MailAddress(repository.CurrentStaff.Email) };
        //        //message.AddTo("northstar.shannon@gmail.com");
        //        message.From = new MailAddress("support@northstaret.net", repository.CurrentStaff.FullName);
        //        //message.From = new MailAddress(repository.CurrentStaff.Email);
        //        message.Html = emailText.ToString();
        //        message.Subject = "Team Meeting Invitation";
        //        //message.AddCc(repository.CurrentStaff.Email);
        //        message.AddBcc("support@northstaret.net");

        //        var transportInstance = SMTP.GenerateInstance(new System.Net.NetworkCredential("Hayaku77", "dammit77"),
        //                                                      "smtp.sendgrid.net", 25);
        //        transportInstance.Deliver(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        // log exception
        //        log.Error("Error occurred while sending Team Meeting Notice email: " + ex.Message);
        //    }

        //}


    }
}
