using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace AccountingService.Mail;

public class MailSender
{
    public async void SendMail(string userMail, string sum)
    {
        var client = new MailjetClient("",
            "");

        var request = new MailjetRequest
            {
                Resource = Send.Resource,
                
            }
            .Property(Send.Messages, new JArray
            {
                new JObject
                {
                    {
                        "From",
                        new JObject
                        {
                            { "Email", "rfrsk@yandex.ru" },
                            { "Name", "AccountingService" }
                        }
                    },
                    {
                        "To",
                        new JArray
                        {
                            new JObject
                            {
                                {
                                    "Email", userMail
                                },
                                {
                                    "Name",
                                    "User"
                                }
                            }
                        }
                    },
                    {
                        "Subject",
                        "Выплата денег"
                    },
                    {
                        "HTMLPart",
                        $"Вам выплачено: {sum}"
                    },
                }
            });
        var response = client.PostAsync(request);
        if (response.IsCompleted)
        {
            
        }
    }
}