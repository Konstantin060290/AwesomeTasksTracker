using System.Globalization;
using AnalyticService.BrokerExchange;
using AnalyticService.Context;
using AnalyticService.Models;
using AnalyticService.ViewModels;
using Microsoft.AspNetCore.Mvc;
using TasksTrackerService.WebConstants;

namespace AnalyticService.Controllers;

public class AnalyticController : Controller
{
    private readonly ApplicationContext _context;
    private readonly BrokerProducer _brokerProducer;

    public AnalyticController(ApplicationContext context)
    {
        _context = context;
        _brokerProducer = new BrokerProducer();
    }

    [HttpGet]
    public IActionResult GetAnalytics()
    {
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }
        
        if (userRoleName != RoleNames.AdminRole)
        {
            return Unauthorized("У попуга нет прав на просмотр аналитики");
        }

        var viewModel = new AnalyticsViewModel();

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> GetAnalytics(AnalyticsViewModel viewModel)
    {
        if (!AuthChecker.IsUserAuthenticated(_context, out var userRoleName, out var userEmail))
        {
            return Unauthorized("Попуг не аутентифицирован");
        }
        if (userRoleName != RoleNames.AdminRole)
        {
            return Unauthorized("У попуга нет прав на просмотр аналитики");
        }

        if (viewModel.FinishDate is not null)
        {
            await _brokerProducer.SendAnalyticRequest(userEmail!, DateTime.ParseExact(viewModel.StartDate!,
                    "d/M/yyyy",
                    CultureInfo.InvariantCulture), DateTime.ParseExact(viewModel.FinishDate, "d/M/yyyy",
                    CultureInfo.InvariantCulture),
                EventsNames.AnalyticWasRequested); 
        }
        else
        {
            await _brokerProducer.SendAnalyticRequest(userEmail!, DateTime.ParseExact(viewModel.StartDate!,
                    "d/M/yyyy",
                    CultureInfo.InvariantCulture), new DateTime(1,1,1),
                EventsNames.AnalyticWasRequested);
        }

        var consumer = new AnalyticConsumer();
        var answer = consumer.ConsumeAnalyticAnswer(userEmail!);
        
        viewModel.TopManagersEarnsMoney = answer.TopManagersEarnsMoney;
        viewModel.PopugsQtyWithNegativeBalance = answer.PopugsQtyWithNegativeBalance;
        viewModel.MostExpensiveTaskForDay = Convert.ToDecimal(answer.MostExpensiveTaskForDay);
        viewModel.MostExpensiveTaskForPeriod = Convert.ToDecimal(answer.MostExpensiveTaskForPeriod);
        
        return View(viewModel);
    }
}