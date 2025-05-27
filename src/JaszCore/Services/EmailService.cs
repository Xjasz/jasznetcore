using JaszCore.Common;
using JaszCore.Models;
using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;

namespace JaszCore.Services
{
    [Service(typeof(EmailService))]
    public interface IEmailService
    {
        void SendDevErrorEmail(UnhandledExceptionEventArgs e);
        void SendTestEmail();
        void SendEmail(string recipient, string subject, string body);
        void SendMultipleRecipientEmail(string subject, string body, List<EmailAddress> recipients, List<EmailAddress> bccs, string fileAttachmentPath);
    }
    public class EmailService : IEmailService
    {
        private static ILoggerService Log => ServiceLocator.Get<ILoggerService>();

        private readonly ExchangeService ExchangeService;

        internal EmailService(string serviceEmail, string servicePass)
        {
            Log.Debug($"EmailService starting....");
            ExchangeService = ExchangeService ??= new ExchangeService(ExchangeVersion.Exchange2010_SP1);
            ExchangeService.Credentials = new WebCredentials(serviceEmail, servicePass);
            ExchangeService.TraceEnabled = true;
            ExchangeService.TraceFlags = TraceFlags.None;
            //ExchangeService.AutodiscoverUrl(serviceEmail, RedirectionUrlValidationCallback);
        }

        public void SendEmail(string recipient, string subject, string body)
        {
            Log.Debug($"Sent email to {recipient}....");
            EmailMessage email = new EmailMessage(ExchangeService);
            email.ToRecipients.Add(recipient);
            email.Subject = subject;
            email.Body = new MessageBody(body);
            email.Send().GetAwaiter().GetResult();
        }

        public void SendDevErrorEmail(UnhandledExceptionEventArgs e)
        {
            Log.Debug($"Sent error email....");
            var exception = (Exception)e.ExceptionObject;
            var etarget = "\n\nTargetSite:  " + exception.TargetSite?.Name;
            var emessage = "\nMessage:  " + exception?.Message;
            var estack = "\nStackTrace:  " + exception?.StackTrace;
            var iemessage = exception?.InnerException?.Message != null ? "\nInnerMessage:  " + exception?.InnerException?.Message : "";
            var iestack = exception?.InnerException?.StackTrace != null ? "\nInnerStackTrace:  " + exception?.InnerException?.StackTrace : "";
            var errorMessage = etarget + emessage + estack + iemessage + iestack;
            EmailMessage email = new EmailMessage(ExchangeService);
            email.ToRecipients.Add(S.GetErrorEmail());
            email.Subject = $"{S.APP_NAME} Error";
            email.Body = new MessageBody($"The application has thrown an error.  Please check error message below... {errorMessage}");
            email.Send().GetAwaiter().GetResult();
        }

        public void SendTestEmail()
        {
            Log.Debug($"SendTestEmail email....");
            EmailMessage email = new EmailMessage(ExchangeService);
            email.ToRecipients.Add(S.GetReceipient());
            email.Subject = $"{S.APP_NAME} Test Error";
            email.Body = new MessageBody($"This is a test please ignore....");
            email.Send().GetAwaiter().GetResult();
        }

        public void SendMultipleRecipientEmail(string subject, string body, List<EmailAddress> recipients, List<EmailAddress> bccs, string fileAttachmentPath)
        {
            Log.Debug($"SendMultipleRecipientEmail email ran Subject: {subject}");
            EmailMessage email = new EmailMessage(ExchangeService);
            if (recipients != null)
                recipients.ForEach(r => email.ToRecipients.Add(r.Address));
            if (bccs != null)
                bccs.ForEach(b => email.BccRecipients.Add(b.Address));
            email.Subject = subject;
            email.Body = new MessageBody(body);
            if (fileAttachmentPath != null)
                email.Attachments.AddFileAttachment(fileAttachmentPath);
            email.Send().GetAwaiter().GetResult();
        }

        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            var result = false;
            var redirectionUri = new Uri(redirectionUrl);
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }
    }
}