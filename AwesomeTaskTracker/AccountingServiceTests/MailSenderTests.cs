using AccountingService.Mail;

namespace AccountingServiceTests;

[TestClass]
public class MailSenderTests
{
    [TestMethod]
    public void SendMail()
    {
        var mailSender = new MailSender();
        mailSender.SendMail("rfrsk@yandex.ru", "50");
    }
}