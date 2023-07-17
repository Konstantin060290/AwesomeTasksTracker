using AccountingService.Mail;
using AccountingService.Settings;
using Microsoft.Extensions.Options;

namespace AccountingServiceTests;

[TestClass]
public class MailSenderTests
{
    [TestMethod]
    public void SendMail()
    {
        var mailSettings = new MailSettings
        {
            Password = "lala",
            ServiceEmail = "lala@yandex.ru"
        };
        var options = Options.Create(mailSettings);
        var mailSender = new MailSender(options);
        mailSender.SendMail("rfrsk@yandex.ru", "50");
    }
}