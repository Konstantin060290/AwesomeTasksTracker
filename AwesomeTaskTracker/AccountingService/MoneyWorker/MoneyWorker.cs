using System.Globalization;
using AccountingService.Context;
using AccountingService.Mail;

namespace AccountingService.MoneyWorker;

public class MoneyWorker : IMoneyWorker
{
    private readonly ApplicationContext _context;

    public MoneyWorker(ApplicationContext context)
    {
        _context = context;
        ManageTransactions();
    }

    private void ManageTransactions()
    {
        Task.Run(Manage);
    }

    private async void Manage()
    {
        while (true)
        {
            Thread.Sleep(10000);
            var today = DateTime.UtcNow;
            if (today.Hour == 0 && today.Minute == 0 && today.Second < 11)
            {
                var transactions = _context.Transactions.ToList()
                    .Where(t => t.TransactionDate.Date == today.Date)
                    .ToList()
                    .GroupBy(t => t.TransactionId);
                
                foreach (var grouping in transactions)
                {
                    decimal accrued = 0;
                    decimal writtenOff = 0;
                    foreach (var transaction in grouping)
                    {
                        var userId = _context.Bills.ToList().FirstOrDefault(b => b.BillId == transaction.BillId)!
                            .UserId;
                        var userMail = _context.Users.ToList().FirstOrDefault(u => u.UserId == userId)!.Email;
                        accrued += transaction.Accrued;
                        writtenOff += transaction.WrittenOff;
                        var sum = accrued + writtenOff;
                        
                        var bill = _context.Bills.FirstOrDefault(b => b.BillId == userId);
                        bill!.Balance += sum;
                        await _context.SaveChangesAsync();
                        
                        var mailSender = new MailSender();
                        mailSender.SendMail(userMail, sum.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }
        // ReSharper disable once FunctionNeverReturns
    }
}

public interface IMoneyWorker
{
}