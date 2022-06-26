using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimplySharp.Areas.Identity.Data;
using SimplySharp.Data;
using SimplySharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace SimplySharp.Controllers
{
    public class PaymentController : Controller
    {

        private readonly ClassContext _clsContext;
        
        
        /// <summary>
        /// A UserManager object to access current user information.
        /// </summary>
        private readonly UserManager<SimplySharpUser> _userManager;

        /// <summary>
        /// A DBContext object used to access the database.
        /// </summary>
        private readonly SimplySharpDBContext _db;

        /// <summary>
        /// The cost of tuition per credit hour.
        /// </summary>
        private int tuitionRate;

        /// <summary>
        /// The number of credit hours the user is enrolled in.
        /// </summary>
        private int creditHours;

        /// <summary>
        /// The total cost of tuition.
        /// </summary>
        private decimal totalTuitionCost;

        public PaymentController(ClassContext clsContext, UserManager<SimplySharpUser> userManager, SimplySharpDBContext db)
        {
            _clsContext = clsContext;
            
            _db = db;
            _userManager = userManager;
            tuitionRate = 100;
            creditHours = 0;
            totalTuitionCost = 0;
        }

        public async Task <IActionResult> Index()
        {
            var lstRegisteredClasses = await(from cr in _clsContext.ClassRegistration
                                     join c in _clsContext.Class on cr.ClassId equals c.Id
                                     where cr.StudentId == (_userManager.GetUserAsync(User).Result.Id)
                                     orderby cr.ClassId
                                     select c).ToListAsync();
            creditHours = 0;

            foreach (var cls in lstRegisteredClasses)
            {
                creditHours += cls.Credits;
            }

            //find if the user has made any past payments and update the total tuition balance
            decimal totalPayments = 0;
            var lstPayments = await (from p in _clsContext.Payment
                                     where p.UserId == (_userManager.GetUserAsync(User).Result.Id)
                                     orderby p.UserId
                                     select p).ToListAsync();
            foreach (var payment in lstPayments)
            {
                if (payment.PaymentAmount > 0)
                {
                    totalPayments += payment.PaymentAmount;
                }
            }
            totalTuitionCost = (creditHours * tuitionRate) - totalPayments;

            ViewBag.TotalCost = totalTuitionCost;

            return View();
        }

        // POST: Payment/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string ccNumber, string expMonth, string expYear, string cvcNumber, string amount, string description, bool isUnitTest, string studentId)
        {           
           
            // Create base path
            string path = "https://api.stripe.com/v1/";

            // Initiate Post API Call to process payment
            using (var httpClient = new HttpClient())
            {
                // Populate request headers to pass to the server
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer sk_test_51J1IQbKmskNFO4wINsJX5HtvxPMfHDQ4pkwnM3rEV0CRzQyruiYXfLyQ6JanFsrR2XmTVHbdu0JDz53F6FbN7byF00SCipuX38");
                
                // Create token request
                var objTokenRequest = new HttpRequestMessage(HttpMethod.Post, path + "tokens");

                // Populate token request content to pass to the server
                var dctTokenContentPairs = new Dictionary<string, string>
                {
                    { "card[number]", ccNumber },
                    { "card[exp_month]", expMonth },
                    { "card[exp_year]", expYear },
                    { "card[cvc]", cvcNumber }
                };
                objTokenRequest.Content = new FormUrlEncodedContent(dctTokenContentPairs);

                // Check token response
                using (HttpResponseMessage objTokenResponse = await httpClient.SendAsync(objTokenRequest))
                {
                    if (objTokenResponse.IsSuccessStatusCode)
                    {
                        
                        //Parse response to get token
                        var deserializedTokenObject = JsonConvert.DeserializeObject<PostToken>(await objTokenResponse.Content.ReadAsStringAsync());
                        string strToken = deserializedTokenObject.id;
                        objTokenResponse.Dispose();

                        // Populate charge request
                        var objChargeRequest = new HttpRequestMessage(HttpMethod.Post, path + "charges");
                        
                        //Check if description is empty and replace with default string
                        if (String.IsNullOrEmpty(description))
                        {
                            description = "Tuition payment";
                        }

                        //We need to take in the user's amount, which should be in a string of "xxx.xx" format, turn it into "xxxxx"
                        //multiply by 100 and convert to int. this serves a double purpose; removing any decimal point and truncating useless digits after decimal point.
                        double centsDoubleAmount = Convert.ToDouble(amount);
                        centsDoubleAmount *= 100;
                        int centsAmount = Convert.ToInt32(centsDoubleAmount);

                        //Then reconvert to string for the dictionary send
                        string stringAmount = Convert.ToString(centsAmount);

                        // Populate charge request content to pass to the server
                        var dctChargeContentPairs = new Dictionary<string, string>
                        {
                            { "amount", stringAmount },
                            { "currency", "usd" },
                            { "source", strToken },
                            { "description", description }
                        };
                        objChargeRequest.Content = new FormUrlEncodedContent(dctChargeContentPairs);

                        // Check token response
                        using (HttpResponseMessage objChargeResponse = await httpClient.SendAsync(objChargeRequest))
                        {
                            if (objChargeResponse.IsSuccessStatusCode)
                            {
                                //Parse response to get receipt url
                                var deserializedChargeObject = JsonConvert.DeserializeObject<PostCharge>(await objChargeResponse.Content.ReadAsStringAsync());
                                string strReceiptUrl = deserializedChargeObject.receipt_url;
                                double dblAmountCharged = deserializedChargeObject.amount_captured / 100.0;
                                objChargeResponse.Dispose();

                                if (!isUnitTest)
                                {
                                    TempData["Msg"] = "<p>Payment successful. <a target='_blank' href=" + strReceiptUrl + ">Click on this link for the Stripe receipt.</a></p>";
                                }

                                //Update database with new payment
                                Payment newPayment = new Payment();
                                newPayment.UserId = isUnitTest ? studentId : _userManager.GetUserAsync(User).Result.Id;
                                newPayment.PaymentAmount = Convert.ToDecimal(dblAmountCharged);
                                newPayment.PaymentDate = DateTime.Now;
                                await _clsContext.Payment.AddAsync(newPayment);
                                await _clsContext.SaveChangesAsync();
                           
                                
                            }
                            else if (!isUnitTest)
                            {
                                string failedStatus = objTokenResponse.StatusCode.ToString();
                                string reasonPhrase = objTokenResponse.ReasonPhrase;

                                TempData["Msg"] = "<p>Payment failed. Reason- " + reasonPhrase + ". " + failedStatus + "</p>";

                            }                            
                        }
                        
                    }
                    else if (true)
                    {
                        string failedStatus = objTokenResponse.StatusCode.ToString();
                        string reasonPhrase = objTokenResponse.ReasonPhrase;

                        TempData["Msg"] = "<p>Payment failed. Reason- " + reasonPhrase + ". " + failedStatus + "</p>";

                    }                    
                }
            }
            // Return to Tuition view
            return RedirectToAction(nameof(Index));
            
        }
    }
    // JSON structures
    public struct PostToken
    {
        public string id;
    }

    public struct PostCharge
    {
        public int amount_captured;
        public string receipt_url;   
    }

    
}
