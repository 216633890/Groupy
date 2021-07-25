using Groupy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Groupy.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        GroupyEntities storeDB = new GroupyEntities();
        const string PromoCode = "50";

        public ActionResult AddressAndPayment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddressAndPayment(FormCollection values)
        {
            var order = new Order();
            TryUpdateModel(order);

            try
            {
                if (string.Equals(values["PromoCode"], PromoCode,
                    StringComparison.OrdinalIgnoreCase) == false)
                {
                    return View(order);
                }
                else
                {
                    FraudCheck fraudCheck = new FraudCheck();

                    order.Username = User.Identity.Name;
                    order.OrderDate = DateTime.Now;
                    order.OrderCountry = fraudCheck.GetCountry();

                    //storeDB.Orders.Add(order);
                    //storeDB.SaveChanges();

                    //Start credit card fruad checks
                    if (!fraudCheck.IsValidTrans(order))
                    {
                        //logout
                        return RedirectToAction("AutoLogOff", "Account");
                    }

                    var cart = ShoppingCart.GetCart(this.HttpContext);

                    cart.CreateOrder(order);

                    return RedirectToAction("Complete",
                        new { id = order.OrderId });
                }
            }
            catch
            {
                return View(order);
            }
        }

        public ActionResult Complete(int id)
        {
            bool isValid = storeDB.Orders.Any(
                o => o.OrderId == id &&
                o.Username == User.Identity.Name);

            if (isValid)
            {
                return View(id);
            }
            else
            {
                return View("Error");
            }
        }
    }
}