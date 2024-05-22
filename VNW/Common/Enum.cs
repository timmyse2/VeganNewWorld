using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

//namespace VNW.Models
namespace VNW.Common
{
    //public class Enum
    //{
    //}
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
