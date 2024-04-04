﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartHeader
    {
        [Key]
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public string? CouponCode { get; set;}

        [NotMapped]
        public Double Discount { get; set; }
        [NotMapped]
        public Double CartTotal {  get; set; }
    }
}
