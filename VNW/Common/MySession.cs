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
                {
                    return false;
                }
                //::string to byte[]
                byte[] bv = System.Text.Encoding.Default.GetBytes(val); ;
                //HttpContext.Session.Set(key, bv);
                myIS.Set(key, bv);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write("exception " + ex.ToString());
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
                {
                    return null;
                }

                byte[] bv = null;
                //HttpContext.Session.TryGetValue(key, out bv);
                myIS.TryGetValue(key, out bv);
                //::byte[] to string
                _str = System.Text.Encoding.Default.GetString(bv);
                //System.Diagnostics.Debug.WriteLine(" ss" + _str.Length);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write("exception " + ex.ToString());
            }
            return _str;
        }

        //::test cases
        public string Test(string key)
        {
            return "pass " + key;
        }
        public string Test2(Microsoft.AspNetCore.Http.ISession myIS)
        {
            if (myIS != null)
                return "pass ";
            else
                return "fail";
        }

    }

}