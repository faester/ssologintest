# ssologintest
Example og integration with the JP/Politikens Hus login. This should not neccessarily be considered a best practice but exemplifies a simple implementation.


This is a vanilla exmpale of .NET mvc 4 with an added HttpModule handling the actual login process. 

All codes relevant to the example is located in /sso in the project folder. 

The module is injected in Global.asax.cs

The strategy in the example is to check for a cookie signifying if a login attempt has been made in each page load. If no attempt to login has been made an immediate request will be made against the sso (OpenID) url. 
If a login session exists at the OpenID provider the user will be logged in immediately. If not a cookie will be set to inform the relaying party that 
no further login attempts should be made unless the user triggeres it using the /login action. This latter action will create a full login with setup and login box.
