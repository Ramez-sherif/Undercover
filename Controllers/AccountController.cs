using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PlaySafe.Data;
using PlaySafe.Models;
using PlaySafe.ViewModels;
using System;
using System.Security.Claims;

namespace PlaySafe.Controllers
{
    public class AccountController : Controller
    {
        private readonly dbContext _context;
        private readonly IWebHostEnvironment HostingEnvironment;
        private SignInManager<ApplicationUser> SignInManager;
        private UserManager<ApplicationUser> UserManager;

        public AccountController(dbContext context, IWebHostEnvironment hostingEnvironment, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            HostingEnvironment = hostingEnvironment;
            SignInManager = signInManager;
            UserManager = userManager;
            _context = context;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> listAllGuards()
        {
            var current_user = await UserManager.GetUserAsync(User);
            var users = await UserManager.GetUsersInRoleAsync("Guard");
            List<ApplicationUser> listOfGuards = new List<ApplicationUser>();
            foreach(var guard in users)
            {
                if(guard.supervisorId == current_user.Id)
                {
                    listOfGuards.Add(guard);
                }
            }
            return View(listOfGuards);
        }
        [HttpGet]
        [Authorize(Roles = "Owner,Guard")]
        public async Task<IActionResult> listAllAdmins()
        {
            var users = await UserManager.GetUsersInRoleAsync("Admin");
            return View(users);
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Guard")]
        public async Task<IActionResult> listAllPlayers()
        {
            string id = null;
            var Current_User = await UserManager.GetUserAsync(User);
            var users = await UserManager.GetUsersInRoleAsync("Player");
            List<playerViewModel> listOfPlayers = new List<playerViewModel>();
            foreach (var player in users)
            {

                if (player.supervisorId == Current_User.Id || player.supervisorId== Current_User.supervisorId)
                {
                    id = player.Id;
                    var playerPoints = _context.player.Where(x => x.userId == player.Id).FirstOrDefault();
                    playerViewModel playerAdd = new playerViewModel()
                    {
                        name = player.name,
                        userName = player.UserName,
                        phoneNum = player.PhoneNumber,
                        points = playerPoints.points,
                        createdDate = player.createdDate,
                        photo = null,
                        password = "",
                        confirmPassword = ""

                    };
                    listOfPlayers.Add(playerAdd);
                    HttpContext.Session.SetString(playerAdd.userName,id);
                }
            }
            return View(listOfPlayers);
        }
        [HttpGet]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> confirmDelete(string? id)
        {
            ViewBag.id = id;
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Delete(string? id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                var role = await UserManager.GetRolesAsync(user);                               
                if (role.ElementAt(0) == "Admin")
                {
                    var allPlayers = await UserManager.GetUsersInRoleAsync("Player");
                    foreach (var player in allPlayers)
                    {
                        try
                        {
                            if (player.supervisorId == id)
                                await UserManager.DeleteAsync(player);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }

                    var allGuards = await UserManager.GetUsersInRoleAsync("Guard");
                    foreach (var guard in allGuards)
                    {
                        try
                        {
                            if(guard.supervisorId == id)
                                await UserManager.DeleteAsync(guard);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }
                }
                var result = await UserManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                        if(role.ElementAt(0) == "Admin")
                        {
                            return RedirectToAction("listAllAdmins");
                        }else if (role.ElementAt(0) == "Guard")
                        {
                            return RedirectToAction("listAllGuards");
                        }
                        else if (role.ElementAt(0) == "Player")
                        {
                            return RedirectToAction("listAllPlayers");
                        }
                }                
            }
            return RedirectToAction("viewSpecial");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult rarw()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> rarw(registerViewModel model)
        {
            var user = new ApplicationUser()
            {
                name = model.name,
                UserName = model.userName,
                createdDate = DateTime.Now,
                PhoneNumber = model.phoneNum
            };
            var result = await UserManager.CreateAsync(user, model.password);
            if (result.Succeeded)
            {
                var role = await UserManager.AddToRoleAsync(user, "Owner");
                if (role.Succeeded)
                {
                    //await SignInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "home");
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Block(string? id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            user.status = false;
            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {            
                return RedirectToAction("listAllAdmins");
            }
            return Redirect("/Account/Details/"+id);
        }
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> UnBlock(string? id)
        {
            var user = await UserManager.FindByIdAsync(id);
            user.status = true;
            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("listAllAdmins");
            }
            return Redirect("/Account/Details/" +id);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult AddSpecials()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> AddSpecials(specialsViewModel s)
        {
            if (ModelState.IsValid)
            {
                var admin = await UserManager.GetUserAsync(User);
                string filename = null;
                string id = Guid.NewGuid().ToString();
                string filePath = "specials\\" + id;
                string path = Path.Combine(HostingEnvironment.WebRootPath, filePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    filename = s.photo.FileName;
                    s.photo.CopyTo(new FileStream(Path.Combine(path, filename), FileMode.Create));
                }
                var special = new specials()
                {
                    id = new Guid(id),
                    description = s.description,
                    price = s.price,
                    photo = filename,
                    supervisorId = admin.Id
                };
                _context.specials.Add(special);
                _context.SaveChanges();
                return RedirectToAction("AddSpecials", "Account");

            }
            IEnumerable<ModelError> allErrors = ModelState.Values.SelectMany(v => v.Errors);
            Console.WriteLine(allErrors);
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSpecials()
        {
            var admin = await UserManager.GetUserAsync(User);
            if (admin == null)
                return RedirectToAction("Login", "Account");
            var allspecials = await _context.specials.ToListAsync();
            List<specials> specials = new List<specials>(); 
            foreach(var special in allspecials)
            {
                if(special.supervisorId == admin.Id)
                    specials.Add(special);  

            }
            return View(specials);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveSpecials(Guid ? id)
        { 
            if(id == null)
                return NotFound();  
            
            else
            {
                var special = await _context.specials.FindAsync(id);
                if (special != null)
                {
                    _context.specials.Remove(special);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("RemoveSpecials","Account");
        }

            [Authorize(Roles = "Owner,Guard,Player,Admin")]
        public async Task<IActionResult> viewSpecial()
        {
            var admin = await UserManager.GetUserAsync(User);
            string id = null;
            if (User.IsInRole("Admin"))
            {
                id = admin.Id;
            }
            else if (User.IsInRole("Guard") || User.IsInRole("Player"))
            {
                id = admin.supervisorId;
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            List<specials> s = _context.specials.ToList();
            List<specials> Special = new List<specials>();
            foreach (var special in s)
            {
                //var S = await UserManager.FindByIdAsync(special.supervisorId);
                if (special.supervisorId == id)
                {
                    Special.Add(special);
                }

            }

            return View(Special);

        }
        [HttpGet]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || UserManager.Users == null)
            {
                return NotFound();
            }
            var editUser = await UserManager.FindByIdAsync(id);
            if (editUser == null) return NotFound();
            registerViewModel register = new registerViewModel()
            {
                name = editUser.name,
                userName =editUser.UserName,
                password = "",
                confirmPassword = "",
                phoneNum = editUser.PhoneNumber,
                createdDate = editUser.createdDate,
                photo = null
            };
            return View(register);
        }
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> Edit(string? id, registerViewModel register)
        {
            var user = await UserManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            else
            {
                user.name = register.name;
                user.UserName = register.userName;
                user.PhoneNumber = register.phoneNum; 
                user.PasswordHash = UserManager.PasswordHasher.HashPassword(user,register.password);
                var result = await UserManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("viewSpecial","Account");
                }
            }
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public IActionResult registerForAdmin()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> registerForAdmin(registerViewModel model)
        {
            if (ModelState.IsValid)
            {

                var current_user = await UserManager.GetUserAsync(User);
                string id = Guid.NewGuid().ToString();
                var user = new ApplicationUser()
                {
                    Id = id,
                    name = model.name,
                    UserName = model.userName,
                    createdDate = DateTime.Now,
                    supervisorId = null,
                    PhoneNumber = model.phoneNum,
                    status = true
                };

                var result = await UserManager.CreateAsync(user, model.password);
                if (result.Succeeded)
                {
                    var role = await UserManager.AddToRoleAsync(user, "Owner");
                    if (role.Succeeded)
                    {
                        //await SignInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "Owner,Admin")]
        public IActionResult register()
        {
            string userType = null;
            if (User.IsInRole("Owner"))//owner
            {
                userType = "Admin";

            }
            else if (User.IsInRole("Admin"))//admin
            {
                userType = "Guard";
            }
            ViewBag.role = userType;
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> register(registerViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userType = null;
                string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (User.IsInRole("Owner"))//owner
                {
                    userType = "Admin";
                    id = User.FindFirstValue(ClaimTypes.NameIdentifier);

                }
                else if(User.IsInRole("Admin"))//admin
                {
                    userType = "Guard";
                    id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }
                else
                {
                    return RedirectToAction("login", "Account");//redirect to an error //couldnt register
                }
                ViewBag.role = userType;
                var user = new ApplicationUser()
                {
                    name = model.name,
                    UserName = model.userName,
                    createdDate = DateTime.Now,
                    supervisorId = id,
                    PhoneNumber = model.phoneNum,
                    status = true
                };
                var result = await UserManager.CreateAsync(user, model.password);
                
                if (result.Succeeded)
                {
                    var role = await UserManager.AddToRoleAsync(user, userType);
                    if (role.Succeeded)
                    {
                        //await SignInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("viewSpecial", "Account");

                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Guard")]
        public IActionResult registerForPlayer()
        {
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Guard")]
        public async Task<IActionResult> registerForPlayer(playerViewModel model)
        {
            if (ModelState.IsValid)
            {

                var current_user = await UserManager.GetUserAsync(User);
                string filename = null;
                string id = Guid.NewGuid().ToString();
                string filePath = "images\\" + id;
                string path = Path.Combine(HostingEnvironment.WebRootPath, filePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    filename = model.photo.FileName;
                    model.photo.CopyTo(new FileStream(Path.Combine(path, filename), FileMode.Create));
                }
                var user = new ApplicationUser()
                {
                    Id = id,
                    name = model.name,
                    UserName = model.userName,
                    createdDate = DateTime.Now,
                    supervisorId = current_user.supervisorId,
                    PhoneNumber = model.phoneNum
                };

                var player = new player()    
                {
                    Id = Guid.NewGuid(),
                    userId = id,
                    photo = filename,
                    points = 0

                };
                var result = await UserManager.CreateAsync(user, model.password);
                if (result.Succeeded)
                {
                    _context.player.Add(player);
                    _context.SaveChanges();
                    var role = await UserManager.AddToRoleAsync(user, "Player");
                    if (role.Succeeded)
                    {
                        //await SignInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("viewSpecial","Account");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> logOut()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction("index","home");
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> checkLogin(string? returnUrl)
        {
            var user = await UserManager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound();
            }
            if(User.IsInRole("Player") || User.IsInRole("Guard"))
            {
                var admin = await UserManager.FindByIdAsync(user.supervisorId);
                if(admin.status == false)
                {
                    await SignInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "An Owner has blocked you and your admin, please contact whoever gave you this account");
                    return RedirectToAction("login");
                }
                if (User.IsInRole("Player"))
                {
                    var user2 = await UserManager.GetUserAsync(User);
                    return Redirect("/Account/playerDetails/"+user2.Id);
                }
                else RedirectToAction("registerForPlayer", "Account");
            }
            if (user.status == false)
            {
                await SignInManager.SignOutAsync();
                ModelState.AddModelError(string.Empty, "An Owner has blocked you, please contact whoever gave you this account");
                return RedirectToAction("login");
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            if (User.IsInRole("Owner"))
            {
                return Redirect("/Home/Index");
            }
            return Redirect("/Account/viewSpecial");
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> login(string ReturnUrl)
        {
            var user = await UserManager.GetUserAsync(User);
            if (SignInManager.IsSignedIn(User))
            {
                if(User.IsInRole("Guard")) return RedirectToAction("registerForPlayer", "Account");
                if (User.IsInRole("Player")) return Redirect("/Account/playerDetails/"+user.Id);
                if (User.IsInRole("Admin")) return RedirectToAction("addSpecials", "Account");
                if (User.IsInRole("Owner")) return RedirectToAction("Index", "Home");
            }
            if (ReturnUrl == null)
            {
                ReturnUrl = string.Empty;
            }
            HttpContext.Session.SetString("ReturnUrl", ReturnUrl);
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> login(loginViewModel model)
        {
           
            var returnUrl = HttpContext.Session.GetString("ReturnUrl");
            if (ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(model.userName, model.password, model.rememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect("/Account/checkLogin");
                    }
                    ModelState.AddModelError(string.Empty, "The Url your using is not safe or unknown \n please use another one or check if it is right");
                    return RedirectToAction("checkLogin");
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Owner,Guard,Admin")]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.id = id;
            registerViewModel register = new registerViewModel()
            {
                name = user.name,
                userName = user.UserName,
                password = "",
                confirmPassword = "",
                createdDate = user.createdDate,
                phoneNum = user.PhoneNumber,
                photo = null,
                status = user.status
            };
            return View(register);
        }
        [Authorize(Roles = "Player,Admin")]
        public async Task<IActionResult> playerDetails(string? id)
        {
            if (id == null || _context.user == null)
            {
                return NotFound();
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
          
            var playerData = _context.player.Where(x => x.userId == id).FirstOrDefault();
            if(playerData == null)
            {
                return NotFound();
            }
            playerViewModel player = new playerViewModel()
            { 
                name = user.name,
                userName = user.UserName,
                password = "",
                confirmPassword = "",
                createdDate = user.createdDate,
                phoneNum = user.PhoneNumber,  
                points = playerData.points
            };
            ViewBag.photo = playerData.photo;
            ViewBag.id = id;

            return View(player);
        }
        [HttpGet]
        [Authorize(Roles = "Player")]
        public async Task<IActionResult> ChooseMatch()
        {
            var current_user = await UserManager.GetUserAsync(User);
            var costs = _context.entry.ToArray();
            var player = _context.player.Where(x => x.userId == current_user.Id).FirstOrDefault();
            List<int> allCosts = new List<int>();
            foreach (var entry in costs)
            {
                allCosts.Add(entry.price);
            }
            allCosts.Sort();
            ViewBag.costs = allCosts;
            ViewBag.Points = player.points;
            ViewBag.userId = current_user.Id;
            //ViewBag.typeId = HttpContext.Session.GetString("userTypeId");
            //ViewBag.userType = HttpContext.Session.GetString("userType");
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Player")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChooseMatch(matchViewModel match)
        {
            var current_user = await UserManager.GetUserAsync(User);
            var costs = _context.entry.ToArray();
            List<int> allCosts = new List<int>();
            foreach (var entry2 in costs)
            {
                allCosts.Add(entry2.price);
            }
            allCosts.Sort();
            ViewBag.costs = allCosts;


            string userId = current_user.Id;
            var entry = _context.entry.Where(n => n.price == match.matchCost).FirstOrDefault();
            if (current_user == null || (entry == null && match.customPrice == null))
            {
                ModelState.AddModelError("customPrice", "Please Enter a number");//need to change
                return View();//need to change
            }
            if (match.customPrice < 20)
            {
                ModelState.AddModelError("customPrice", "Points need to be more than 20");
                return View();
            }

            var oldMatches = _context.matchHistory.Where(x => x.userId == userId).ToArray();
            var user = _context.player.Where(n => n.userId == userId).FirstOrDefault();
            matchHistory lastMatch = _context.matchHistory.Where(n => n.userId == userId && n.active == true).FirstOrDefault();
            if (lastMatch == null || lastMatch.createdDate.AddHours(24) <= DateTime.Now)
            {
                if (lastMatch != null)
                {
                    lastMatch.active = false;
                }
                Guid entryId = Guid.Empty;
                if (entry != null)
                {
                    entryId = entry.id;
                }
                if (match.isCustomPrice && match.customPrice == null)
                {
                    ModelState.AddModelError("morePoints", "Please Enter a number");
                    return View(match);
                }
                matchHistory matchHistory = new matchHistory()
                {
                    id = Guid.NewGuid(),
                    userId = userId,
                    entryId = match.isCustomPrice == true ? null : entryId,
                    createdDate = DateTime.Now,
                    active = true,
                    withPoints = match.withPoints,
                    customPrice = match.isCustomPrice == true ? match.customPrice : null
                };
                _context.matchHistory.Add(matchHistory);
                if (match.withPoints)
                {
                    if (oldMatches.Count() <= 4)
                    {
                        ModelState.AddModelError("matchCost", "Cannot use Points unless you play 4 matches");
                        ViewBag.Points = user.points;
                        return View();
                    }
                    if (match.isCustomPrice)
                    {
                        if (user.points < match.customPrice)
                        {
                            ModelState.AddModelError("customPrice", "You don't have enough points");
                            ViewBag.Points = user.points;
                            return View();
                        }
                        user.points = user.points - (int)match.customPrice;
                    }
                    else
                    {
                        if (user.points < match.matchCost)
                        {
                            ModelState.AddModelError("matchCost", "You don't have enough points");
                            ViewBag.Points = user.points;
                            return View();
                        }
                        user.points = user.points - (int)match.matchCost;
                    }
                }
                else
                {
                    if (match.isCustomPrice)
                    {
                        user.points = user.points + (int)(match.customPrice * 4);
                    }
                    else
                    {
                        user.points = user.points + (int)(match.matchCost * 4);
                    }
                }

                ViewBag.Points = user.points;
                _context.player.Update(user);
                _context.SaveChanges();
                if (match.isCustomPrice == true) HttpContext.Session.SetString("customPrice", "1");
                else HttpContext.Session.SetString("customPrice", "0");
                if (match.withPoints) HttpContext.Session.SetString("withPoints", "1");
                else HttpContext.Session.SetString("withPoints", "0");
                //return Redirect("/Users/logOut");                
                return RedirectToAction("MatchTicket","Account");
           
            }
            var date = DateTime.Now - lastMatch.createdDate;
            int hours = 24 - date.Hours;
            ViewBag.Points = user.points;
            ModelState.AddModelError("matchCost", "You Have to wait "+hours+" hour(s) for your next match");
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Player")]
        public async Task<IActionResult> MatchTicket()
        {
            var user = await UserManager.GetUserAsync(User);
            var cost = _context.matchHistory.Where(x => x.userId == user.Id && x.active == true).FirstOrDefault();
            var entry = _context.entry.Where(x => x.id == cost.entryId).FirstOrDefault();
            ViewBag.points = "No";
            if (HttpContext.Session.GetString("withPoints") == "1")
            {
                ViewBag.points = "Yes";
            }
            else if (HttpContext.Session.GetString("withPoints") == "0")
            {
                ViewBag.points = "No";
            }
            if (HttpContext.Session.GetString("customPrice") == "1")
            {
                ViewBag.value = cost.customPrice;
            }
            else if (HttpContext.Session.GetString("customPrice") == "0")
            {
                ViewBag.value = entry.price;
            }
            else
            {
                return RedirectToAction("ChooseMatch","Account");
            }
            HttpContext.Session.SetString("Name", user.name.ToString());
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "Player")]
        public async Task<IActionResult> comment()
        {
            
            var user = await UserManager.GetUserAsync(User);
            //var userInGuid = new Guid(userid);
            var player = _context.player.Where(x => x.userId == user.Id).FirstOrDefault();
            ViewBag.id = user.Id;
            ViewBag.photo = player.photo;
            HttpContext.Session.SetString("Name", user.UserName.ToString());
            //HttpContext.Session.SetString("photo", (user.photo));
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Player")]
        public async Task<IActionResult> comment(string commentText)
        {

            //var userInGuid = new Guid(userid);
            var user = await UserManager.GetUserAsync(User);
            var player = _context.player.Where(x => x.userId == user.Id).FirstOrDefault();
            ViewBag.id = user.Id;
            ViewBag.photo = player.photo;            

            if (commentText == "" || commentText== null)
            {
                ModelState.AddModelError(string.Empty, "Comment cannot be empty");
                return View();
            }
                if (user == null)
                {
                    return RedirectToAction("login");

                }
                comments c = new comments()
                {
                    id = Guid.NewGuid(),
                    comment = commentText,
                    userId = user.Id
                };
                _context.comments.Add(c);
                _context.SaveChanges();
                return RedirectToAction("comment", "Account");
        }
        
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> readcomment()
        {
            var admin = await UserManager.GetUserAsync(User);
            //var c = _context.comments.Include(m => m.comment).ToList();
            
            List<comments> c = _context.comments.ToList();
            List<ViewCommentVM> comments = new List<ViewCommentVM>();
            foreach (var comment in c)
            {
                var player = await UserManager.FindByIdAsync(comment.userId);
                if(player.supervisorId==admin.Id)
                {
                    ViewCommentVM newcomment = new ViewCommentVM();
                    newcomment.name = player.name;
                    newcomment.comment = comment.comment;
                    newcomment.userId = comment.userId;
                    comments.Add(newcomment);  
                }

            }
            return View(comments);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> addPoints(string? id)
        {
            var user = await UserManager.FindByIdAsync(id);
            if(user == null)
            {
                return NotFound();
            }
            var player = _context.player.Where(x => x.userId == user.Id).FirstOrDefault();
            if (player == null)
            {
                return NotFound();
            }
            ViewBag.id = id;
            ViewBag.Name = user.name;
            ViewBag.photo = player.photo;
            ViewBag.points = player.points;
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> addPoints(string? id,int points)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var player = _context.player.Where(x => x.userId == user.Id).FirstOrDefault();
            if (player == null) return NotFound();
            player.points += points;
            try
            {
            _context.Update(player);
            _context.SaveChanges();
            }
            catch(Exception e)
            {
                throw e;
            }
            ViewBag.id = id;
            ViewBag.Name = user.name;
            ViewBag.photo = player.photo;
            ViewBag.points = player.points;
            return View();
        }
    }
}
