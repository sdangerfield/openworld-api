﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Facebook;
using NHibernate;
using norwold.model;
using DataModel.libDB;



namespace DataModel.libHosting
{
    public class sysFacebook
    {
        public virtual string ID { get; set; }
        public virtual string Gender { get; set; }
        public virtual string AccessToken { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string Surname { get; set; }
        public virtual string Email { get; set; }
        public virtual bool Verified { get; set; }
        public virtual DateTime Expires { get; set; }

        protected FacebookClient fb;

        public sysFacebook()
        {
            
        }

        public virtual bool attachProfile(hostUser user)
        {
               
            // if Email exists but not match then fail and handle in caller
            if (user.Email != null)
            {
                if (user.Email != this.Email) { 
                    return false; 
                }
            }
            user.fbAccessToken = this.AccessToken;
            user.fbFirstName = this.FirstName;
            user.fbSurname = this.Surname;
            user.fbGender = this.Gender;
            user.fbID = this.ID;
            return true;
        }

        public virtual string LongDesc()
        {
            string res;
            res = "ID:" + this.ID;
            res = res + "\nAccessToken:" + this.AccessToken;
            res = res + "\nFirst+Surname" + this.FirstName + " " + this.Surname;
            res = res + "\nEmail:" + this.Email;
            res = res + "\nGender:" + this.Gender;
            res = res + "\nVerified:" + this.Verified.ToString();
            res = res + "\nTokenExpires:" + this.Expires.ToLocalTime(); 
            return res;
        }

        public virtual bool TokenExpired()
        {
            return (DateTime.Compare(this.Expires,DateTime.Now)>0);
        }

        public virtual bool PostToWall(string msg,string caption,string description,string name)
        {
            //Fail if expired
            if (this.TokenExpired())
            {
                Debug.Print("Post to Wall failed for:" + this.FirstName + " " + this.Surname+"\n TokenExpired"+this.Expires.ToLocalTime());
                return false;
            }
            //Post to facebook
            var args = new Dictionary<string, object>();
            args["message"] = msg;
            args["caption"] = caption;
            args["description"] = description;
            args["name"] = name;
            args["picture"] = hostConstants.iconURI;
            args["link"] = hostConstants.redirectURI;
            fb = new FacebookClient();
            fb.AccessToken = this.AccessToken;
            fb.Post("/me/feed", args);
            return true;
        }


        //STATIC Methods
        public static sysFacebook ParseResponse(IDictionary<string, object> response, FacebookOAuthResult authResult) 
        {
            if (!authResult.IsSuccess) { return null; }
            sysFacebook result = new sysFacebook();
            result.Verified = (bool)response["verified"];
            result.AccessToken = authResult.AccessToken;
            result.FirstName= (string)response["first_name"];
            result.Surname = (string)response["last_name"];
            result.Gender = (string)response["gender"];
            result.Email = (string)response["email"];
            result.ID = (string)response["id"];
            result.Expires = authResult.Expires;
            return result;
        }

        public static bool PostToAppWall(string msg,string caption,string description,string name)
        {
            //Post to facebook
            //Need ManagePages on Admin ArgumentOutOfRangeException Openworld
            //Then Get"/me/accounts"
            //Then find Openworld Page
            //Then store that token
            var args = new Dictionary<string, object>();
            args["message"] = msg;
            args["caption"] = caption;
            args["description"] = description;
            args["name"] = name;
            args["picture"] = hostConstants.iconURI;
            args["link"] = hostConstants.redirectURI;
            var fb = new FacebookClient();
            fb.AccessToken = hostConstants.appToken;
            fb.Post("/me/feed", args);
            return true;
            
        }

        public static hostUser UserExists(ISession sess,sysFacebook fbRecord)
        {
            return sess.QueryOver<hostUser>().Where(x => x.fbID == fbRecord.ID).SingleOrDefault();

        }

        public static hostUser FindByUserName(ISession sess, string user)
        {
            return sess.QueryOver<hostUser>().Where(x => x.LongName == user).SingleOrDefault();
        }
    
    }

}




