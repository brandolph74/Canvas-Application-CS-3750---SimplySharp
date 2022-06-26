using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimplySharp.Areas.Identity.Data;

namespace SimplySharp.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<SimplySharpUser> _userManager;
        private readonly SignInManager<SimplySharpUser> _signInManager;

        public IndexModel(
            UserManager<SimplySharpUser> userManager,
            SignInManager<SimplySharpUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "First Name")]
            public string FirstName { get; set; }
            [Display(Name = "Last Name")]
            public string LastName { get; set; }
            [Display(Name = "Username")]
            public string Username { get; set; }

            
            [MinimumAge(16)]
            [MaximumAge(125)]
            [DataType(DataType.Date)]
            [Display(Name = "Date of Birth")]
            [BindProperty]
            public DateTime Birthdate { get; set; }

            [Display(Name ="Address Line 1")]
            public string AddressLine1 { get; set; }

            [Display(Name = "Address Line 2")]
            public string AddressLine2 { get; set; }

            [Display(Name = "City")]
            public string City { get; set; }

            [Display(Name = "State")]
            public string State { get; set; }

            [Display(Name = "Zip Code")]
            public string Zip { get; set; }

            [Display(Name = "Biography")]
            public string Biography { get; set; }

            [Display(Name = "Link 1:")]
            public string Link1 { get; set; }

            [Display(Name = "Link 2:")]
            public string Link2 { get; set; }

            [Display(Name = "Link 3:")]
            public string Link3 { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Profile Picture")]
            public byte[] ProfilePicture { get; set; }

        }

        private async Task LoadAsync(SimplySharpUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var firstName = user.FirstName;
            var lastName = user.LastName;
            var birthDate = user.Birthdate;
            var addressL1 = user.AddressLine1;
            var addressL2 = user.AddressLine2;
            var city = user.City;
            var state = user.State;
            var zip = user.Zip;
            var bio = user.Biography;
            var link1 = user.Link1;
            var link2 = user.Link2;
            var link3 = user.Link3;
            var profilePicture = user.ProfilePicture;
            Username = userName;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Username = userName,
                FirstName = firstName,
                LastName = lastName,
                Birthdate = birthDate,
                AddressLine1 = addressL1,
                AddressLine2 = addressL2,
                City = city,
                State = state,
                Zip = zip,
                Biography = bio,
                Link1 = link1,
                Link2 = link2,
                Link3 = link3,
                ProfilePicture = profilePicture
        };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }


            var firstName = user.FirstName;
            var lastName = user.LastName;
            if (Input.FirstName != firstName)
            {
                user.FirstName = Input.FirstName;
                await _userManager.UpdateAsync(user);
            }
            if (Input.LastName != lastName)
            {
                user.LastName = Input.LastName;
                await _userManager.UpdateAsync(user);
            }


            var birthDate = user.Birthdate;
            if(Input.Birthdate != birthDate)
            {
                user.Birthdate = Input.Birthdate;
                await _userManager.UpdateAsync(user);
            }


            var addressL1 = user.AddressLine1;
            var addressL2 = user.AddressLine2;
            if (Input.AddressLine1 != addressL1)
            {
                user.AddressLine1 = Input.AddressLine1;
                await _userManager.UpdateAsync(user);
            }
            if (Input.AddressLine2 != addressL2)
            {
                user.AddressLine2 = Input.AddressLine2;
                await _userManager.UpdateAsync(user);
            }

            var city = user.City;
            var state = user.State;
            var zip = user.Zip;
            if (Input.City != city)
            {
                user.City = Input.City;
                await _userManager.UpdateAsync(user);
            }
            if (Input.State != state)
            {
                user.State = Input.State;
                await _userManager.UpdateAsync(user);
            }
            if (Input.Zip != zip)
            {
                user.Zip = Input.Zip;
                await _userManager.UpdateAsync(user);
            }


            var bio = user.Biography;
            var link1 = user.Link1;
            var link2 = user.Link2;
            var link3 = user.Link3;
            if (Input.Biography != bio)
            {
                user.Biography = Input.Biography;
                await _userManager.UpdateAsync(user);
            }
            if (Input.Link1 != link1)
            {
                user.Link1 = Input.Link1;
                await _userManager.UpdateAsync(user);
            }
            if (Input.Link2 != link2)
            {
                user.Link2 = Input.Link2;
                await _userManager.UpdateAsync(user);
            }
            if (Input.Link3 != link3)
            {
                user.Link3 = Input.Link3;
                await _userManager.UpdateAsync(user);
            }

            // Add user profile picture
            // IFormFile is an interface that represents a file sent with the HttpRequest.
            if (Request.Form.Files.Count > 0)
            {
                IFormFile file = Request.Form.Files.FirstOrDefault();

                // Use memory streams to convert the image file to memory object/byte array.
                // A memory stream is created from an array of unsigned bytes rather than from a file or other stream.
                using (var dataStream = new MemoryStream())
                {
                    // Copy the file into dataStream
                    await file.CopyToAsync(dataStream);

                    // Set Profile Image to the dataStream in the form of an array
                    user.ProfilePicture = dataStream.ToArray();
                }
                await _userManager.UpdateAsync(user);
            }


            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
