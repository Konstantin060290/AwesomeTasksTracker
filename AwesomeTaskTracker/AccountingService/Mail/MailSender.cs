using System.Net;
using System.Net.Mail;
using AccountingService.Settings;
using Microsoft.Extensions.Options;

namespace AccountingService.Mail;

public class MailSender
{
    private readonly MailSettings _mailSettings;

    public MailSender(IOptions<MailSettings> mailSettings)
    {
        _mailSettings = mailSettings.Value;
    }
    public void SendMail(string userMail, string sum)
    {
        var mail = new MailMessage();
        mail.From = new MailAddress("rfrsk@yandex.ru"); // Адрес отправителя
        mail.To.Add(new MailAddress(userMail)); // Адрес получателя
        mail.Subject = "Попуг, Ваша выплата";
        mail.Body = $"За сегодня Вы заработали {sum}";

        var client = new SmtpClient();
        client.Host = "smtp.yandex.ru";
        client.Port = 587; // Обратите внимание что порт 587
        client.EnableSsl = true;
        client.Credentials = new NetworkCredential(_mailSettings.ServiceEmail, _mailSettings.Password); // Ваши логин и пароль
        client.Send(mail);

        Console.ReadKey();
    }
}