﻿namespace Mango.Web.Utility
{
    public class SD
    {
        public static string CouponAPIBase {  get; set; }
        public static string ProductAPIBase {  get; set; }
        public static string AuthAPIBase {  get; set; }
        public static string ShoppingCartAPIBase {  get; set; }
        public static string OrderAPIBase { get; set; }
        public static string RoleAdmin {  get; set; } = "ADMIN";
        public static string RoleCustomer { get; set; } = "CUSTOMER";
        public static string TokenCookie { get; set; } = "JWTTOKEN";
        public enum ApiType
        {
            GET,
            POST,
            PUT,
            DELETE
        }
            
    }
}
