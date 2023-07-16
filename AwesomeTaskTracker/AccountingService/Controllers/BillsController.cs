using AccountingService.Context;
using AccountingService.Models;
using AccountingService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.WebConstants;

namespace AccountingService.Controllers;

public class BillsController : Controller
{
    private readonly ApplicationContext _context;

    public BillsController(ApplicationContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult ManageBills()
    {
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }

        var viewModel = new BillsViewModel();
        var bills = _context.Bills.ToList();

        if (userRoleName == RoleNames.AccountantRole)
        {
            AddAllBills(bills, viewModel);
        }
        else
        {
            AddUserBills(bills, viewModel, userEmail!);
        }

        viewModel.UserRole = userRoleName;
        viewModel.UserEmail = userEmail!;

        return View(viewModel);
    }

    private void AddAllBills(List<Bill> bills, BillsViewModel viewModel)
    {
        foreach (var bill in bills)
        {
            var billViewModel = new BillViewModel
            {
                BillId = bill.BillId,
                Balance = bill.Balance,
                Status = bill.Status
            };

            var user = _context.Users.FirstOrDefault(u => u.UserId == bill.UserId);
            if (user is not null)
            {
                billViewModel.UserEmail = user.Email;
            }

            viewModel.Bills.Add(billViewModel);
        }
    }

    private void AddUserBills(IEnumerable<Bill> bills, BillsViewModel viewModel, string userMail)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == userMail);
        if (user is null)
        {
            return;
        }

        foreach (var bill in bills.Where(b => b.UserId == user.UserId))
        {
            var billViewModel = new BillViewModel
            {
                BillId = bill.BillId,
                Balance = bill.Balance,
                Status = bill.Status
            };

            if (user is not null)
            {
                billViewModel.UserEmail = user.Email;
            }

            viewModel.Bills.Add(billViewModel);
        }
    }
    
    //На самом деле это GET, но через него не прокинуть billId
    [HttpPost]
    public IActionResult ShowBillLog(int billId)
    {
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }

        var bill = _context.Bills.FirstOrDefault(b => b.BillId == billId);
        var user = _context.Users.FirstOrDefault(u => u.UserId == bill!.UserId);
        var billViewModel = new BillViewModel();
        if (bill is not null)
        {
            billViewModel.BillId = bill.BillId;
            billViewModel.Status = bill.Status;
            billViewModel.Balance = bill.Balance;
        }

        if (user is not null)
        {
            billViewModel.UserEmail = user.Email;
        }

        var transactions = _context.Transactions.ToList().Where(t => t.BillId == billId).ToList();

        foreach (var transaction in transactions)
        {
            billViewModel.TransactionViewModels.Add(
                new TransactionViewModel()
            {
                TransactionId = transaction.TransactionId,
                BillId = transaction.BillId,
                Accrued = transaction.Accrued,
                WrittenOff = transaction.WrittenOff,
                TaskDescription = transaction.TaskDescription,
                TransactionDate = transaction.TransactionDate
            });
        }
        
        return View(billViewModel);
    }
}