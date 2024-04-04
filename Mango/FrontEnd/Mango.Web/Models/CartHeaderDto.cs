﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models.Dto
{
    public class CartHeaderDto
    {
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public string? CouponCode { get; set; }
        public Double Discount { get; set; }
        public Double CartTotal { get; set; }
        public string? FirstName {  get; set; }
        public string? LastName {  get; set; }
        public string? Phone {  get; set; }
        public string? Email {  get; set; }
    }
}
