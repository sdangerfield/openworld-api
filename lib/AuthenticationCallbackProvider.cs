﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using Nancy.SimpleAuthentication;
using DataModel.libHosting;


namespace OpenworldAPI.nancyfx.lib
{
    public class AuthenticationCallbackProvider : IAuthenticationCallbackProvider
    {
        public dynamic Process(NancyModule nancyModule, AuthenticateCallbackData model)
        {
            /** Logic - if FBiD Exists - login, else register and login

            if (hostFacebook.UserExistsStr(nancyModule.Session, model.AuthenticatedClient.UserInformation.Id)) {
                return nancyModule.Negotiate.WithView("owUser").WithModel(model);
            }
            **/



            return nancyModule.Negotiate.WithView("AuthenticateCallback").WithModel(model);
        }

        public dynamic OnRedirectToAuthenticationProviderError(NancyModule nancyModule, string errorMessage)
        {
            throw new System.NotImplementedException(); // Provider canceled auth or it failed for some reason e. g. user canceled it
        }
    }
}