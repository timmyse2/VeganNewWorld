using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using System.Reflection; //for GetRuntimeField
//using System.ComponentModel; //for ?


//namespace VNW.Models
namespace VNW.Common
{
    //::copy from openai solution <May22, 2025>
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var enumType = enumValue.GetType();
            var member = enumType.GetMember(enumValue.ToString())[0];
            var attributes = member.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attributes.Length > 0)
            {
                var displayAttribute = (DisplayAttribute)attributes[0];
                return displayAttribute.Name;
            }

            return enumValue.ToString();
        }

        //::from ithelp 
        public static string ToDescription(this Enum value)
        {
            return value.GetType()
                .GetRuntimeField(value.ToString())
                .GetCustomAttributes<System.ComponentModel.DescriptionAttribute>()
                .FirstOrDefault()?.Description ?? string.Empty;
        }
    }

    public enum PayEnum
    {
        [Display(Name = "信用卡支付")]
        CreditCard = 0,
        [Display(Name = "ATM轉帳")]
        ATM = 1,
        [Display(Name = "貨到付款")]
        CashOnDelivery = 2,
        //[Display(Name = "點數")]
        //Point = 3,
        //[Display(Name = "其它")]
        //Other = 9,
    }

    public enum InvoiceEnum
    {
        [Display(Name = "捐贈")]
        Donate = 0,
        [Display(Name = "電子發票")]
        EInvoice = 1,
        [Display(Name = "公司戶電子發票")]
        CompanyEInvoicel = 2,        
    }

    public enum FoodEnum
    {
        [Display(Name = "馬卡龍")]
        Macaron = 0,
        [Display(Name = "美式鬆餅")]
        Pancake = 1,
        [Display(Name = "甜甜圈")]
        Donuts = 2,
        [Display(Name = "泡芙")]
        Puff = 3,
        [Display(Name = "聖代")]
        Sundae = 4,
        [Display(Name = "布朗尼")]
        Brownie = 5,
        [Display(Name = "提拉米蘇")]
        Tiramisu = 6,
        [Display(Name = "熔岩蛋糕")]
        LavaCake = 7,
        [Display(Name = "蛋塔")]
        CustardTart = 8,
        [Display(Name = "蜜糖吐司")]
        HoneyToast = 9,
    }
}
