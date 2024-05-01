using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using VNW.Models;
//using VNW.ViewModels;//

namespace VNW.Common
{
    public class MySession : Controller
    {
        //::my api for session
        public bool SetMySession(string key, string val, Microsoft.AspNetCore.Http.ISession myIS)
        {
            try
            {
                if (myIS == null)
                    return false;

                //::string to byte[]
                byte[] bv = System.Text.Encoding.Default.GetBytes(val); ;
                //HttpContext.Session.Set(key, bv);
                myIS.Set(key, bv);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("exception " + ex.ToString());
                return false;
            }
            return true;
        }

        //::my api for session
        public string GetMySession(string key, Microsoft.AspNetCore.Http.ISession myIS)
        {
            string _str = null;
            try
            {
                if (myIS == null)
                    return null;

                byte[] bv = null;
                //HttpContext.Session.TryGetValue(key, out bv);
                myIS.TryGetValue(key, out bv);
                if (bv == null) return null;
                //::byte[] to string
                _str = System.Text.Encoding.Default.GetString(bv);
                //System.Diagnostics.Debug.WriteLine(" ss" + _str.Length);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("exception " + ex.ToString());
            }
            //System.Diagnostics.Debug.WriteLine("*Get My Session " + key + " : " + _str); //debug trace
            return _str;
        }

        ////::test cases
        //public string Test(string key)
        //{
        //    return "pass " + key;
        //}
        //public string Test2(Microsoft.AspNetCore.Http.ISession myIS)
        //{
        //    if (myIS != null)
        //        return "pass ";
        //    else
        //        return "fail";
        //}

        //::try to set Common function
        public bool LoginPrecheck(Microsoft.AspNetCore.Http.ISession myIS)
        {
            string UserAccount = GetMySession("UserAccount", myIS);
            string IsUserLogin = GetMySession("IsUserLogin", myIS);

            string UserLevel = GetMySession("UserLevel", myIS); 
            /*:: 1A (Admin), 2B(Vendor), 3C(Customer), null(Guest)*/

            //ViewBag.UserAccount = UserAccount;
            if (UserAccount == null || UserAccount == "" || IsUserLogin == "" || IsUserLogin == null)
            {
                return false;                
            }
            return true;
        }

        public bool CheckAdmin(Microsoft.AspNetCore.Http.ISession myIS)
        {
            string UserLevel = GetMySession("UserLevel", myIS);
            /*:: 1A (Admin), 2B(Vendor), 3C(Customer), null(Guest)*/
            //::pass case
            if (UserLevel == "1A")            
                return true;            
            else            
                return false;                     
        }

    }

}